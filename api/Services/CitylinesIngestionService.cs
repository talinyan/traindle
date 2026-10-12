using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Traindle.Data;

namespace Traindle.Services;

public class CitylinesIngestionService
{
    // Heroku dataclip IDs behind citylines.co/data
    private static readonly Dictionary<string, string> Datasets = new()
    {
        ["cities"] = "jebekjcluccqocceiptedgqkozzd",
        ["systems"] = "qhywjbvyydeavbdhjxmwujjnbrhd",
        ["lines"] = "otnctkgbjkbzeoldkyvidokyeoan",
        ["sections"] = "fhmwgljariodradnpdsamxsibvao",
        ["section_lines"] = "qinblnpytdjbbbckxxojysegjjoi",
        ["stations"] = "jbidyazpowvbwiswtglhdknebmvp",
        ["station_lines"] = "atetlpcsnccpbzphyigybilqowlh",
        ["transport_modes"] = "pizaxxjacfuttqbrglmojarnostn",
    };

    private static readonly Dictionary<Type, string> DatasetForType = new()
    {
        [typeof(City)] = "cities",
        [typeof(TransportSystem)] = "systems",
        [typeof(TransportMode)] = "transport_modes",
        [typeof(Line)] = "lines",
        [typeof(Station)] = "stations",
        [typeof(StationLine)] = "station_lines",
        [typeof(Section)] = "sections",
        [typeof(SectionLine)] = "section_lines",
    };

    private readonly PostgresContext _db;
    private readonly HttpClient _http;
    private readonly string _csvDir;
    private readonly ILogger<CitylinesIngestionService> _logger;

    private readonly CsvConfiguration _config = new(CultureInfo.InvariantCulture)
    {
        MissingFieldFound = null,   // don't throw when an optional column is absent
        HeaderValidated = null,     // same, for the header check
        TrimOptions = TrimOptions.Trim,
    };

    public CitylinesIngestionService(PostgresContext db, HttpClient http, IHostEnvironment env,
        ILogger<CitylinesIngestionService> logger)
    {
        _db = db;
        _http = http;
        _logger = logger;
        _csvDir = Path.Combine(env.ContentRootPath, "csvs");
    }

    // Ingests every dataset in foreign-key order; returns rows written per table
    public async Task<Dictionary<string, int>> IngestAllAsync(bool forceDownload = true)
    {
        var cts = new Dictionary<string, int>();

        async Task Run<T>(string name) where T : class
        {
            cts[name] = await SaveCitylinesDataAsync<T>(forceDownload);
            _logger.LogInformation("Ingested {Count} {Name}", cts[name], name);
        }

        await Run<City>("Cities");
        await Run<TransportSystem>("Systems");
        await Run<TransportMode>("TransportModes");
        await Run<Line>("Lines");
        await Run<Station>("Stations");
        await Run<StationLine>("StationLines");
        await Run<Section>("Sections");
        await Run<SectionLine>("SectionLines");

        return cts;
    }

    public async Task<List<T>> GetCitylinesDataAsync<T>(bool forceDownload = false)
    {
        if (!DatasetForType.TryGetValue(typeof(T), out var dataset))
            throw new ArgumentException($"No Citylines dataset mapped for {typeof(T).Name}");
        var path = await DownloadAsync(dataset, forceDownload);
        return ReadCsv<T>(path);
    }

    public async Task<int> SaveCitylinesDataAsync<T>(bool forceDownload = false) where T : class
    {
        var items = await GetCitylinesDataAsync<T>(forceDownload);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        await _db.Set<T>().ExecuteDeleteAsync();   // clear the table
        items = await DropOrphansAsync(items);
        _db.Set<T>().AddRange(items);
        var added = await _db.SaveChangesAsync();  // number of rows written

        await transaction.CommitAsync();
        return added;
    }

    // Rows sometimes no longer exist but don't cascade, so ensure they are dropped
    private async Task<List<T>> DropOrphansAsync<T>(List<T> items)
    {
        IEnumerable<object> kept;
        switch (items)
        {
            case List<TransportSystem> rows:
                {
                    var cities = await _db.Cities.Select(c => c.Id).ToHashSetAsync();
                    kept = rows.Where(r => cities.Contains(r.CityId));
                    break;
                }
            case List<Station> rows:
                {
                    var cities = await _db.Cities.Select(c => c.Id).ToHashSetAsync();
                    kept = rows.Where(r => cities.Contains(r.CityId));
                    break;
                }
            case List<Section> rows:
                {
                    var cities = await _db.Cities.Select(c => c.Id).ToHashSetAsync();
                    kept = rows.Where(r => cities.Contains(r.CityId));
                    break;
                }
            case List<Line> rows:
                {
                    var cities = await _db.Cities.Select(c => c.Id).ToHashSetAsync();
                    var systems = await _db.Systems.Select(s => s.Id).ToHashSetAsync();
                    var modes = await _db.TransportModes.Select(m => m.Id).ToHashSetAsync();
                    kept = rows.Where(r => cities.Contains(r.CityId)
                                           && systems.Contains(r.SystemId)
                                           && modes.Contains(r.TransportModeId));
                    break;
                }
            case List<StationLine> rows:
                {
                    var stations = await _db.Stations.Select(s => s.Id).ToHashSetAsync();
                    var lines = await _db.Lines.Select(l => l.Id).ToHashSetAsync();
                    var cities = await _db.Cities.Select(c => c.Id).ToHashSetAsync();
                    kept = rows.Where(r => stations.Contains(r.StationId)
                                           && lines.Contains(r.LineId)
                                           && cities.Contains(r.CityId));
                    break;
                }
            case List<SectionLine> rows:
                {
                    var sections = await _db.Sections.Select(s => s.Id).ToHashSetAsync();
                    var lines = await _db.Lines.Select(l => l.Id).ToHashSetAsync();
                    var cities = await _db.Cities.Select(c => c.Id).ToHashSetAsync();
                    kept = rows.Where(r => sections.Contains(r.SectionId)
                                           && lines.Contains(r.LineId)
                                           && cities.Contains(r.CityId));
                    break;
                }
            default:
                return items;
        }

        var result = kept.Cast<T>().ToList();
        if (result.Count < items.Count)
            _logger.LogWarning("Skipped {Count} orphaned {Type} rows", items.Count - result.Count, typeof(T).Name);
        return result;
    }

    // Downloads a dataset to csvs/{name}.csv unless a copy under a week old exists.
    private async Task<string> DownloadAsync(string dataset, bool force)
    {
        Directory.CreateDirectory(_csvDir);
        var path = Path.Combine(_csvDir, $"{dataset}.csv");

        if (!force && File.Exists(path) && File.GetLastWriteTimeUtc(path) > DateTime.UtcNow.AddDays(-7))
            return path;

        var url = $"https://data.heroku.com/dataclips/{Datasets[dataset]}.csv";
        using var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        // Write to a temp file first so a failed download never leaves a half-written CSV
        var tmp = path + ".tmp";
        await using (var file = File.Create(tmp))
            await response.Content.CopyToAsync(file);
        File.Move(tmp, path, overwrite: true);

        return path;
    }

    private List<T> ReadCsv<T>(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, _config);

        var utc = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;
        csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().DateTimeStyle = utc;
        csv.Context.TypeConverterOptionsCache.GetOptions<DateTime?>().DateTimeStyle = utc;

        return csv.GetRecords<T>().ToList();
    }
}