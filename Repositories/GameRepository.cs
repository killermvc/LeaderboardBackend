using Leaderboard.Models;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Repositories;

public class GameRepository(AppDbContext context) : IGameRepository
{
	private AppDbContext _context = context;

	public async Task AddAsync(Game game)
	{
		await _context.Games.AddAsync(game);
		await _context.SaveChangesAsync();
	}

	public async Task<Game?> GetGameByIdAsync(int id)
	{
		return await _context.Games.FirstOrDefaultAsync(g => g.Id == id);
	}
	public async Task<Game?> GetGameByNameAsync(string name)
	{
		return await _context.Games.FirstOrDefaultAsync(g => g.Name == name);
	}
}