using System.Text.Json;
using Traindle.Data;

namespace Traindle.Services;

public class TrackingService
{
  private readonly PostgresContext _db;
  private readonly ILogger<TrackingService> _log;

  public TrackingService(PostgresContext db, ILogger<TrackingService> log)
  {
    _db = db;
    _log = log;
  }

  public Task ErrorAsync(string type, object? data = null) => LogAsync(type, StatusType.Error, data);
  public Task InfoAsync(string type, object? data = null) => LogAsync(type, StatusType.Info, data);

  public Task FetchAsync(GameKind game, DateOnly? date, Continent? continent, int? answerId = null) =>
      SaveAsync(new GameEvent { Type = GameEventType.Fetch, Game = game, PuzzleDate = date, Continent = continent, AnswerId = answerId });

  public Task GuessAsync(GameKind game, DateOnly? date, Continent? continent, int guessId, int answerId) =>
      SaveAsync(new GameEvent
      {
        Type = GameEventType.Guess,
        Game = game,
        PuzzleDate = date,
        Continent = continent,
        GuessId = guessId,
        AnswerId = answerId
      });

  private Task LogAsync(string type, StatusType status, object? data) =>
      SaveAsync(new Log
      {
        Type = type,
        Status = status,
        Data = data is null ? "{}" : JsonSerializer.Serialize(data)
      });

  private async Task SaveAsync(object entity)
  {
    try
    {
      _db.Add(entity);
      await _db.SaveChangesAsync();
    }
    catch (Exception ex)
    {
      // Tracking should never break the game itself
      _log.LogWarning(ex, "Failed to save {Entity}", entity.GetType().Name);
    }
  }
}