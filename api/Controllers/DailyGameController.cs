using Microsoft.AspNetCore.Mvc;
using Traindle.Data;
using Traindle.Services;

namespace Traindle.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DailyGameController : ControllerBase
{
    public DailyGameController(DailyGameService daily)
    {
        _daily = daily;
    }

    private readonly DailyGameService _daily;

    [HttpGet("systems/daily")]
    public async Task<IActionResult> GetDailySystem([FromQuery] DateOnly date) =>
        OkOrNotFound(await _daily.GetDailyAsync(GameKind.Systems, date));

    [HttpPost("systems/guess")]
    public async Task<IActionResult> GuessSystem(SystemDailyGuessRequest req) =>
        OkOrNotFound(await _daily.GuessSystemAsync(req.Date, req.SystemId));

    [HttpGet("systems/reveal")]
    public async Task<IActionResult> RevealSystem([FromQuery] DateOnly date) =>
        OkOrNotFound(await _daily.RevealSystemAsync(date));

    [HttpGet("lines/daily")]
    public async Task<IActionResult> GetDailyLine([FromQuery] DateOnly date) =>
        OkOrNotFound(await _daily.GetDailyAsync(GameKind.Lines, date));

    [HttpPost("lines/guess")]
    public async Task<IActionResult> GuessLine(LineDailyGuessRequest req) =>
        OkOrNotFound(await _daily.GuessLineAsync(req.Date, req.LineId));

    [HttpGet("lines/reveal")]
    public async Task<IActionResult> RevealLine([FromQuery] DateOnly date) =>
        OkOrNotFound(await _daily.RevealLineAsync(date));

    private IActionResult OkOrNotFound(object? result) => result == null ? NotFound() : Ok(result);
}
