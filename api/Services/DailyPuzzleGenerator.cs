using Microsoft.EntityFrameworkCore;
using Traindle.Data;

namespace Traindle.Services;

public class DailyPuzzleGenerationService
{
    private const int DaysAhead = 3;
    private const int NoRepeatDays = 120;

    private readonly PostgresContext _db;
    private readonly ILogger<DailyPuzzleGenerationService> _logger;

    public DailyPuzzleGenerationService(PostgresContext db, ILogger<DailyPuzzleGenerationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> EnsurePuzzlesAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var created = 0;

        foreach (var game in Enum.GetValues<GameKind>())
        {
            for (var date = today.AddDays(-1); date <= today.AddDays(DaysAhead); date = date.AddDays(1))
            {
                if (await _db.DailyPuzzles.AnyAsync(p => p.Game == game && p.Date == date, ct))
                    continue;

                if (await CreatePuzzleAsync(game, date, ct))
                    created++;
            }
        }

        return created;
    }

    private async Task<bool> CreatePuzzleAsync(GameKind game, DateOnly date, CancellationToken ct)
    {
        // Avoid repeating recent answers
        var since = date.AddDays(-NoRepeatDays);
        var recent = await _db.DailyPuzzles
            .Where(p => p.Game == game && p.Date >= since)
            .Select(p => p.TargetId)
            .ToListAsync(ct);

        var targetId = await _db.PickRandomTargetIdAsync(game, recent, ct)
                       ?? await _db.PickRandomTargetIdAsync(game, [], ct);
        if (targetId is null)
        {
            _logger.LogError("No {Game} available to create the {Date} puzzle", game, date);
            return false;
        }

        _db.DailyPuzzles.Add(new DailyPuzzle { Game = game, Date = date, TargetId = targetId.Value });
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            return false;
        }

        _logger.LogInformation("Created {Game} puzzle for {Date}: {TargetId}", game, date, targetId);
        return true;
    }
}

public class DailyPuzzleGenerator : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<DailyPuzzleGenerator> _logger;

    public DailyPuzzleGenerator(IServiceScopeFactory scopes, ILogger<DailyPuzzleGenerator> logger)
    {
        _scopes = scopes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = _scopes.CreateScope();
                await scope.ServiceProvider
                    .GetRequiredService<DailyPuzzleGenerationService>()
                    .EnsurePuzzlesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Daily puzzle generation failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
