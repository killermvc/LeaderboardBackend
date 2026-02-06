using System;
using Leaderboard.Models;

namespace Leaderboard.Dtos;

public class ScoreDto
{
    public int Id { get; set; }
    public UserDto? User { get; set; }
    public GameDto? Game { get; set; }
    public int Value { get; set; }
    public DateTime DateAchieved { get; set; }
    public ScoreStatus Status { get; set; }
    public UserDto? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}