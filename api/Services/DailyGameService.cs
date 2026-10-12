using Microsoft.EntityFrameworkCore;
using Traindle.Data;

namespace Traindle.Services;

public class DailyGameService
{
    private const double EarthRadiusKm = 6371;

    private readonly PostgresContext _db;
    private readonly GameDataService _gameData;
    private readonly ILogger<DailyGameService> _logger;

    public DailyGameService(PostgresContext db, GameDataService gameData, ILogger<DailyGameService> logger)
    {
        _db = db;
        _gameData = gameData;
        _logger = logger;
    }

    public async Task<DailyPuzzleResponse?> GetDailyAsync(GameKind game, DateOnly date)
    {
        var puzzle = await GetOrCreatePuzzleAsync(game, date);
        if (puzzle == null)
            return null;

        // Numbered by days since the first puzzle, so it's stable and skipped days still count
        var first = await _db.DailyPuzzles.Where(p => p.Game == game).MinAsync(p => p.Date);
        return new DailyPuzzleResponse(date, date.DayNumber - first.DayNumber + 1);
    }

    public async Task<SystemGuessResponse?> GuessSystemAsync(DateOnly date, int guessId)
    {
        var puzzle = await GetOrCreatePuzzleAsync(GameKind.Systems, date);
        if (puzzle == null)
            return null;

        var guess = await _db.GameSystems.AsNoTracking().FirstOrDefaultAsync(s => s.Id == guessId);
        var answer = await FindAnswerAsync(_db.GameSystems, puzzle);
        if (guess == null || answer == null)
            return null;

        var (distance, bearing) = DistanceAndBearing(guess, answer);
        var correctModes = guess.Modes.Intersect(answer.Modes).Order().ToList();
        var modeMatch = correctModes.Count == 0 ? ModeMatch.None
            : guess.Modes.ToHashSet().SetEquals(answer.Modes) ? ModeMatch.Exact
            : ModeMatch.Partial;

        return new SystemGuessResponse(
            guess.Id, guess.Id == answer.Id,
            Compare(answer.LineCount, guess.LineCount), distance, bearing,
            Compare(answer.OpeningYear, guess.OpeningYear),
            Compare(answer.Length, guess.Length),
            Compare(answer.StationCount, guess.StationCount),
            modeMatch, correctModes);
    }

    public async Task<LineGuessResponse?> GuessLineAsync(DateOnly date, int guessId)
    {
        var puzzle = await GetOrCreatePuzzleAsync(GameKind.Lines, date);
        if (puzzle == null)
            return null;

        var guess = await _db.GameLines.AsNoTracking().FirstOrDefaultAsync(l => l.Id == guessId);
        var answer = await FindAnswerAsync(_db.GameLines, puzzle);
        if (guess == null || answer == null)
            return null;

        // Lines have no location of their own, so measure between their systems' cities
        var systems = await _db.GameSystems.AsNoTracking()
            .Where(s => s.Id == guess.SystemId || s.Id == answer.SystemId)
            .ToDictionaryAsync(s => s.Id);
        var (distance, bearing) = systems.TryGetValue(guess.SystemId, out var guessSystem)
                                  && systems.TryGetValue(answer.SystemId, out var answerSystem)
            ? DistanceAndBearing(guessSystem, answerSystem)
            : (null, null);

        return new LineGuessResponse(
            guess.Id, guess.Id == answer.Id,
            distance, bearing,
            guess.Mode == answer.Mode,
            Compare(answer.Length, guess.Length),
            Compare(answer.StationCount, guess.StationCount));
    }

    // Only reveals puzzles that already exist, so it can't be used to peek at future days
    public async Task<GameSystemResponse?> RevealSystemAsync(DateOnly date)
    {
        var puzzle = await FindPuzzleAsync(GameKind.Systems, date);
        return puzzle == null ? null : await _gameData.GetGameSystemAsync(puzzle.TargetId);
    }

    public async Task<GameLineResponse?> RevealLineAsync(DateOnly date)
    {
        var puzzle = await FindPuzzleAsync(GameKind.Lines, date);
        return puzzle == null ? null : await _gameData.GetGameLineAsync(puzzle.TargetId);
    }

    // Time zones run from UTC-12 to UTC+14, so a player's local date is within a day of UTC
    private static bool IsPlayableDate(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return date >= today.AddDays(-1) && date <= today.AddDays(1);
    }

    private Task<DailyPuzzle?> FindPuzzleAsync(GameKind game, DateOnly date) =>
        _db.DailyPuzzles.AsNoTracking().FirstOrDefaultAsync(p => p.Game == game && p.Date == date);

    // Past puzzles stay playable; new ones are only created for today (in some time zone)
    private async Task<DailyPuzzle?> GetOrCreatePuzzleAsync(GameKind game, DateOnly date)
    {
        var puzzle = await FindPuzzleAsync(game, date);
        if (puzzle != null || !IsPlayableDate(date))
            return puzzle;

        var candidates = game == GameKind.Systems
            ? await _db.GameSystems.Select(s => s.Id).ToListAsync()
            : await _db.GameLines.Select(l => l.Id).ToListAsync();
        if (candidates.Count == 0)
        {
            _logger.LogError("No game data to pick a {Game} puzzle from; run `process` first", game);
            return null;
        }

        // Avoid repeats until everything has been an answer once
        var used = await _db.DailyPuzzles.Where(p => p.Game == game).Select(p => p.TargetId).ToListAsync();
        var fresh = candidates.Except(used).ToList();
        var pool = fresh.Count > 0 ? fresh : candidates;

        puzzle = new DailyPuzzle { Date = date, Game = game, TargetId = pool[Random.Shared.Next(pool.Count)] };
        _db.DailyPuzzles.Add(puzzle);
        try
        {
            await _db.SaveChangesAsync();
            _logger.LogInformation("Created {Game} puzzle for {Date}", game, date);
            return puzzle;
        }
        catch (DbUpdateException)
        {
            // A concurrent request created this day's puzzle first; use theirs
            _db.ChangeTracker.Clear();
            var existing = await FindPuzzleAsync(game, date);
            if (existing == null)
                throw;
            return existing;
        }
    }

    private async Task<T?> FindAnswerAsync<T>(DbSet<T> set, DailyPuzzle puzzle) where T : class
    {
        var answer = await set.FindAsync(puzzle.TargetId);
        if (answer == null)
            _logger.LogError(
                $"Failed to find puzzle target for game {puzzle.Game} with id {puzzle.TargetId} from date {puzzle.Date}"
            );
        return answer;
    }

    private static Comparison Compare(int answer, int guess) =>
        answer > guess ? Comparison.Higher : answer < guess ? Comparison.Lower : Comparison.Equal;

    // Rounded km and degrees between two systems' cities; nulls when either has no coordinates
    private static (double? Km, double? Bearing) DistanceAndBearing(GameSystem from, GameSystem to)
    {
        if (from.Latitude is not { } fLat || from.Longitude is not { } fLng
            || to.Latitude is not { } tLat || to.Longitude is not { } tLng)
            return (null, null);

        var (km, bearing) = DistanceAndBearing(fLat, fLng, tLat, tLng);
        return (Math.Round(km), Math.Round(bearing));
    }

    // Great-circle distance and initial bearing (degrees clockwise from north) from the guess to the answer
    private static (double Km, double Bearing) DistanceAndBearing(double lat1, double lng1, double lat2, double lng2)
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

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
