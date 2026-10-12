using System.Text.Json.Serialization;
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

builder.Services.AddCors(o => o.AddPolicy("www", p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

// convert enums to string on return
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// CLI mode: `dotnet run -- ingest` to ingest data from citylines.co
if (args.Contains("ingest"))
{
    using var scope = app.Services.CreateScope();
    var counts = await scope.ServiceProvider
        .GetRequiredService<CitylinesIngestionService>()
        .IngestAllAsync();
    foreach (var (name, ct) in counts)
        Console.WriteLine($"{name}: {ct}");
    return;
}

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
