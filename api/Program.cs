using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Traindle.Data;
using Traindle.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();


builder.Services.AddDbContext<PostgresContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention()
    );

builder.Services.AddHttpClient<CitylinesIngestionService>(c =>
{
  c.Timeout = TimeSpan.FromMinutes(5);
  c.DefaultRequestHeaders.UserAgent.ParseAdd("Traindle/0.1");
});

builder.Services.AddHttpClient<OpenStreetMapService>(c =>
{
  c.Timeout = TimeSpan.FromMinutes(5);
  c.DefaultRequestHeaders.UserAgent.ParseAdd("Traindle/0.1");
});

builder.Services.AddScoped<GameDataService>();
builder.Services.AddScoped<DailyGameService>();
builder.Services.AddScoped<RandomGameService>();
builder.Services.AddScoped<DailyPuzzleGenerationService>();
builder.Services.AddScoped<TrackingService>();

if (!args.Contains("ingest") && !args.Contains("generate-daily"))
  builder.Services.AddHostedService<DailyPuzzleGenerator>();

builder.Services.AddCors(o => o.AddPolicy("www", p =>
{
  p.WithOrigins("http://localhost:5173", "http://localhost:5174", "https://traindle.talinya.ca")
   .AllowAnyHeader().AllowAnyMethod();
}));

// convert enums to string on return
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<PostgresContext>();
  db.Database.Migrate();
}

// CLI mode: `dotnet run -- ingest` to ingest data from citylines.co
if (args.Contains("ingest"))
{
  using var scope = app.Services.CreateScope();
  var counts = await scope.ServiceProvider
      .GetRequiredService<CitylinesIngestionService>()
      .IngestAllAsync();
  foreach (var (name, ct) in counts)
    Console.WriteLine($"{name}: {ct}");

  var (systems, lines) = await scope.ServiceProvider
          .GetRequiredService<GameDataService>()
          .ProcessAsync();
  Console.WriteLine($"GameSystems: {systems}");
  Console.WriteLine($"GameLines: {lines}");
  return;
}

// CLI mode: `dotnet run -- generate-daily` to create any missing daily puzzles (also runs hourly in the server)
if (args.Contains("generate-daily"))
{
  using var scope = app.Services.CreateScope();
  var created = await scope.ServiceProvider
      .GetRequiredService<DailyPuzzleGenerationService>()
      .EnsurePuzzlesAsync();
  Console.WriteLine($"Daily puzzles created: {created}");
  return;
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors("www");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
