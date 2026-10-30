using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Traindle.Data;

namespace Traindle.Services;

public class OpenStreetMapService
{
  private const string OverpassUrl = "https://overpass-api.de/api/interpreter";

  private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

  private readonly PostgresContext _db;
  private readonly HttpClient _http;
  private readonly ILogger<OpenStreetMapService> _logger;

  public OpenStreetMapService(PostgresContext db, HttpClient http, ILogger<OpenStreetMapService> logger)
  {
    _db = db;
    _http = http;
    _logger = logger;
  }

  private static readonly string[] OverpassUrls =
  [
    "https://overpass-api.de/api/interpreter",
  "https://overpass.private.coffee/api/interpreter",
  "https://overpass.kumi.systems/api/interpreter",
];

  private static readonly HashSet<HttpStatusCode> RetryableStatuses =
  [
    HttpStatusCode.TooManyRequests,     // 429 rate limited
    HttpStatusCode.BadGateway,          // 502 at capacity
    HttpStatusCode.ServiceUnavailable,  // 503
    HttpStatusCode.GatewayTimeout,      // 504
  ];

  // Runs an Overpass QL query and returns the parsed elements.
  public async Task<OverpassResponse> GetDataAsync(string query, CancellationToken ct = default)
  {
    const int maxAttempts = 5;
    var delay = TimeSpan.FromSeconds(5);

    for (var attempt = 1; ; attempt++)
    {
      // Rotate through the servers so one overloaded instance doesn't block every attempt
      var url = OverpassUrls[(attempt - 1) % OverpassUrls.Length];

      HttpResponseMessage response;
      try
      {
        response = await _http.PostAsync(url, new FormUrlEncodedContent([new("data", query)]), ct);
      }
      catch (HttpRequestException ex) when (attempt < maxAttempts)
      {
        _logger.LogWarning(ex, "Overpass request to {Url} failed, retrying in {Delay}s", url, delay.TotalSeconds);
        await Task.Delay(delay, ct);
        delay *= 2;
        continue;
      }

      using (response)
      {
        if (RetryableStatuses.Contains(response.StatusCode) && attempt < maxAttempts)
        {
          var wait = response.Headers.RetryAfter?.Delta ?? delay;
          _logger.LogWarning("Overpass {Url} returned {Status}, retrying in {Delay}s (attempt {Attempt}/{Max})",
              url, (int)response.StatusCode, wait.TotalSeconds, attempt, maxAttempts);
          await Task.Delay(wait, ct);
          delay *= 2;
          continue;
        }

        if (!response.IsSuccessStatusCode)
        {
          var body = await response.Content.ReadAsStringAsync(ct);
          _logger.LogError("Overpass query to {Url} failed with {Status}: {Body}", url, (int)response.StatusCode, body);
          response.EnsureSuccessStatusCode();
        }

        var data = await response.Content.ReadFromJsonAsync<OverpassResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Overpass returned an empty response");

        _logger.LogInformation("Overpass {Url} returned {Count} elements", url, data.Elements.Count);
        return data;
      }
    }
  }
}

public record OverpassResponse(List<OsmElement> Elements);

public record OsmElement(
    string Type, long Id,
    Dictionary<string, string>? Tags,
    List<OsmMember>? Members,
    double? Lat, double? Lon);

public record OsmMember(
    string Type, long Ref, string Role,
    double? Lat, double? Lon,
    List<OsmLatLon>? Geometry);

public record OsmLatLon(double Lat, double Lon);