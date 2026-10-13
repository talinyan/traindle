using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Traindle.Data;

namespace Traindle.Services;

public partial class GameDataService
{
    [GeneratedRegex(@"\s*\(\s*[-–]?\s*\d{4}[^)]*\)\s*$")]
    private static partial Regex YearSuffix();

    private readonly PostgresContext _db;
    private readonly ILogger<GameDataService> _logger;

    public GameDataService(PostgresContext db, ILogger<GameDataService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Rebuilds game_systems and game_lines from the raw Citylines tables, keeping only what is open this year.
    // Citylines uses 0/-1/null for an unknown opening year and 999999 for "never closed".
    public async Task<(int Systems, int Lines)> ProcessAsync()
    {
        var year = DateTime.UtcNow.Year;

        var cities = await _db.Cities.AsNoTracking().ToDictionaryAsync(c => c.Id);
        var modes = await _db.TransportModes.AsNoTracking().ToDictionaryAsync(m => m.Id, m => m.Name ?? "default");
        var systems = await _db.Systems.AsNoTracking().Where(s => !s.Historic).ToDictionaryAsync(s => s.Id);
        var lines = await _db.Lines.AsNoTracking().ToListAsync();

        var openStations = await _db.Stations.AsNoTracking()
            .Where(s => s.Opening > 0 && s.Opening <= year && (s.Closure == null || s.Closure > year))
            .ToDictionaryAsync(s => s.Id, s => s.Opening!.Value);

        var openSections = await _db.Sections.AsNoTracking()
            .Where(s => s.Opening > 0 && s.Opening <= year && (s.Closure == null || s.Closure > year))
            .Select(s => new { s.Id, s.Length, Opening = s.Opening!.Value, s.Geometry })
            .ToDictionaryAsync(s => s.Id);

        // line id -> its open stations / sections, only counting links active this year
        var stationsByLine = (await _db.StationLines.AsNoTracking()
                .Where(sl => (sl.FromYear == null || sl.FromYear <= year) && (sl.ToYear == null || sl.ToYear > year))
                .Select(sl => new { sl.LineId, sl.StationId })
                .ToListAsync())
            .Where(sl => openStations.ContainsKey(sl.StationId))
            .GroupBy(sl => sl.LineId)
            .ToDictionary(g => g.Key, g => g.Select(sl => sl.StationId).ToHashSet());

        var sectionsByLine = (await _db.SectionLines.AsNoTracking()
                .Where(sl => (sl.FromYear == null || sl.FromYear <= year) && (sl.ToYear == null || sl.ToYear > year))
                .Select(sl => new { sl.LineId, sl.SectionId })
                .ToListAsync())
            .Where(sl => openSections.ContainsKey(sl.SectionId))
            .GroupBy(sl => sl.LineId)
            .ToDictionary(g => g.Key, g => g.Select(sl => sl.SectionId).ToHashSet());

        var gameLines = new List<GameLine>();
        // system id -> its lines with open track this year, whether or not Citylines has their stations
        var openLineCounts = new Dictionary<int, int>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.Name) || !systems.ContainsKey(line.SystemId))
                continue;
            if (!sectionsByLine.TryGetValue(line.Id, out var sectionIds))
                continue;
            openLineCounts[line.SystemId] = openLineCounts.GetValueOrDefault(line.SystemId) + 1;
            if (!stationsByLine.TryGetValue(line.Id, out var stationIds) || stationIds.Count < 2)
                continue;

