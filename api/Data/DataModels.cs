using CsvHelper.Configuration.Attributes;

namespace Traindle.Data;

// 0/-1/null -> unknown opening year
// 999999 -> never closed

public class City
{
    [Name("id")] public int Id { get; set; }
    [Name("name")] public string? Name { get; set; }
    [Name("coords")] public string? Coords { get; set; }
    [Name("start_year")] public int StartYear { get; set; }
    [Name("url_name")] public string? UrlName { get; set; }
    [Name("country")] public string? Country { get; set; }
    [Name("country_state"), Optional] public string? CountryState { get; set; }
    [Name("length")] public long Length { get; set; }

    [Ignore] public List<TransportSystem> Systems { get; set; } = new();
    [Ignore] public List<Line> Lines { get; set; } = new();
    [Ignore] public List<Station> Stations { get; set; } = new();
    [Ignore] public List<StationLine> StationLines { get; set; } = new();
    [Ignore] public List<Section> Sections { get; set; } = new();
    [Ignore] public List<SectionLine> SectionLine { get; set; } = new();

}

public class TransportSystem
{
    [Name("id")] public int Id { get; set; }
    [Name("city_id")] public int CityId { get; set; }
    [Name("name")] public string? Name { get; set; }
    [Name("historic")] public bool Historic { get; set; }
    [Name("length")] public long Length { get; set; }

    [Ignore] public City? City { get; set; }
    [Ignore] public List<Line> Lines { get; set; } = new();
}

public class TransportMode
{
    [Name("id")] public int Id { get; set; }
    [Name("name")] public string? Name { get; set; }

    [Ignore] public List<Line> Lines { get; set; } = new();
}

public class Line
{
    [Name("id")] public int Id { get; set; }
    [Name("city_id")] public int CityId { get; set; }
    [Name("name")] public string? Name { get; set; }
    [Name("url_name")] public string? UrlName { get; set; }
    [Name("color")] public string? Color { get; set; }
    [Name("system_id")] public int SystemId { get; set; }
    [Name("transport_mode_id")] public int TransportModeId { get; set; }

    [Ignore] public City? City { get; set; }
    [Ignore] public TransportSystem? TransportSystem { get; set; }
    [Ignore] public TransportMode? TransportMode { get; set; }
    [Ignore] public List<Station> Stations { get; set; } = new();
    [Ignore] public List<StationLine> StationLines { get; set; } = new();
    [Ignore] public List<SectionLine> SectionLine { get; set; } = new();
}

public class Station
{
    [Name("id")] public int Id { get; set; }
    [Name("name")] public string? Name { get; set; }
    [Name("geometry")] public string? Geometry { get; set; }
    [Name("buildstart")] public int? BuildStart { get; set; }
    [Name("opening")] public int? Opening { get; set; }
    [Name("closure")] public int? Closure { get; set; }
    [Name("city_id")] public int CityId { get; set; }

    [Ignore] public City? City { get; set; }
    [Ignore] public List<StationLine> StationLines { get; set; } = new();
}

public class StationLine
{
    [Name("id")] public int Id { get; set; }
    [Name("station_id")] public int StationId { get; set; }
    [Name("line_id")] public int LineId { get; set; }
    [Name("city_id")] public int CityId { get; set; }
    [Name("created_at")] public DateTime CreatedAt { get; set; }
    [Name("updated_at")] public DateTime UpdatedAt { get; set; }
    [Name("fromyear")] public int? FromYear { get; set; }
    [Name("toyear")] public int? ToYear { get; set; }
    [Name("deprecated_line_group")] public int DeprecatedLineGroup { get; set; }

    [Ignore] public Station? Station { get; set; }
    [Ignore] public Line? Line { get; set; }
    [Ignore] public City? City { get; set; }
}

public class Section
{
    [Name("id")] public int Id { get; set; }
    [Name("geometry")] public string? Geometry { get; set; }
    [Name("buildstart")] public int? BuildStart { get; set; }
    [Name("opening")] public int? Opening { get; set; }
    [Name("closure")] public int? Closure { get; set; }
    [Name("length")] public int Length { get; set; }
    [Name("city_id")] public int CityId { get; set; }

    [Ignore] public City? City { get; set; }
    [Ignore] public List<SectionLine> SectionLines { get; set; } = new();
}

public class SectionLine
{
    [Name("id")] public int Id { get; set; }
    [Name("section_id")] public int SectionId { get; set; }
    [Name("line_id")] public int LineId { get; set; }
    [Name("created_at")] public DateTime CreatedAt { get; set; }
    [Name("updated_at")] public DateTime UpdatedAt { get; set; }
    [Name("city_id")] public int CityId { get; set; }
    [Name("fromyear")] public int? FromYear { get; set; }
    [Name("toyear")] public int? ToYear { get; set; }
    [Name("deprecated_line_group")] public int DeprecatedLineGroup { get; set; }

    [Ignore] public Section? Section { get; set; }
    [Ignore] public Line? Line { get; set; }
    [Ignore] public City? City { get; set; }
}