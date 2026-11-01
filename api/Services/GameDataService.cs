using Microsoft.EntityFrameworkCore;
using Traindle.Data;

namespace Traindle.Services;

public partial class GameDataService
{


  private readonly PostgresContext _db;
  private readonly ILogger<GameDataService> _logger;

  public GameDataService(PostgresContext db, ILogger<GameDataService> logger)
  {
    _db = db;
    _logger = logger;
  }

  // Rebuilds game_systems and game_lines from the raw Citylines tables, keeping only what is open this year.
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
        Name = GameExtensions.LineName(line.Name),
        Colour = line.Color,
        Mode = modes.GetValueOrDefault(line.TransportModeId, "default"),
        StationCount = stationIds.Count,
        Length = sections.Sum(s => s.Length),
        OpeningYear = stationIds.Select(id => openStations[id]).Concat(sections.Select(s => s.Opening)).Min(),
        Geometry = GameExtensions.ToMultiLineString(sections.Select(s => s.Geometry)),
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
      var (lat, lng) = GameExtensions.ParsePoint(city.Coords);
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

  public Task<List<GameSystemOption>> GetGameSystemOptionsAsync(Continent? continent) =>
    _db.GameSystems
      .OrderBy(s => s.Name)
      .WhereContinent(continent)
      .Select(s => new GameSystemOption(
          s.Id, s.Name, s.City, s.Country, s.Continent,
          s.Modes, s.LineCount, s.StationCount, s.Length, s.OpeningYear))
      .ToListAsync();

  public Task<List<GameLineOption>> GetGameLineOptionsAsync(Continent? continent) =>
    _db.GameLines
      .OrderBy(l => l.Name)
      .WhereContinent(continent)
      .Select(l => new GameLineOption(
          l.Id, l.Name, l.Colour, l.Mode, l.StationCount, l.Length, l.OpeningYear,
          l.SystemId, l.System!.Name, l.System.City, l.System.Country, l.System.Continent))
      .ToListAsync();

  public Task<GameSystemResponse?> GetGameSystemAsync(int id) =>
      _db.GameSystems.Where(s => s.Id == id).Select(GameExtensions.ToSystemResponse).FirstOrDefaultAsync();

  public Task<GameLineResponse?> GetGameLineAsync(int id) =>
      _db.GameLines.Where(l => l.Id == id).Select(GameExtensions.ToLineResponse).FirstOrDefaultAsync();

  public async Task<SystemGuessResponse?> GuessSystemAsync(int guessId, GameSystem answer)
  {
    var guess = await _db.GameSystems.AsNoTracking().FirstOrDefaultAsync(l => l.Id == guessId);
    if (guess == null)
      return null;

    var (distance, bearing) = GameExtensions.DistanceAndBearing(guess, answer);
    var correctModes = guess.Modes.Intersect(answer.Modes).Order().ToList();
    var modeMatch = correctModes.Count == 0 ? ModeMatch.None
        : guess.Modes.ToHashSet().SetEquals(answer.Modes) ? ModeMatch.Exact
        : ModeMatch.Partial;

    return new SystemGuessResponse(
        guess.Id, guess.Id == answer.Id,
        GameExtensions.Compare(answer.LineCount, guess.LineCount), distance, bearing,
        GameExtensions.Compare(answer.OpeningYear, guess.OpeningYear),
        GameExtensions.Compare(answer.Length, guess.Length),
        GameExtensions.Compare(answer.StationCount, guess.StationCount),
        modeMatch, correctModes);
  }

  public async Task<LineGuessResponse?> GuessLineAsync(int guessId, GameLine answer)
  {
    var guess = await _db.GameLines.AsNoTracking().FirstOrDefaultAsync(l => l.Id == guessId);
    if (guess == null)
      return null;

    // Lines have no location of their own, so measure between their systems' cities
    var systems = await _db.GameSystems.AsNoTracking()
        .Where(s => s.Id == guess.SystemId || s.Id == answer.SystemId)
        .ToDictionaryAsync(s => s.Id);

    var (distance, bearing) = systems.TryGetValue(guess.SystemId, out var guessSystem)
                              && systems.TryGetValue(answer.SystemId, out var answerSystem)
        ? GameExtensions.DistanceAndBearing(guessSystem, answerSystem)
        : (null, null);

    return new LineGuessResponse(
        guess.Id, guess.Id == answer.Id,
        distance, bearing,
        guess.Mode == answer.Mode,
        GameExtensions.Compare(answer.OpeningYear, guess.OpeningYear),
        GameExtensions.Compare(answer.Length, guess.Length),
        GameExtensions.Compare(answer.StationCount, guess.StationCount));
  }
}