            var sections = sectionIds.Select(id => openSections[id]).ToList();
            gameLines.Add(new GameLine
            {
                Id = line.Id,
                SystemId = line.SystemId,
                Name = LineName(line.Name),
                Colour = line.Color,
                Mode = modes.GetValueOrDefault(line.TransportModeId, "default"),
                StationCount = stationIds.Count,
                Length = sections.Sum(s => s.Length),
                OpeningYear = stationIds.Select(id => openStations[id]).Concat(sections.Select(s => s.Opening)).Min(),
                Geometry = ToMultiLineString(sections.Select(s => s.Geometry)),
            });
        }

        var gameSystems = new List<GameSystem>();
        var unmappedCountries = new HashSet<string>();
        var thinSystems = 0;
        foreach (var group in gameLines.GroupBy(l => l.SystemId))
        {
            // one entry per (line, station), so a station appearing more than once is an interchange
            var stationIds = group.SelectMany(l => stationsByLine[l.Id]).ToList();
            var distinctStations = stationIds.Distinct().Count();
            if (distinctStations < 3)
                continue;
            // Citylines is missing stations for most of this system's open lines, so it would be misrepresented
            if (group.Count() * 2 < openLineCounts[group.Key])
            {
                thinSystems++;
                continue;
            }

            var system = systems[group.Key];
            var city = cities[system.CityId];
            var (lat, lng) = ParsePoint(city.Coords);
            var (country, continent) = CountryInfo.Resolve(city.Country);
            if (continent == null && country != null && unmappedCountries.Add(country))
                _logger.LogWarning("No continent mapped for country {Country}", country);

            gameSystems.Add(new GameSystem
            {
                Id = system.Id,
                Name = string.IsNullOrWhiteSpace(system.Name) ? city.Name ?? "" : system.Name,
                CityId = city.Id,
                City = city.Name ?? "",
                Country = country,
                CountryState = string.IsNullOrWhiteSpace(city.CountryState) ? null : city.CountryState,
                Continent = continent,
                Latitude = lat,
                Longitude = lng,
                Modes = group.Select(l => l.Mode).Distinct().Order().ToList(),
                LineCount = group.Count(),
                StationCount = distinctStations,
                InterchangeCount = stationIds.GroupBy(id => id).Count(g => g.Count() > 1),
                // sections shared by several lines only count once
                Length = group.SelectMany(l => sectionsByLine[l.Id]).Distinct().Sum(id => openSections[id].Length),
                OpeningYear = group.Min(l => l.OpeningYear),
            });
        }

        if (thinSystems > 0)
            _logger.LogInformation("Skipped {Count} systems missing stations for most of their lines", thinSystems);

        var keptSystems = gameSystems.Select(s => s.Id).ToHashSet();
        gameLines.RemoveAll(l => !keptSystems.Contains(l.SystemId));

        await using var transaction = await _db.Database.BeginTransactionAsync();

        await _db.GameLines.ExecuteDeleteAsync();
        await _db.GameSystems.ExecuteDeleteAsync();
        _db.GameSystems.AddRange(gameSystems);
        _db.GameLines.AddRange(gameLines);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        _logger.LogInformation("Processed {Systems} game systems and {Lines} game lines", gameSystems.Count, gameLines.Count);
        return (gameSystems.Count, gameLines.Count);
    }

    // Drops a trailing year range like "(2017-2025)", "(-2025)" or "(2016-Present)",
    // then bare identifiers like "1" or "A" -> "Line 1" / "Line A"
    private static string LineName(string name)
    {
        name = YearSuffix().Replace(name, "").Trim();
        return name.All(char.IsAsciiDigit) || (name.Length == 1 && char.IsLetter(name[0]))
            ? $"Line {name}"
            : name;
    }

    // "LINESTRING(x y, ...)" pieces -> "MULTILINESTRING((x y, ...), ...)"
    private static string ToMultiLineString(IEnumerable<string?> lineStrings)
    {
        var parts = lineStrings
            .OfType<string>()
            .Where(g => g.Contains('(') && !g.Contains("EMPTY"))
            .Select(g => g[g.IndexOf('(')..]);
        return $"MULTILINESTRING({string.Join(",", parts)})";
    }

    // "POINT(lng lat)" -> (lat, lng)
    private static (double? Lat, double? Lng) ParsePoint(string? wkt)
    {
        if (string.IsNullOrWhiteSpace(wkt) || !wkt.StartsWith("POINT("))
            return (null, null);

        var coords = wkt["POINT(".Length..^1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (coords.Length < 2
            || !double.TryParse(coords[0], CultureInfo.InvariantCulture, out var lng)
            || !double.TryParse(coords[1], CultureInfo.InvariantCulture, out var lat))
            return (null, null);

        return (lat, lng);
    }

    // Projected into response records: avoids the System <-> Lines cycle and leaves line geometry out of the system list
    private static readonly Expression<Func<GameSystem, GameSystemResponse>> ToSystemResponse = s =>
        new GameSystemResponse(
            s.Id, s.Name, s.City, s.Country, s.CountryState, s.Continent,
            s.Latitude, s.Longitude, s.Modes,
            s.LineCount, s.StationCount, s.InterchangeCount, s.Length, s.OpeningYear,
            s.Lines.OrderBy(l => l.Name)
                .Select(l => new GameLineSummary(l.Id, l.Name, l.Colour, l.Mode))
                .ToList());

    private static readonly Expression<Func<GameLine, GameLineResponse>> ToLineResponse = l =>
        new GameLineResponse(
            l.Id, l.Name, l.Colour, l.Mode,
            l.StationCount, l.Length, l.OpeningYear, l.Geometry,
            l.SystemId,
            new GameSystemSummary(l.System!.Id, l.System.Name, l.System.City, l.System.Country, l.System.Continent));

    public Task<List<GameSystemOption>> GetGameSystemOptionsAsync() =>
        _db.GameSystems
            .OrderBy(s => s.Name)
            .Select(s => new GameSystemOption(
                s.Id, s.Name, s.City, s.Country, s.Continent,
                s.Modes, s.LineCount, s.StationCount, s.Length, s.OpeningYear))
            .ToListAsync();

    public Task<List<GameLineOption>> GetGameLineOptionsAsync() =>
        _db.GameLines
            .OrderBy(l => l.Name)
            .Select(l => new GameLineOption(
                l.Id, l.Name, l.Colour, l.Mode, l.StationCount, l.Length, l.OpeningYear,
                l.SystemId, l.System!.Name, l.System.City, l.System.Country))
            .ToListAsync();

    public Task<GameSystemResponse?> GetGameSystemAsync(int id) =>
        _db.GameSystems.Where(s => s.Id == id).Select(ToSystemResponse).FirstOrDefaultAsync();

    public Task<GameLineResponse?> GetGameLineAsync(int id) =>
        _db.GameLines.Where(l => l.Id == id).Select(ToLineResponse).FirstOrDefaultAsync();
}
