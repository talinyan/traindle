using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddGameDataProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_puzzles",
                columns: table => new
                {
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    game = table.Column<string>(type: "text", nullable: false),
                    target_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_puzzles", x => new { x.date, x.game });
                });

            migrationBuilder.CreateTable(
                name: "game_systems",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: true),
                    country_state = table.Column<string>(type: "text", nullable: true),
                    continent = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    modes = table.Column<List<string>>(type: "text[]", nullable: false),
                    line_count = table.Column<int>(type: "integer", nullable: false),
                    station_count = table.Column<int>(type: "integer", nullable: false),
                    interchange_count = table.Column<int>(type: "integer", nullable: false),
                    length = table.Column<int>(type: "integer", nullable: false),
                    opening_year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_systems", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    system_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    colour = table.Column<string>(type: "text", nullable: true),
                    mode = table.Column<string>(type: "text", nullable: false),
                    station_count = table.Column<int>(type: "integer", nullable: false),
                    length = table.Column<int>(type: "integer", nullable: false),
                    opening_year = table.Column<int>(type: "integer", nullable: false),
                    geometry = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_game_lines_game_systems_system_id",
                        column: x => x.system_id,
                        principalTable: "game_systems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_game_lines_system_id",
                table: "game_lines",
                column: "system_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_puzzles");

            migrationBuilder.DropTable(
                name: "game_lines");

            migrationBuilder.DropTable(
                name: "game_systems");
        }
    }
}
