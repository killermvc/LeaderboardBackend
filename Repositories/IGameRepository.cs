using Leaderboard.Models;

namespace Leaderboard.Repositories;

public interface IGameRepository
{
	public Task AddAsync(Game game);
	public Task<Game?> GetGameByIdAsync(int id);
	public Task<Game?> GetGameByNameAsync(string name);
}