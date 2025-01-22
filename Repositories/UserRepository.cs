using Leaderboard.Models;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task<User?> GetUserByNameAsync(string username)
	{
		return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
	}

	public async Task<User?> GetUserByIdAsync(int id)
	{
		return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
	}

    public async Task AddUserAsync(User user)
	{
		await _context.Users.AddAsync(user);
		await _context.SaveChangesAsync();
	}
}