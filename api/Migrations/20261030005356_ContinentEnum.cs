using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
  /// <inheritdoc />
  public partial class ContinentEnum : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
          "UPDATE game_systems SET continent = REPLACE(continent, ' ', '') WHERE continent LIKE '% %';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(@"
                UPDATE game_systems SET continent = 'North America' WHERE continent = 'NorthAmerica';
                UPDATE game_systems SET continent = 'South America' WHERE continent = 'SouthAmerica';");
    }
  }
}