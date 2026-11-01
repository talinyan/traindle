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
  public async Task<IActionResult> GetDailySystem(DateOnly date, Continent? continent) =>
      OkOrNotFound(await _daily.GetDailyAsync(GameKind.Systems, date, continent));

  [HttpPost("systems/guess")]
  public async Task<IActionResult> GuessSystem(SystemDailyGuessRequest req) =>
      OkOrNotFound(await _daily.GuessSystemAsync(req.Date, req.Continent, req.SystemId));

  [HttpGet("systems/reveal")]
  public async Task<IActionResult> RevealSystem(DateOnly date, Continent? continent) =>
      OkOrNotFound(await _daily.RevealSystemAsync(date, continent));

  [HttpGet("lines/daily")]
  public async Task<IActionResult> GetDailyLine(DateOnly date, Continent? continent) =>
      OkOrNotFound(await _daily.GetDailyAsync(GameKind.Lines, date, continent));

  [HttpPost("lines/guess")]
  public async Task<IActionResult> GuessLine(LineDailyGuessRequest req) =>
      OkOrNotFound(await _daily.GuessLineAsync(req.Date, req.Continent, req.LineId));

  [HttpGet("lines/reveal")]
  public async Task<IActionResult> RevealLine(DateOnly date, Continent? continent) =>
      OkOrNotFound(await _daily.RevealLineAsync(date, continent));

  private IActionResult OkOrNotFound(object? result) => result == null ? NotFound() : Ok(result);
}
