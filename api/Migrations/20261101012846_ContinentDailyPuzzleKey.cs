using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class ContinentDailyPuzzleKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_daily_puzzles",
                table: "daily_puzzles");

            migrationBuilder.AlterColumn<int>(
                name: "continent",
                table: "daily_puzzles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "pk_daily_puzzles",
                table: "daily_puzzles",
                columns: new[] { "date", "game", "continent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_daily_puzzles",
                table: "daily_puzzles");

            migrationBuilder.AlterColumn<int>(
                name: "continent",
                table: "daily_puzzles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "pk_daily_puzzles",
                table: "daily_puzzles",
                columns: new[] { "date", "game" });
        }
    }
}
