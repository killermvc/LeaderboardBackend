using Leaderboard.Models;

namespace Leaderboard.Repositories;

public interface IScoreRepository
{
	public Task SubmitScoreAsync(int userId, int gameId, int score);
	public Task<Score?> GetByIdAsync(int id);
	public Task<List<LeaderboardEntry>> GetLeaderboardAsync(int game, int limit);
	public Task<long?> GetRankAsync(int game, int user);
	public Task<List<LeaderboardEntry>> GetTopPlayersAsync(DateTime start_date, DateTime end_date, int limit);
}