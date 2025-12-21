namespace Leaderboard.Dtos;

public class LeaderboardEntryDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int Score { get; set; }
}