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

    [HttpGet("lines/options")]
    public async Task<IActionResult> GetGameLineOptions()
    {
        var lines = await _gd.GetGameLineOptionsAsync();
        return Ok(lines);
    }
}
