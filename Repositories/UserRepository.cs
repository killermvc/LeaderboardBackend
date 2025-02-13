using Leaderboard.Models;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

	/// <summary>
	/// Retrieves a user by their username from the database.
	/// </summary>
	/// <param name="username">The username of the user to retrieve.</param>
	/// <returns>The user with the specified username, or null if not found.</returns>
    public async Task<User?> GetUserByNameAsync(string username)
	{
		return await _context.Users
			.Include(u => u.UserRoles)
			.ThenInclude(ur => ur.Role)
			.FirstOrDefaultAsync(u => u.Username == username);
	}

	/// <summary>
	/// Retrieves a user by their ID from the database.
	/// </summary>
	/// <param name="id">The ID of the user to retrieve.</param>
	/// <returns>The user with the specified ID, or null if not found.</returns>

	public async Task<User?> GetUserByIdAsync(int id)
	{
		return await _context.Users
			.Include(u => u.UserRoles)
			.ThenInclude(ur => ur.Role)
			.FirstOrDefaultAsync(u => u.Id == id);
	}

	/// <summary>
	/// Adds a new user to the database asynchronously.
	/// </summary>
	/// <param name="user">The user entity to add.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddUserAsync(User user)
	{
		await _context.Users.AddAsync(user);
		await _context.SaveChangesAsync();
	}
}