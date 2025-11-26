namespace Leaderboard.Models;

public class Score
{
	public int Id {get; set;}
	public required User User {get; set;}
	public required Game Game {get; set;}
	public int Value {get; set;}
	public DateTime DateAchieved {get; set;}
}