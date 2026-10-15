using Microsoft.EntityFrameworkCore;
using Traindle.Data;

namespace Traindle.Services;

public class DailyGameService
{

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
        var puzzle = await FindPuzzleAsync(game, date);
        if (puzzle == null)
            return null;

        // Numbered by days since the first puzzle, so it's stable and skipped days still count
        var first = await _db.DailyPuzzles.Where(p => p.Game == game).MinAsync(p => p.Date);
        return new DailyPuzzleResponse(date, date.DayNumber - first.DayNumber + 1);
    }

    public async Task<SystemGuessResponse?> GuessSystemAsync(DateOnly date, int guessId)
    {
        var puzzle = await FindPuzzleAsync(GameKind.Systems, date);
        if (puzzle == null)
            return null;

        var answer = await _db.GameSystems.FindAsync(puzzle.TargetId);
        if (answer == null)
            return null;

        return await _gameData.GuessSystemAsync(guessId, answer);
    }

    public async Task<LineGuessResponse?> GuessLineAsync(DateOnly date, int guessId)
    {
        var puzzle = await FindPuzzleAsync(GameKind.Lines, date);
        if (puzzle == null)
            return null;

        var answer = await _db.GameLines.FindAsync(puzzle.TargetId);
        if (answer == null)
            return null;

        return await _gameData.GuessLineAsync(guessId, answer);
    }

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

    // Puzzles are generated a few days ahead, so refuse dates no timezone (max UTC+14) has reached yet
    private async Task<DailyPuzzle?> FindPuzzleAsync(GameKind game, DateOnly date)
    {
        if (date > DateOnly.FromDateTime(DateTime.UtcNow.AddHours(14)))
            return null;

        return await _db.DailyPuzzles.AsNoTracking().FirstOrDefaultAsync(p => p.Game == game && p.Date == date);
    }
}
