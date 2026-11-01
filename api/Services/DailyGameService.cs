using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Traindle.Data;

namespace Traindle.Services;

public class DailyGameService
{
  private readonly PostgresContext _db;
  private readonly GameDataService _gameData;
  private readonly TrackingService _tracking;

  public DailyGameService(PostgresContext db, GameDataService gameData, TrackingService tracking)
  {
    _db = db;
    _gameData = gameData;
    _tracking = tracking;
  }

  public async Task<DailyPuzzleResponse?> GetDailyAsync(GameKind game, DateOnly date, Continent? continent)
  {
    var puzzle = await FindPuzzleAsync(game, date, continent);
    if (puzzle == null)
      return null;

    // Numbered by days since the first puzzle, so it's stable and skipped days still count
    var first = await _db.DailyPuzzles.Where(p => p.Game == game).MinAsync(p => p.Date);
    var daily = new DailyPuzzleResponse(date, date.DayNumber - first.DayNumber + 1);

    await _tracking.FetchAsync(game, date, continent);
    return daily;
  }

  public async Task<SystemGuessResponse?> GuessSystemAsync(DateOnly date, Continent? continent, int guessId)
  {
    var puzzle = await FindPuzzleAsync(GameKind.Systems, date, continent);
    if (puzzle == null)
      return null;

    var answer = await _db.GameSystems.FindAsync(puzzle.TargetId);
    if (answer == null)
    {
      await _tracking.ErrorAsync(nameof(GuessSystemAsync),
          new { reason = "AnswerNotFound", game = GameKind.Systems, date, targetId = puzzle.TargetId });
      return null;
    }

    var guess = await _gameData.GuessSystemAsync(guessId, answer);
    if (guess != null)
      await _tracking.GuessAsync(GameKind.Systems, date, continent, guessId, answer.Id);

    return guess;
  }

  public async Task<LineGuessResponse?> GuessLineAsync(DateOnly date, Continent? continent, int guessId)
  {
    var puzzle = await FindPuzzleAsync(GameKind.Lines, date, continent);
    if (puzzle == null)
      return null;

    var answer = await _db.GameLines.FindAsync(puzzle.TargetId);
    if (answer == null)
    {
      await _tracking.ErrorAsync(nameof(GuessLineAsync),
          new { reason = "AnswerNotFound", game = GameKind.Lines, date, targetId = puzzle.TargetId });
      return null;
    }

    var guess = await _gameData.GuessLineAsync(guessId, answer);
    if (guess != null)
      await _tracking.GuessAsync(GameKind.Lines, date, continent, guessId, answer.Id);

    return guess;
  }

  public async Task<GameSystemResponse?> RevealSystemAsync(DateOnly date, Continent? continent)
  {
    var puzzle = await FindPuzzleAsync(GameKind.Systems, date, continent);
    return puzzle == null ? null : await _gameData.GetGameSystemAsync(puzzle.TargetId);
  }

  public async Task<GameLineResponse?> RevealLineAsync(DateOnly date, Continent? continent)
  {
    var puzzle = await FindPuzzleAsync(GameKind.Lines, date, continent);
    return puzzle == null ? null : await _gameData.GetGameLineAsync(puzzle.TargetId);
  }

  // Puzzles are generated a few days ahead, so refuse dates no timezone (max UTC+14) has reached yet.
  // Logs failures under the calling method's name, so callers don't need to.
  private async Task<DailyPuzzle?> FindPuzzleAsync(GameKind game, DateOnly date, Continent? continent,
      [CallerMemberName] string caller = "")
  {
    continent ??= Continent.Global;

    var puzzle = await _db.DailyPuzzles.AsNoTracking()
        .FirstOrDefaultAsync(p => p.Game == game && p.Date == date && p.Continent == continent);

    if (puzzle == null)
      await _tracking.ErrorAsync(caller, new { reason = "PuzzleNotFound", game, date, continent });

    return puzzle;
  }
}