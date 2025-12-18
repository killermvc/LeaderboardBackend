using Leaderboard.Models;

namespace Leaderboard.Repositories;

public interface IUserRepository
{
	public Task<User?> GetUserByNameAsync(string username);
	public Task<User?> GetUserByIdAsync(int id);
	public Task AddUserAsync(User user);
	public Task UpdateUserAsync(User user);

	public Task AddRoleToUserAsync(User user, Role role);
}