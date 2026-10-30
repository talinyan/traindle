using Microsoft.AspNetCore.Mvc;
using Traindle.Data;
using Traindle.Services;

namespace Traindle.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RandomGameController : ControllerBase
{
  public RandomGameController(RandomGameService random)
  {
    _random = random;
  }

  private readonly RandomGameService _random;

  [HttpGet("systems/random")]
  public async Task<IActionResult> GetRandomSystem(Continent? continent) =>
      OkOrNotFound(await _random.GetRandomPuzzleAsync(GameKind.Systems, continent));

  [HttpPost("systems/guess")]
  public async Task<IActionResult> GuessSystem(SystemRandomGuessRequest req) =>
      OkOrNotFound(await _random.GuessSystemAsync(req.AnswerId, req.SystemId));

  [HttpGet("lines/random")]
  public async Task<IActionResult> GetRandomLine(Continent? continent) =>
      OkOrNotFound(await _random.GetRandomPuzzleAsync(GameKind.Lines, continent));

  [HttpPost("lines/guess")]
  public async Task<IActionResult> GuessLine(LineRandomGuessRequest req) =>
      OkOrNotFound(await _random.GuessLineAsync(req.AnswerId, req.LineId));

  private IActionResult OkOrNotFound(object? result) => result == null ? NotFound() : Ok(result);
}
