using Leaderboard.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Leaderboard.Repositories;

public class ScoreRepository(AppDbContext context, ConnectionMultiplexer multiplexer) : IScoreRepository
{

	private readonly AppDbContext _context = context;
	private readonly IDatabase _redisDb = multiplexer.GetDatabase();

	public async Task AddAsync(Score score)
	{

		await _context.Scores.AddAsync(score);
		await _context.SaveChangesAsync();
	}
	public async Task<Score?> GetByIdAsync(int id)
	{
		return await _context.Scores.FirstOrDefaultAsync(s => s.Id == id);
	}
	public IEnumerable<Score> GetAllFromUser(User user)
	{
		return _context.Scores.Where(s => s.User.Id == user.Id);
	}
	public IEnumerable<Score> GetTopNFromUser(User user, int topN)
	{
		return _context.Scores
			.Where(s => s.User.Id == user.Id)
			.OrderByDescending(s => s.Value)
			.Take(topN);
	}

	public IEnumerable<Score> GetAllFromUserForGame(User user, Game game)
	{
		return _context.Scores.Where(s => s.User.Id == user.Id && s.Game.Id == game.Id);
	}

	public IEnumerable<Score> GetTopNFromUserForGame(User user, Game game, int topN)
	{
		return _context.Scores
			.Where(s => s.User.Id == user.Id && s.Game.Id == game.Id)
			.OrderByDescending(s => s.Value)
			.Take(topN);
	}

	public IEnumerable<Score> GetAllFromGame(Game game)
	{
		return _context.Scores.Where(s => s.Game.Id == game.Id);
	}

	public IEnumerable<Score> GetTopNFromGame(Game game, int topN)
	{
		return _context.Scores
			.Where(s => s.Game.Id == game.Id)
			.OrderByDescending(s => s.Value)
			.Take(topN);
	}

}