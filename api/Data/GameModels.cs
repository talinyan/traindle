namespace Traindle.Data;

// Processed data
public class GameSystem
{
    public int Id { get; set; }                 // TransportSystem.Id
    public string Name { get; set; } = "";
    public int CityId { get; set; }
    public string City { get; set; } = "";
    public string? Country { get; set; }
    public string? CountryState { get; set; }
    public string? Continent { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<string> Modes { get; set; } = new();
    public int LineCount { get; set; }
    public int StationCount { get; set; }
    public int InterchangeCount { get; set; }   // stations served by 2+ of its lines
    public int Length { get; set; }             // in metres
    public int OpeningYear { get; set; }

    public List<GameLine> Lines { get; set; } = new();
}

public class GameLine
{
    public int Id { get; set; }                 // Line.Id
    public int SystemId { get; set; }
    public string Name { get; set; } = "";
    public string? Colour { get; set; }
    public string Mode { get; set; } = "";
    public int StationCount { get; set; }
    public int Length { get; set; }             // in metres
    public int OpeningYear { get; set; }
    public string Geometry { get; set; } = "";

    public GameSystem? System { get; set; }
}

public enum GameKind { Systems, Lines }

// The answer for a game per day
public class DailyPuzzle
{
    public DateOnly Date { get; set; }
    public GameKind Game { get; set; }
    public int TargetId { get; set; }
}
public record DailyPuzzleResponse(DateOnly Date, int PuzzleNumber);

public class RandomPuzzle
{
    public GameKind Game { get; set; }
    public int TargetId { get; set; }
}
public record RandomPuzzleResponse(GameKind game, int Id);

// How the answer compares to the guess
public enum Comparison { Lower, Equal, Higher }
public enum ModeMatch { None, Partial, Exact }

public record GameSystemSummary(int Id, string Name, string City, string? Country, string? Continent);

// Entire doc
public record GameSystemResponse(
    int Id, string Name, string City, string? Country, string? CountryState, string? Continent,
    double? Latitude, double? Longitude, List<string> Modes,
    int LineCount, int StationCount, int InterchangeCount, int Length, int OpeningYear,
    List<GameLineSummary> Lines);

// Limited doc for autocomplete
public record GameSystemOption(
    int Id, string Name, string City, string? Country, string? Continent,
    List<string> Modes, int LineCount, int StationCount, int Length, int OpeningYear);

public record GameLineSummary(int Id, string Name, string? Colour, string Mode);

// Entire doc
public record GameLineResponse(
    int Id, string Name, string? Colour, string Mode,
    int StationCount, int Length, int OpeningYear, string Geometry,
    int SystemId, GameSystemSummary System);

// Limited doc for autocomplete
public record GameLineOption(
    int Id, string Name, string? Colour, string Mode, int StationCount, int Length,
    int OpeningYear, int SystemId, string SystemName, string City, string? Country);

public record SystemDailyGuessRequest(DateOnly Date, int SystemId);
public record SystemRandomGuessRequest(int AnswerId, int SystemId);

public record LineDailyGuessRequest(DateOnly Date, int LineId);
public record LineRandomGuessRequest(int AnswerId, int LineId);

// DistanceKm/Bearing are null when either city has no coordinates; Bearing is degrees clockwise from north, guess -> answer
public record SystemGuessResponse(
    int GuessId, bool Correct,
    Comparison LineCount, double? DistanceKm, double? Bearing,
    Comparison OpeningYear, Comparison Length, Comparison StationCount,
    ModeMatch ModeMatch, List<string> CorrectModes);

public record LineGuessResponse(
    int GuessId, bool Correct,
    double? DistanceKm, double? Bearing,      // between the two lines' cities, same as for systems
    bool ModeCorrect, Comparison OpeningYear, Comparison Length, Comparison StationCount);
