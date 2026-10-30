using Microsoft.AspNetCore.Mvc;
using Traindle.Services;

namespace Traindle.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OSMController : ControllerBase
{
  private readonly OpenStreetMapService _osm;

  public OSMController(OpenStreetMapService osm)
  {
    _osm = osm;
  }

  private readonly RandomGameService _random;

  [HttpGet("osm")]
  public async Task<IActionResult> GetOSM() =>
      Ok(await _osm.GetDataAsync("""
    [out:json][timeout:180];
    area["wikidata"="Q172"]->.city;
    rel(area.city)["type"="route"]["route"~"^(subway|light_rail|tram)$"]->.routes;
    rel(br.routes)["type"="route_master"]->.masters;
    (.routes; .masters;);
    out geom;
    """));
}
