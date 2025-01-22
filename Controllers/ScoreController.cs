using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Leaderboard.Models;
using Leaderboard.Repositories;

namespace Leaderboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoreController(
		IScoreRepository scoreRepository,
		IGameRepository gameRepository,
		IUserRepository userRepository)
	: ControllerBase
{

	private readonly IScoreRepository _scoreRepository = scoreRepository;
	private readonly IGameRepository _gameRepository = gameRepository;
	private readonly IUserRepository _userRepository = userRepository;

    [HttpPost("submit")]
    [Authorize]
    public async Task<IActionResult> SubmitScore([FromBody] ScoreRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;

        if (userId == null)
        {
            return Unauthorized("Invalid token");
        }

		var user = await _userRepository.GetUserByIdAsync(int.Parse(userId));
		if(user == null)
		{
			return Unauthorized("Invalid token");
		}

		var game = await _gameRepository.GetGameByIdAsync(int.Parse(request.GameId));
		if(game == null)
		{
			return BadRequest("Invalid game ID");
		}

		var score = new Score
		{
			User = user,
			Value = request.Score,
			Game = game,
		};
		await _scoreRepository.AddAsync(score);

        return Ok(new { Message = "Score submitted successfully", UserId = userId });
    }

	[HttpPost("{game_id?}")]
	[Authorize]
	public IActionResult GetGlobalLeaderBoard(int game_id)
	{
		return Ok("");
	}
}

public class ScoreRequest
{
    public required string GameId { get; set; }
    public int Score { get; set; }
}