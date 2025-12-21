using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


using Leaderboard.Models;
using Leaderboard.Repositories;
using Leaderboard.Dtos;

namespace Leaderboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreController(
		IScoreRepository scoreRepository,
		IGameRepository gameRepository,
		IUserRepository userRepository,
		ILogger<ScoreController> logger
	) : ControllerBase
{

	private readonly IScoreRepository _scoreRepository = scoreRepository;
	private readonly IGameRepository _gameRepository = gameRepository;
	private readonly IUserRepository _userRepository = userRepository;
	private readonly ILogger _logger = logger;

	//POST /scores
    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> SubmitScore([FromBody] ScoreRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.Identity?.Name;

		if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("Invalid user token.");
        }

        try
        {
            // Submit the score through the repository
            await _scoreRepository.SubmitScoreAsync(userId, request.GameId, request.Score);
            return Ok("Score submitted successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting score.");
            return StatusCode(500, "An error occurred while submitting the score.");
        }
    }

	//GET /leaderboard/<game_id>
    [HttpGet]
    [Route("leaderboard/{gameId}")]
    public async Task<IActionResult> GetLeaderboard(int gameId, [FromQuery] int limit = 10)
    {
        try
        {
            var leaderboard = await _scoreRepository.GetLeaderboardAsync(gameId, limit);
			return Ok(leaderboard);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leaderboard.");
            return StatusCode(500, "An error occurred while retrieving the leaderboard.");
        }
    }

	//GET /leaderboard/<game_id>/rank/<user_id>
	[HttpGet]
	[Route("leaderboard/{gameId}/rank/{userId}")]
	public async Task<IActionResult> GetRank(int gameId, int userId)
	{
		try
		{
			var rank = await _scoreRepository.GetRankAsync(gameId, userId);
			if(rank == null)
			{
				return NotFound("User not found in leaderboard.");
			}
			return Ok(new
            {
                gameId,
                userId,
                rank
            });

		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving rank.");
			return StatusCode(500, "An error occurred while retrieving the rank.");
		}
	}

	//GET /leaderboard/<game_id>/top/<N>
	[HttpGet]
	[Route("leaderboard/{gameId}/top/{limit}")]
	public async Task<IActionResult> GetTopPlayers(int gameId, int limit)
	{
		try
		{
			var leaderboard = await _scoreRepository.GetLeaderboardAsync(gameId, limit);
			return Ok(new
            {
                gameId,
				limit,
                leaderboard
            });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving leaderboard.");
			return StatusCode(500, "An error occurred while retrieving the leaderboard.");
		}
	}

	[HttpGet]
	[Route("scores/user/{userId}")]
	public async Task<IActionResult> GetScoresByUser(int userId, [FromQuery] int limit = 10, [FromQuery] int offset = 0)
	{
		try
		{
			var scores = await _scoreRepository.GetScoresByUserAsync(userId, limit, offset);
			var dtos = scores.Select(s => new ScoreDto
			{
				Id = s.Id,
				User = s.User == null ? null : new UserDto { Id = s.User.Id, Username = s.User?.Username ?? string.Empty },
				Game = s.Game == null ? null : new GameDto { Id = s.Game.Id, Name = s.Game.Name },
				Value = s.Value,
				DateAchieved = s.DateAchieved
			}).ToList();

			return Ok(dtos);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving scores by user.");
			return StatusCode(500, "An error occurred while retrieving the scores.");
		}
	}

	[HttpGet]
	[Route("scores/recent")]
	public async Task<IActionResult> GetRecentScores([FromQuery] int limit = 10, [FromQuery] int offset = 0)
	{
		try
		{
			var scores = await _scoreRepository.GetRecentScoresAsync(limit, offset);
			var dtos = scores.Select(s => new ScoreDto
			{
				Id = s.Id,
				User = s.User == null ? null : new UserDto { Id = s.User.Id, Username = s.User?.Username ?? string.Empty },
				Game = s.Game == null ? null : new GameDto { Id = s.Game.Id, Name = s.Game.Name },
				Value = s.Value,
				DateAchieved = s.DateAchieved
			}).ToList();

			return Ok(dtos);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving recent scores.");
			return StatusCode(500, "An error occurred while retrieving the recent scores.");
		}
	}
}


public class ScoreRequest
{
    public required int GameId { get; set; }
    public int Score { get; set; }
}