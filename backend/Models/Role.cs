using System.ComponentModel.DataAnnotations;

namespace Leaderboard.Models;

public class Role
{
	public int Id {get; set;}
	[Required]
	public string Name {get; set;} = null!;
	public List<UserRole> UserRoles { get; set;} = [];
}