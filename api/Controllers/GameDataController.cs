using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetGameSystemOptions()
    {
        var systems = await _gd.GetGameSystemOptionsAsync();
        return Ok(systems);
    }

    [HttpGet("systems/reveal")]
    public async Task<IActionResult> GetGameSystem([FromQuery] int id)
    {
        var system = await _gd.GetGameSystemAsync(id);
        return Ok(system);
    }

    [HttpGet("lines/options")]
    public async Task<IActionResult> GetGameLineOptions()
    {
        var lines = await _gd.GetGameLineOptionsAsync();
        return Ok(lines);
    }

    [HttpGet("lines/reveal")]
    public async Task<IActionResult> GetGameLine([FromQuery] int id)
    {
        var line = await _gd.GetGameLineAsync(id);
        return Ok(line);
    }
}
