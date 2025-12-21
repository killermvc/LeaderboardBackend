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
using Leaderboard.Dtos;

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
		public async Task<ActionResult<GameDto>> PostGame(GameDto gameDto)
		{
			var game = new Game { Name = gameDto.Name };
			await _gameRepository.AddAsync(game);
			var resultDto = new GameDto { Id = game.Id, Name = game.Name };
			return CreatedAtAction("GetGame", new { id = game.Id }, resultDto);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<GameDto>> GetGame(int id)
		{
			var game = await _gameRepository.GetGameByIdAsync(id);
			if (game == null)
			{
				return NotFound();
			}
			return new GameDto { Id = game.Id, Name = game.Name };
		}
    }
}
