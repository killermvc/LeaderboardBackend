using Leaderboard.Models;

namespace Leaderboard.Repositories;

public interface IScoreRepository
{
	public Task AddAsync(Score score);
	public Task<Score?> GetByIdAsync(int id);
	public IEnumerable<Score> GetAllFromUser(User user);
	public IEnumerable<Score> GetTopNFromUser(User user, int topN);
	public IEnumerable<Score> GetAllFromUserForGame(User user, Game game);
	public IEnumerable<Score> GetTopNFromUserForGame(User user, Game game, int topN);
	public IEnumerable<Score> GetAllFromGame(Game game);
	public IEnumerable<Score> GetTopNFromGame(Game game, int topN);
}