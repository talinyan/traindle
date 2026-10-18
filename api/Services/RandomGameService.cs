using Traindle.Data;

namespace Traindle.Services;

public class RandomGameService
{
    private readonly PostgresContext _db;
    private readonly GameDataService _gameData;
    private readonly TrackingService _tracking;

    public RandomGameService(PostgresContext db, GameDataService gameData, TrackingService tracking)
    {
        _db = db;
        _gameData = gameData;
        _tracking = tracking;
    }

    public async Task<RandomPuzzleResponse?> GetRandomPuzzleAsync(GameKind game)
    {
        var id = await _db.PickRandomTargetIdAsync(game, []);
        if (id is null)
        {
            await _tracking.ErrorAsync(nameof(GetRandomPuzzleAsync), new { reason = "NoTargetId", game });
            return null;
        }

        await _tracking.FetchAsync(game, null, (int)id);
        return new RandomPuzzleResponse(game, (int)id);
    }

    public async Task<SystemGuessResponse?> GuessSystemAsync(int answerId, int guessId)
    {
        var answer = await _db.GameSystems.FindAsync(answerId);
        if (answer == null)
        {
            await _tracking.InfoAsync(nameof(GuessSystemAsync),
                new { reason = "AnswerNotFound", game = GameKind.Systems, answerId });
            return null;
        }

        var guess = await _gameData.GuessSystemAsync(guessId, answer);
        if (guess != null)
            await _tracking.GuessAsync(GameKind.Systems, null, guessId, answer.Id);

        return guess;
    }

    public async Task<LineGuessResponse?> GuessLineAsync(int answerId, int guessId)
    {
        var answer = await _db.GameLines.FindAsync(answerId);
        if (answer == null)
        {
            await _tracking.InfoAsync(nameof(GuessLineAsync),
                new { reason = "AnswerNotFound", game = GameKind.Lines, answerId });
            return null;
        }

        var guess = await _gameData.GuessLineAsync(guessId, answer);
        if (guess != null)
            await _tracking.GuessAsync(GameKind.Lines, null, guessId, answer.Id);

        return guess;
    }
}