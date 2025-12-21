using System;

namespace Leaderboard.Dtos;

public class ScoreDto
{
    public int Id { get; set; }
    public UserDto? User { get; set; }
    public GameDto? Game { get; set; }
    public int Value { get; set; }
    public DateTime DateAchieved { get; set; }
}