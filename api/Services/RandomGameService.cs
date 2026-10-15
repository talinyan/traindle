using Traindle.Data;

namespace Traindle.Services;

public class RandomGameService
{
    private readonly PostgresContext _db;
    private readonly GameDataService _gameData;
    private readonly ILogger<RandomGameService> _logger;

    public RandomGameService(PostgresContext db, GameDataService gameData, ILogger<RandomGameService> logger)
    {
        _db = db;
        _gameData = gameData;
        _logger = logger;
    }

    public async Task<RandomPuzzleResponse?> GetRandomPuzzleAsync(GameKind game)
    {
        var id = await _db.PickRandomTargetIdAsync(game, []);

        if (id is null)
        {
            _logger.LogError("Failed to generate id for random {Game} puzzle", game);
            return null;
        }

        return new RandomPuzzleResponse(game, (int)id);
    }

    public async Task<SystemGuessResponse?> GuessSystemAsync(int answerId, int guessId)
    {
        var answer = await _db.GameSystems.FindAsync(answerId);
        if (answer == null)
            return null;

        return await _gameData.GuessSystemAsync(guessId, answer);
    }

    public async Task<LineGuessResponse?> GuessLineAsync(int answerId, int guessId)
    {
        var answer = await _db.GameLines.FindAsync(answerId);
        if (answer == null)
            return null;

        return await _gameData.GuessLineAsync(guessId, answer);
    }
}