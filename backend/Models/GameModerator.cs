namespace Leaderboard.Models;

/// <summary>
/// Represents a moderator assignment for a specific game.
/// Game moderators can approve or reject scores submitted for their assigned games.
/// </summary>
public class GameModerator
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
