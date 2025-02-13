using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Models;
public class User
{
	[Key]
	public int Id {get; set;}
	[Required]
	public string? Username {get; set;}
	[Required]
	public string? PasswordHash {get; set;}
	public List<UserRole> UserRoles {get; set;} = [];

}