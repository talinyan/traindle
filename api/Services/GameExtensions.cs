using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Traindle.Data;

namespace Traindle.Services;

public static partial class GameExtensions
{
    [GeneratedRegex(@"\s*\(\s*[-–]?\s*\d{4}[^)]*\)\s*$")]
    public static partial Regex YearSuffix();

    private const double EarthRadiusKm = 6371;

    public static Comparison Compare(int answer, int guess) =>
        answer > guess ? Comparison.Higher : answer < guess ? Comparison.Lower : Comparison.Equal;

    // Rounded km and degrees between two systems' cities; nulls when either has no coordinates
    public static (double? Km, double? Bearing) DistanceAndBearing(GameSystem from, GameSystem to)
    {
        if (from.Latitude is not { } fLat || from.Longitude is not { } fLng
            || to.Latitude is not { } tLat || to.Longitude is not { } tLng)
            return (null, null);

        var (km, bearing) = DistanceAndBearing(fLat, fLng, tLat, tLng);
        return (Math.Round(km), Math.Round(bearing));
    }

    // Great-circle distance and initial bearing (degrees clockwise from north) from the guess to the answer
    public static (double Km, double Bearing) DistanceAndBearing(double lat1, double lng1, double lat2, double lng2)
    {
        double phi1 = ToRadians(lat1), phi2 = ToRadians(lat2);
        double dPhi = ToRadians(lat2 - lat1), dLambda = ToRadians(lng2 - lng1);

        var a = Math.Pow(Math.Sin(dPhi / 2), 2)
                + Math.Cos(phi1) * Math.Cos(phi2) * Math.Pow(Math.Sin(dLambda / 2), 2);
        var km = 2 * EarthRadiusKm * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var y = Math.Sin(dLambda) * Math.Cos(phi2);
        var x = Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(dLambda);
        var bearing = (Math.Atan2(y, x) * 180 / Math.PI + 360) % 360;

        return (km, bearing);
    }

    public static double ToRadians(double degrees) => degrees * Math.PI / 180;

    // Drops a trailing year range like "(2017-2025)", "(-2025)" or "(2016-Present)",
    // then bare identifiers like "1" or "A" -> "Line 1" / "Line A"
    public static string LineName(string name)
    {
        name = YearSuffix().Replace(name, "").Trim();

        return name.All(char.IsAsciiDigit)
            ? $"Line {name}"
            : name.Length == 1 && char.IsLetter(name[0])
                ? $"{name} Line"
                : name;
    }

    // "LINESTRING(x y, ...)" pieces -> "MULTILINESTRING((x y, ...), ...)"
    public static string ToMultiLineString(IEnumerable<string?> lineStrings)
    {
        var parts = lineStrings
            .OfType<string>()
            .Where(g => g.Contains('(') && !g.Contains("EMPTY"))
            .Select(g => g[g.IndexOf('(')..]);
        return $"MULTILINESTRING({string.Join(",", parts)})";
    }

    // "POINT(lng lat)" -> (lat, lng)
    public static (double? Lat, double? Lng) ParsePoint(string? wkt)
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

    public static Task<int?> PickRandomTargetIdAsync(
        this PostgresContext db, GameKind game, IReadOnlyCollection<int> exclude, CancellationToken ct = default) =>
        game == GameKind.Systems
            ? db.GameSystems
                .Where(s => !exclude.Contains(s.Id))
                .OrderBy(_ => EF.Functions.Random())
                .Select(s => (int?)s.Id)
                .FirstOrDefaultAsync(ct)
            : db.GameLines
                .Where(l => !exclude.Contains(l.Id))
                .OrderBy(_ => EF.Functions.Random())
                .Select(l => (int?)l.Id)
                .FirstOrDefaultAsync(ct);

    public static readonly Expression<Func<GameSystem, GameSystemResponse>> ToSystemResponse = s =>
        new GameSystemResponse(
            s.Id, s.Name, s.City, s.Country, s.CountryState, s.Continent,
            s.Latitude, s.Longitude, s.Modes,
            s.LineCount, s.StationCount, s.InterchangeCount, s.Length, s.OpeningYear,
            s.Lines.OrderBy(l => l.Name)
                .Select(l => new GameLineSummary(l.Id, l.Name, l.Colour, l.Mode))
                .ToList());

    public static readonly Expression<Func<GameLine, GameLineResponse>> ToLineResponse = l =>
        new GameLineResponse(
            l.Id, l.Name, l.Colour, l.Mode,
            l.StationCount, l.Length, l.OpeningYear, l.Geometry,
            l.SystemId,
            new GameSystemSummary(l.System!.Id, l.System.Name, l.System.City, l.System.Country, l.System.Continent));
}