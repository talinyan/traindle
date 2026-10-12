using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    coords = table.Column<string>(type: "text", nullable: true),
                    start_year = table.Column<int>(type: "integer", nullable: false),
                    url_name = table.Column<string>(type: "text", nullable: true),
                    country = table.Column<string>(type: "text", nullable: true),
                    country_state = table.Column<string>(type: "text", nullable: true),
                    length = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transport_modes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_modes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sections",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    geometry = table.Column<string>(type: "text", nullable: true),
                    build_start = table.Column<int>(type: "integer", nullable: true),
                    opening = table.Column<int>(type: "integer", nullable: true),
                    closure = table.Column<int>(type: "integer", nullable: true),
                    length = table.Column<int>(type: "integer", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sections", x => x.id);
                    table.ForeignKey(
                        name: "fk_sections_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "systems",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    historic = table.Column<bool>(type: "boolean", nullable: false),
                    length = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_systems", x => x.id);
                    table.ForeignKey(
                        name: "fk_systems_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    url_name = table.Column<string>(type: "text", nullable: true),
                    color = table.Column<string>(type: "text", nullable: true),
                    system_id = table.Column<int>(type: "integer", nullable: false),
                    transport_mode_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_lines_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lines_systems_system_id",
                        column: x => x.system_id,
                        principalTable: "systems",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lines_transport_modes_transport_mode_id",
                        column: x => x.transport_mode_id,
                        principalTable: "transport_modes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "section_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    section_id = table.Column<int>(type: "integer", nullable: false),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    from_year = table.Column<int>(type: "integer", nullable: true),
                    to_year = table.Column<int>(type: "integer", nullable: true),
                    deprecated_line_group = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_section_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_section_lines_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_section_lines_lines_line_id",
                        column: x => x.line_id,
                        principalTable: "lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_section_lines_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    geometry = table.Column<string>(type: "text", nullable: true),
                    build_start = table.Column<int>(type: "integer", nullable: true),
                    opening = table.Column<int>(type: "integer", nullable: true),
                    closure = table.Column<int>(type: "integer", nullable: true),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    line_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stations", x => x.id);
                    table.ForeignKey(
                        name: "fk_stations_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stations_lines_line_id",
                        column: x => x.line_id,
                        principalTable: "lines",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "station_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    station_id = table.Column<int>(type: "integer", nullable: false),
                    line_id = table.Column<int>(type: "integer", nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    from_year = table.Column<int>(type: "integer", nullable: true),
                    to_year = table.Column<int>(type: "integer", nullable: true),
                    deprecated_line_group = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_station_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_station_lines_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_station_lines_lines_line_id",
                        column: x => x.line_id,
                        principalTable: "lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_station_lines_stations_station_id",
                        column: x => x.station_id,
                        principalTable: "stations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lines_city_id_url_name",
                table: "lines",
                columns: new[] { "city_id", "url_name" });

            migrationBuilder.CreateIndex(
                name: "ix_lines_system_id",
                table: "lines",
                column: "system_id");

            migrationBuilder.CreateIndex(
                name: "ix_lines_transport_mode_id",
                table: "lines",
                column: "transport_mode_id");

            migrationBuilder.CreateIndex(
                name: "ix_section_lines_city_id",
                table: "section_lines",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_section_lines_line_id",
                table: "section_lines",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_section_lines_section_id",
                table: "section_lines",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "ix_sections_city_id",
                table: "sections",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_station_lines_city_id",
                table: "station_lines",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_station_lines_line_id",
                table: "station_lines",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_station_lines_station_id",
                table: "station_lines",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "ix_stations_city_id",
                table: "stations",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_stations_line_id",
                table: "stations",
                column: "line_id");

            migrationBuilder.CreateIndex(
                name: "ix_systems_city_id",
                table: "systems",
                column: "city_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "section_lines");

            migrationBuilder.DropTable(
                name: "station_lines");

            migrationBuilder.DropTable(
                name: "sections");

            migrationBuilder.DropTable(
                name: "stations");

            migrationBuilder.DropTable(
                name: "lines");

            migrationBuilder.DropTable(
                name: "systems");

            migrationBuilder.DropTable(
                name: "transport_modes");

            migrationBuilder.DropTable(
                name: "cities");
        }
    }
}
