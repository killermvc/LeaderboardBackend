using System.ComponentModel.DataAnnotations;

namespace Leaderboard.Models;

public class Game
{
	public int Id {get; set;}
	public required string Name {get; set;}
}