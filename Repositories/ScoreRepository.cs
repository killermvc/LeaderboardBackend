using Leaderboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using StackExchange.Redis;

namespace Leaderboard.Repositories;

public class ScoreRepository(AppDbContext context, ConnectionMultiplexer multiplexer) : IScoreRepository
{

	private readonly AppDbContext _context = context;
	private readonly IDatabase _redisDb = multiplexer.GetDatabase();

	/// <summary>
	/// Submits a score for a user in a specific game.
	/// If the game or user does not exist, a KeyNotFoundException is thrown.
	/// The score is saved in the SQL database and also added to the Redis leaderboard cache.
	/// </summary>
	/// <param name="userId">The ID of the user submitting the score.</param>
	/// <param name="gameId">The ID of the game for which the score is being submitted.</param>
	/// /// <param name="scoreValue">The score value to be submitted.</param>
	/// <exception cref="KeyNotFoundException">Thrown when the specified game or user ID is not found.</exception>
	public async Task SubmitScoreAsync(int userId, int gameId, int scoreValue)
	{
		Game game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId)
			?? throw new KeyNotFoundException($"Game with ID {gameId} not found.");
		User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
			?? throw new KeyNotFoundException($"User with ID {userId} not found");

		var score = new Score{
			Value = scoreValue,
			User = user,
			Game = game,
		};

        _context.Scores.Add(score);
        await _context.SaveChangesAsync();

        var leaderboardKey = $"leaderboard:{score.Game.Id}";
        await _redisDb.SortedSetAddAsync(leaderboardKey, score.User.Id.ToString(), score.Value);
	}

	/// <summary>
	/// Retrieves a score from the SQL database by its ID.
	/// </summary>
	/// <param name="id">The ID of the score to retrieve.</param>
	/// <returns>The score with the specified ID, or null if no such score exists.</returns>

	public async Task<Score?> GetByIdAsync(int id)
	{
		return await _context.Scores.FirstOrDefaultAsync(s => s.Id == id);
	}

	/// <summary>
	/// Updates the leaderboard for a given game in Redis.
	/// It does this by retrieving the scores for the specified game from the SQL database,
	/// ordering them by value descending, and then caching the results in Redis.
	/// </summary>
	/// <param name="gameId">The ID of the game.</param>
	private async Task UpdateRedisDbForGame(int gameId)
	{
		var leaderboardKey = $"leaderboard:{gameId}";

		var lb = await _context.Scores
			.Where(s => s.Game.Id == gameId)
			.Include(s => s.User)
			.OrderByDescending(s => s.Value)
			.Select(s => new { s.User.Id, s.User.Username, s.Value })
			.ToListAsync();

			if (lb.Count == 0)
			{
				throw new KeyNotFoundException($"No scores found for game ID {gameId}");
			}

			// Cache the leaderboard in Redis
			var entries = lb.Select(entry => new SortedSetEntry(entry.Id.ToString(), entry.Value)).ToArray();
			await _redisDb.SortedSetAddAsync(leaderboardKey, entries);
	}

	/// <summary>
	/// Retrieves the top N players from the leaderboard for a given game.
	/// If the leaderboard doesn't exist or is empty in redis, it is cached from the sql database.
	/// </summary>
	/// <param name="gameId">The ID of the game.</param>
	/// <param name="limit">The number of top players to retrieve.</param>
	/// <returns>A list of leaderboard entries.</returns>
	/// <exception cref="KeyNotFoundException">Thrown when no scores are found for the specified game ID.</exception>
	public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int gameId, int limit)
	{
		var leaderboardKey = $"leaderboard:{gameId}";

		// If the leaderboard doesn't exist or is empty, update it
		if (
			!await _redisDb.KeyExistsAsync(leaderboardKey)
			|| await _redisDb.SortedSetLengthAsync(leaderboardKey) == 0)
		{
			try
			{
				await UpdateRedisDbForGame(gameId);
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException($"No scores found for game ID {gameId}");
			}
		}

		// Fetch the top players from Redis
		var leaderboardEntries = await _redisDb.SortedSetRangeByRankWithScoresAsync(leaderboardKey, 0, limit - 1, order: Order.Descending);

		// Map Redis entries to leaderboard model
		 var leaderboard = leaderboardEntries.Select(entry => new LeaderboardEntry
		{
			UserId = int.Parse(entry.Element!),
			UserName = _context.Users.Find(int.Parse(entry.Element!))?.Username, // Get the username from the User entity
			Score = (int)entry.Score
		}).ToList();

		return leaderboard;
	}

	/// <summary>
	/// Retrieves the rank of a specific user in the leaderboard for a given game.
	/// If the leaderboard is not available in Redis, it checks the SQL database for the user's score
	/// and caches it in Redis. If the user is not found in the database, a KeyNotFoundException is thrown.
	/// </summary>
	/// <param name="gameId">The ID of the game.</param>
	/// <param name="userId">The ID of the user whose rank is being retrieved.</param>
	/// <returns>The rank of the user in the leaderboard, or null if the user is not ranked.</returns>
	/// <exception cref="KeyNotFoundException">Thrown when the specified user ID is not found in the specified game.</exception>
	public async Task<long?> GetRankAsync(int gameId, int userId)
	{
		var leaderboardKey = $"leaderboard:{gameId}";

		// Check if leaderboard exists in Redis
		if (!await _redisDb.KeyExistsAsync(leaderboardKey))
		{
			// Check if data exists in the SQL database
			var userScore = await _context.Scores
				.Where(s => s.Game.Id == gameId && s.User.Id == userId)
				.Select(s => new { s.User.Id, s.Value })
				.FirstOrDefaultAsync();

			if (userScore != null)
			{
				// Cache the user's score in Redis
				await _redisDb.SortedSetAddAsync(leaderboardKey, userScore.Id.ToString(), userScore.Value);
			}
			else
			{
				throw new KeyNotFoundException($"User with ID {userId} not found in game ID {gameId}");
			}

		}

		// Fetch the rank of the user from Redis
		var rank = await _redisDb.SortedSetRankAsync(leaderboardKey, userId.ToString());

		return rank.HasValue ? rank + 1 : null;
	}


	/// <summary>
	/// Retrieves the top N players across all games based on their top score submitted
	/// between the given start and end dates.
	/// </summary>
	/// <param name="start_date">The start date of the time period for which to retrieve top players.</param>
	/// <param name="end_date">The end date of the time period for which to retrieve top players.</param>
	/// <param name="limit">The number of top players to retrieve.</param>
	/// <returns>A list of leaderboard entries containing the user ID, username, and score of each player.</returns>
	public async Task<List<LeaderboardEntry>> GetTopPlayersAsync(DateTime start_date, DateTime end_date, int limit)
	{
		var topPlayers = await _context.Scores
			.Where(s => s.DateAchieved >= start_date && s.DateAchieved <= end_date)
			.OrderByDescending(s => s.Value)
			.Take(limit)
			.Select(s => new LeaderboardEntry
			{
				UserId = s.User.Id,
				UserName = s.User.Username,
				Score = s.Value
			})
			.ToListAsync();

		return topPlayers;
	}

}

public class LeaderboardEntry
{
    public int UserId { get; set; }
	public string? UserName {get; set;}
    public int Score { get; set; }
}