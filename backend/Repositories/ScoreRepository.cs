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
	/// The score is saved with a Pending status and must be approved before appearing on leaderboards.
	/// If the game or user does not exist, a KeyNotFoundException is thrown.
	/// If the new score is lower than or equal to the user's existing highest approved score, an InvalidOperationException is thrown.
	/// </summary>
	/// <param name="userId">The ID of the user submitting the score.</param>
	/// <param name="gameId">The ID of the game for which the score is being submitted.</param>
	/// <param name="scoreValue">The score value to be submitted.</param>
	/// <exception cref="KeyNotFoundException">Thrown when the specified game or user ID is not found.</exception>
	/// <exception cref="InvalidOperationException">Thrown when the new score is not higher than the existing highest approved score.</exception>
	public async Task SubmitScoreAsync(int userId, int gameId, int scoreValue)
	{
		Game game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId)
			?? throw new KeyNotFoundException($"Game with ID {gameId} not found.");
		User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
			?? throw new KeyNotFoundException($"User with ID {userId} not found");

		// Check if the user already has a higher or equal approved score for this game
		var existingHighScore = await _context.Scores
			.Where(s => s.User.Id == userId && s.Game.Id == gameId && s.Status == ScoreStatus.Approved)
			.MaxAsync(s => (int?)s.Value);

		if (existingHighScore.HasValue && scoreValue <= existingHighScore.Value)
		{
			throw new InvalidOperationException($"New score ({scoreValue}) must be higher than current high score ({existingHighScore.Value}).");
		}

		var score = new Score
		{
			Value = scoreValue,
			User = user,
			Game = game,
			Status = ScoreStatus.Pending // Score starts as pending
		};

		_context.Scores.Add(score);
		await _context.SaveChangesAsync();

		// Note: Score is NOT added to Redis leaderboard until approved
	}

	/// <summary>
	/// Retrieves a score from the SQL database by its ID.
	/// </summary>
	/// <param name="id">The ID of the score to retrieve.</param>
	/// <returns>The score with the specified ID, or null if no such score exists.</returns>

	public async Task<Score?> GetByIdAsync(int id)
	{
		return await _context.Scores
			.Include(s => s.User)
			.Include(s => s.Game)
			.Include(s => s.ReviewedBy)
			.FirstOrDefaultAsync(s => s.Id == id);
	}

	/// <summary>
	/// Updates the leaderboard for a given game in Redis.
	/// Only includes approved scores.
	/// </summary>
	/// <param name="gameId">The ID of the game.</param>
	private async Task UpdateRedisDbForGame(int gameId)
	{
		var leaderboardKey = $"leaderboard:{gameId}";

		// Get only the highest approved score per user for this game
		var lb = await _context.Scores
			.Where(s => s.Game.Id == gameId && s.Status == ScoreStatus.Approved)
			.Include(s => s.User)
			.GroupBy(s => new { s.User.Id, s.User.Username })
			.Select(g => new { g.Key.Id, g.Key.Username, Value = g.Max(s => s.Value) })
			.OrderByDescending(s => s.Value)
			.ToListAsync();

		if (lb.Count == 0)
		{
			throw new KeyNotFoundException($"No approved scores found for game ID {gameId}");
		}

		// Cache the leaderboard in Redis
		var entries = lb.Select(entry => new SortedSetEntry(entry.Id.ToString(), entry.Value)).ToArray();
		await _redisDb.SortedSetAddAsync(leaderboardKey, entries);
	}

	/// <summary>
	/// Retrieves the top N players from the leaderboard for a given game.
	/// Only approved scores are included.
	/// If the leaderboard doesn't exist or is empty in redis, it is cached from the sql database.
	/// </summary>
	/// <param name="gameId">The ID of the game.</param>
	/// <param name="limit">The number of top players to retrieve.</param>
	/// <returns>A list of leaderboard entries.</returns>
	/// <exception cref="KeyNotFoundException">Thrown when no approved scores are found for the specified game ID.</exception>
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
				throw new KeyNotFoundException($"No approved scores found for game ID {gameId}");
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
	/// Only considers approved scores.
	/// If the leaderboard is not available in Redis, it checks the SQL database for the user's approved score
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
			// Check if approved data exists in the SQL database
			var userScore = await _context.Scores
				.Where(s => s.Game.Id == gameId && s.User.Id == userId && s.Status == ScoreStatus.Approved)
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
	/// Retrieves the top N players across all games based on their top approved score submitted
	/// between the given start and end dates.
	/// </summary>
	/// <param name="start_date">The start date of the time period for which to retrieve top players.</param>
	/// <param name="end_date">The end date of the time period for which to retrieve top players.</param>
	/// <param name="limit">The number of top players to retrieve.</param>
	/// <returns>A list of leaderboard entries containing the user ID, username, and score of each player.</returns>
	public async Task<List<LeaderboardEntry>> GetTopPlayersAsync(DateTime start_date, DateTime end_date, int limit)
	{
		var topPlayers = await _context.Scores
			.Where(s => s.DateAchieved >= start_date && s.DateAchieved <= end_date && s.Status == ScoreStatus.Approved)
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

	public Task<List<Score>> GetScoresByUserAsync(int userId, int limit, int offset)
	{
		return _context.Scores
			.AsNoTracking()
			.Include(s => s.User)
			.Include(s => s.Game)
			.Where(s => s.User.Id == userId && s.Status == ScoreStatus.Approved)
			.OrderByDescending(s => s.DateAchieved)
			.Skip(offset)
			.Take(limit)
			.ToListAsync();
	}

	/// <summary>
	/// Gets all scores by a user, including pending and rejected ones.
	/// For the user to see their own submissions.
	/// </summary>
	public Task<List<Score>> GetAllScoresByUserAsync(int userId, int limit, int offset)
	{
		return _context.Scores
			.AsNoTracking()
			.Include(s => s.User)
			.Include(s => s.Game)
			.Include(s => s.ReviewedBy)
			.Where(s => s.User.Id == userId)
			.OrderByDescending(s => s.DateAchieved)
			.Skip(offset)
			.Take(limit)
			.ToListAsync();
	}

	public Task<List<Score>> GetRecentScoresAsync(int limit, int offset)
	{
		return _context.Scores
			.AsNoTracking()
			.Include(s => s.User)
			.Include(s => s.Game)
			.Where(s => s.Status == ScoreStatus.Approved)
			.OrderByDescending(s => s.DateAchieved)
			.Skip(offset)
			.Take(limit)
			.ToListAsync();
	}

	// ==================== Score Approval Methods ====================

	/// <summary>
	/// Approves a pending score and adds it to the Redis leaderboard.
	/// </summary>
	public async Task ApproveScoreAsync(int scoreId, int moderatorId)
	{
		var score = await _context.Scores
			.Include(s => s.User)
			.Include(s => s.Game)
			.FirstOrDefaultAsync(s => s.Id == scoreId)
			?? throw new KeyNotFoundException($"Score with ID {scoreId} not found.");

		if (score.Status != ScoreStatus.Pending)
		{
			throw new InvalidOperationException($"Score is not pending. Current status: {score.Status}");
		}

		var moderator = await _context.Users.FirstOrDefaultAsync(u => u.Id == moderatorId)
			?? throw new KeyNotFoundException($"Moderator with ID {moderatorId} not found.");

		score.Status = ScoreStatus.Approved;
		score.ReviewedBy = moderator;
		score.ReviewedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync();

		// Add the approved score to Redis leaderboard
		var leaderboardKey = $"leaderboard:{score.Game.Id}";

		// Get the user's current highest approved score for this game
		var highestScore = await _context.Scores
			.Where(s => s.User.Id == score.User.Id && s.Game.Id == score.Game.Id && s.Status == ScoreStatus.Approved)
			.MaxAsync(s => (int?)s.Value) ?? 0;

		// Update Redis with the highest score
		await _redisDb.SortedSetAddAsync(leaderboardKey, score.User.Id.ToString(), highestScore);
	}

	/// <summary>
	/// Rejects a pending score with an optional reason.
	/// </summary>
	public async Task RejectScoreAsync(int scoreId, int moderatorId, string? reason = null)
	{
		var score = await _context.Scores
			.Include(s => s.User)
			.Include(s => s.Game)
			.FirstOrDefaultAsync(s => s.Id == scoreId)
			?? throw new KeyNotFoundException($"Score with ID {scoreId} not found.");

		if (score.Status != ScoreStatus.Pending)
		{
			throw new InvalidOperationException($"Score is not pending. Current status: {score.Status}");
		}

		var moderator = await _context.Users.FirstOrDefaultAsync(u => u.Id == moderatorId)
			?? throw new KeyNotFoundException($"Moderator with ID {moderatorId} not found.");

		score.Status = ScoreStatus.Rejected;
		score.ReviewedBy = moderator;
		score.ReviewedAt = DateTime.UtcNow;
		score.RejectionReason = reason;

		// Explicitly mark RejectionReason as modified to ensure EF tracks it
		_context.Entry(score).Property(s => s.RejectionReason).IsModified = true;

		await _context.SaveChangesAsync();
	}

	/// <summary>
	/// Gets all pending scores for a specific game.
	/// </summary>
	public async Task<List<Score>> GetPendingScoresForGameAsync(int gameId, int limit, int offset)
	{
		return await _context.Scores
			.AsNoTracking()
			.Include(s => s.User)
			.Include(s => s.Game)
			.Where(s => s.Game.Id == gameId && s.Status == ScoreStatus.Pending)
			.OrderBy(s => s.DateAchieved)
			.Skip(offset)
			.Take(limit)
			.ToListAsync();
	}

	/// <summary>
	/// Gets all pending scores across all games.
	/// </summary>
	public async Task<List<Score>> GetAllPendingScoresAsync(int limit, int offset)
	{
		return await _context.Scores
			.AsNoTracking()
			.Include(s => s.User)
			.Include(s => s.Game)
			.Where(s => s.Status == ScoreStatus.Pending)
			.OrderBy(s => s.DateAchieved)
			.Skip(offset)
			.Take(limit)
			.ToListAsync();
	}

	/// <summary>
	/// Gets pending scores for games that have no specific moderators.
	/// </summary>
	public async Task<List<Score>> GetPendingScoresForUnmoderatedGamesAsync(int limit, int offset)
	{
		// Use a subquery to find games without moderators - this translates to SQL properly
		return await _context.Scores
			.AsNoTracking()
			.Include(s => s.User)
			.Include(s => s.Game)
			.Where(s => s.Status == ScoreStatus.Pending &&
				!_context.GameModerators.Any(gm => gm.GameId == s.Game.Id))
			.OrderBy(s => s.DateAchieved)
			.Skip(offset)
			.Take(limit)
			.ToListAsync();
	}
}

public class LeaderboardEntry
{
    public int UserId { get; set; }
	public string? UserName {get; set;}
    public int Score { get; set; }
}
