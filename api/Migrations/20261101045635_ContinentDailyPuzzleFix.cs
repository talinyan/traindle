using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class ContinentDailyPuzzleFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "continent",
                table: "game_events",
                type: "integer",
                nullable: true);

            // ContinentDailyPuzzleKey defaulted existing puzzles to 0 (NorthAmerica); they were the global puzzle (Global = 6).
            // Regional puzzles made before the generator filtered by continent have the wrong targets, so drop them to be regenerated.
            migrationBuilder.Sql("""
                UPDATE daily_puzzles p SET continent = 6
                WHERE continent = 0
                  AND NOT EXISTS (SELECT 1 FROM daily_puzzles g WHERE g.date = p.date AND g.game = p.game AND g.continent = 6);
                DELETE FROM daily_puzzles WHERE continent <> 6;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "continent",
                table: "game_events");
        }
    }
}
