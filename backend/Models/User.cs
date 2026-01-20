using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Models;
public class User
{
	[Key]
	public int Id {get; set;}
	[Required]
	public string Username {get; set;} = string.Empty;
	[Required]
	public string PasswordHash {get; set;} = string.Empty;
	public List<UserRole> UserRoles {get; set;} = [];
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}