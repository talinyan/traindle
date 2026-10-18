using System.ComponentModel.DataAnnotations.Schema;

namespace Traindle.Data;

public enum StatusType { Success, Error, Info };

public class Log
{
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public StatusType Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "jsonb")]
    public string Data { get; set; } = "{}";
}

public enum GameEventType { Fetch, Guess }

public class GameEvent
{
    public int Id { get; set; }
    public GameEventType Type { get; set; }
    public GameKind Game { get; set; }
    public DateOnly? PuzzleDate { get; set; }
    public int? GuessId { get; set; }
    public int? AnswerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


