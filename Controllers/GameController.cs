using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using Leaderboard.Models;
using Leaderboard.Repositories;

namespace Leaderboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController(AppDbContext context, IGameRepository gameRepository) : ControllerBase
    {
        private readonly AppDbContext _context = context;
		private readonly IGameRepository _gameRepository = gameRepository;

		// POST: api/Games
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<Game>> PostGame(Game game)
		{
			await _gameRepository.AddAsync(game);
			return CreatedAtAction("GetGame", new { id = game.Id }, game);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Game>> GetGame(int id)
		{
			var game = await _gameRepository.GetGameByIdAsync(id);
			if (game == null)
			{
				return NotFound();
			}
			return game;
		}
    }
}
