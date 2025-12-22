using Microsoft.AspNetCore.Mvc;
using BCryptClass = BCrypt.Net.BCrypt;
using System.Security.Claims;

using Leaderboard.Repositories;
using Leaderboard.Models;
using Leaderboard.Services;
using Microsoft.AspNetCore.Authorization;

namespace Leaderboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserRepository userRepository, IJwtService jwtService) : ControllerBase
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly IJwtService _jwtService = jwtService;

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] CredentialsRequest request)
	{
		if (await _userRepository.GetUserByNameAsync(request.UserName!) != null)
        {
            return BadRequest(new { Message = "Username already exists" });
        }

		var user = new User
		{
			Username = request.UserName,
			PasswordHash = BCryptClass.HashPassword(request.Password)
		};
		await _userRepository.AddUserAsync(user);
		return Ok(new { Message = "User registered successfully" });
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] CredentialsRequest request)
	{
		User? user = await _userRepository.GetUserByNameAsync(request.UserName!);

		if(user == null || !BCryptClass.Verify(request.Password, user.PasswordHash))
		{
			return Unauthorized(new {Message = "Invalid username or password"});
		}

		string token = _jwtService.GenerateToken(user);

		return Ok(new {Token = token, Message = "Login successful"});
	}

	[HttpPut]
	[Authorize]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
	{
		var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
		if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
		{
			return Unauthorized(new { Message = "Invalid token" });
		}

		User? user = await _userRepository.GetUserByIdAsync(userId);

		if(user == null || !BCryptClass.Verify(request.OldPassword, user.PasswordHash))
		{
			return Unauthorized(new {Message = "Invalid password"});
		}

		user.PasswordHash = BCryptClass.HashPassword(request.NewPassword);
		await _userRepository.UpdateUserAsync(user);

		return Ok(new {Message = "Password changed successfully"});
	}

	[HttpPut("promote/{userId}")]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> PromoteToAdmin([FromRoute] int userId)
	{
		User? user = await _userRepository.GetUserByIdAsync(userId);

		if(user == null)
		{
			return NotFound(new {Message = "User not found"});
		}

		await _userRepository.AddRoleToUserAsync(user, new Role { Name = "Admin" });

		return Ok(new {Message = "User promoted to Admin successfully"});
	}

	[HttpPut("username")]
	[Authorize]
	public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
	{
		var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
		if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
		{
			return Unauthorized(new { Message = "Invalid token" });
		}

		if (string.IsNullOrWhiteSpace(request.NewUserName))
			return BadRequest(new { Message = "Invalid username" });

		if (await _userRepository.GetUserByNameAsync(request.NewUserName!) != null)
			return BadRequest(new { Message = "Username already exists" });

		User? user = await _userRepository.GetUserByIdAsync(userId);
		if (user == null)
			return NotFound(new { Message = "User not found" });

		user.Username = request.NewUserName;
		await _userRepository.UpdateUserAsync(user);

		return Ok(new { Message = "Username updated successfully" });
	}
}

public class CredentialsRequest
{
	public string? UserName {get; set;}
	public string? Password {get; set;}
}

public class UpdateUsernameRequest
{
	public string? NewUserName { get; set; }
}

public class ChangePasswordRequest
{
	public string? OldPassword { get; set; }
	public string? NewPassword { get; set; }
}