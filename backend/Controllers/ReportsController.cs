using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Leaderboard.Models;
using Leaderboard.Repositories;

namespace Leaderboard.Controllers;

[ApiController]
[Route("reports")]
public class ReportsController(IScoreRepository scoreRepository, ILogger<ReportsController> logger) : ControllerBase
{
    private readonly IScoreRepository _scoreRepository = scoreRepository;
    private readonly ILogger _logger = logger;

    [HttpGet("top-players")]
	[Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTopPlayersReport([FromQuery] DateTime start_date, [FromQuery] DateTime end_date, [FromQuery] int limit = 10)
    {
        try
        {
            if (start_date > end_date)
            {
                return BadRequest(new { message = "Start date must be before end date." });
            }

            var topPlayers = await _scoreRepository.GetTopPlayersAsync(start_date, end_date, limit);
            return Ok(new
            {
                start_date,
                end_date,
                top_players = topPlayers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating top players report from {StartDate} to {EndDate}", start_date, end_date);
            return StatusCode(500, new { message = "An error occurred while generating the report." });
        }
    }
}