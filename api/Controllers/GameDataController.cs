using Microsoft.AspNetCore.Mvc;
using Traindle.Data;
using Traindle.Services;

namespace Traindle.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameDataController : ControllerBase
{
  public GameDataController(GameDataService gd)
  {
    _gd = gd;
  }

  private readonly GameDataService _gd;

  [HttpGet("systems/options")]
  public async Task<IActionResult> GetGameSystemOptions(Continent? continent)
  {
    var systems = await _gd.GetGameSystemOptionsAsync(continent);
    return Ok(systems);
  }

  [HttpGet("systems/reveal")]
  public async Task<IActionResult> GetGameSystem([FromQuery] int id)
  {
    var system = await _gd.GetGameSystemAsync(id);
    return Ok(system);
  }

  [HttpGet("lines/options")]
  public async Task<IActionResult> GetGameLineOptions(Continent? continent)
  {
    var lines = await _gd.GetGameLineOptionsAsync(continent);
    return Ok(lines);
  }

  [HttpGet("lines/reveal")]
  public async Task<IActionResult> GetGameLine([FromQuery] int id)
  {
    var line = await _gd.GetGameLineAsync(id);
    return Ok(line);
  }
}
