namespace Leaderboard.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int ScoresCount { get; set; }
    public int GamesPlayedCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}