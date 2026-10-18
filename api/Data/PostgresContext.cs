using Microsoft.EntityFrameworkCore;

namespace Traindle.Data;

public class PostgresContext : DbContext
{
    public PostgresContext(DbContextOptions<PostgresContext> options) : base(options) { }

    public DbSet<City> Cities => Set<City>();
    public DbSet<TransportSystem> Systems => Set<TransportSystem>();
    public DbSet<TransportMode> TransportModes => Set<TransportMode>();
    public DbSet<Line> Lines => Set<Line>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<StationLine> StationLines => Set<StationLine>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<SectionLine> SectionLines => Set<SectionLine>();

    public DbSet<GameSystem> GameSystems => Set<GameSystem>();
    public DbSet<GameLine> GameLines => Set<GameLine>();
    public DbSet<DailyPuzzle> DailyPuzzles => Set<DailyPuzzle>();

    public DbSet<Log> Logs => Set<Log>();
    public DbSet<GameEvent> GameEvents => Set<GameEvent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<City>(e =>
        {
            e.ToTable("cities");
            e.Property(c => c.Id).ValueGeneratedNever();
        });

        b.Entity<TransportSystem>(e =>
        {
            e.ToTable("systems");
            e.Property(s => s.Id).ValueGeneratedNever();
            e.HasOne(s => s.City)
                .WithMany(c => c.Systems)
                .HasForeignKey(s => s.CityId);
        });

        b.Entity<TransportMode>(e =>
        {
            e.ToTable("transport_modes");
            e.Property(m => m.Id).ValueGeneratedNever();
        });

        b.Entity<Line>(e =>
        {
            e.ToTable("lines");
            e.Property(l => l.Id).ValueGeneratedNever();
            e.HasIndex(l => new { l.CityId, l.UrlName });

            e.HasOne(l => l.City)
                .WithMany(c => c.Lines)
                .HasForeignKey(l => l.CityId);

            e.HasOne(l => l.TransportSystem)
                .WithMany(s => s.Lines)
                .HasForeignKey(l => l.SystemId);

            e.HasOne(l => l.TransportMode)
                .WithMany(m => m.Lines)
                .HasForeignKey(l => l.TransportModeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Station>(e =>
        {
            e.ToTable("stations");
            e.Property(m => m.Id).ValueGeneratedNever();

            e.HasOne(l => l.City)
                .WithMany(c => c.Stations)
                .HasForeignKey(l => l.CityId);
        });

        b.Entity<StationLine>(e =>
        {
            e.ToTable("station_lines");
            e.Property(m => m.Id).ValueGeneratedNever();

            e.HasOne(l => l.Station)
                .WithMany(c => c.StationLines)
                .HasForeignKey(l => l.StationId);

            e.HasOne(l => l.Line)
                .WithMany(c => c.StationLines)
                .HasForeignKey(l => l.LineId);

            e.HasOne(l => l.City)
                .WithMany(c => c.StationLines)
                .HasForeignKey(l => l.CityId);
        });

        b.Entity<Section>(e =>
        {
            e.ToTable("sections");
            e.Property(m => m.Id).ValueGeneratedNever();

            e.HasOne(l => l.City)
                .WithMany(c => c.Sections)
                .HasForeignKey(l => l.CityId);
        });

        b.Entity<SectionLine>(e =>
        {
            e.ToTable("section_lines");
            e.Property(m => m.Id).ValueGeneratedNever();

            e.HasOne(l => l.Section)
                .WithMany(c => c.SectionLines)
                .HasForeignKey(l => l.SectionId);

            e.HasOne(l => l.Line)
                .WithMany(c => c.SectionLine)
                .HasForeignKey(l => l.LineId);

            e.HasOne(l => l.City)
                .WithMany(c => c.SectionLine)
                .HasForeignKey(l => l.CityId);
        });

        b.Entity<GameSystem>(e =>
        {
            e.ToTable("game_systems");
            e.Property(s => s.Id).ValueGeneratedNever();
        });

        b.Entity<GameLine>(e =>
        {
            e.ToTable("game_lines");
            e.Property(l => l.Id).ValueGeneratedNever();

            e.HasOne(l => l.System)
                .WithMany(s => s.Lines)
                .HasForeignKey(l => l.SystemId);
        });

        b.Entity<DailyPuzzle>(e =>
        {
            e.ToTable("daily_puzzles");
            e.HasKey(p => new { p.Date, p.Game });
            e.Property(p => p.Game).HasConversion<string>();
        });

        b.Entity<Log>(e =>
        {
            e.ToTable("logs");
        });

        b.Entity<GameEvent>(e =>
        {
            e.ToTable("game_events");
        });
    }
}