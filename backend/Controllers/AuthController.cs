using Microsoft.AspNetCore.Mvc;
using BCryptClass = BCrypt.Net.BCrypt;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

using Leaderboard.Repositories;
using Leaderboard.Models;
using Microsoft.OpenApi.Extensions;

namespace Leaderboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration config, IUserRepository userRepository) : ControllerBase
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly IConfiguration _configuration = config;

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

		string token = GenerateJwtToken(user);

		return Ok(new {Token = token, Message = "Login successful"});
	}

	[HttpPut]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
	{
		User? user = await _userRepository.GetUserByNameAsync(request.UserName!);

		if(user == null || !BCryptClass.Verify(request.OldPassword, user.PasswordHash))
		{
			return Unauthorized(new {Message = "Invalid username or password"});
		}

		user.PasswordHash = BCryptClass.HashPassword(request.NewPassword);
		await _userRepository.UpdateUserAsync(user);

		return Ok(new {Message = "Password changed successfully"});
	}

	[HttpPut("promote/{userId}")]
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
	public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.OldUserName) || string.IsNullOrWhiteSpace(request.NewUserName))
			return BadRequest(new { Message = "Invalid username" });

		if (await _userRepository.GetUserByNameAsync(request.NewUserName!) != null)
			return BadRequest(new { Message = "Username already exists" });

		User? user = await _userRepository.GetUserByNameAsync(request.OldUserName!);
		if (user == null)
			return NotFound(new { Message = "User not found" });

		user.Username = request.NewUserName;
		await _userRepository.UpdateUserAsync(user);

		return Ok(new { Message = "Username updated successfully" });
	}
	private string GenerateJwtToken(User user)
	{
        var claims = new List<Claim>
		{
			new(ClaimTypes.Name, user.Id.ToString())
		};

		foreach(UserRole userRole in user.UserRoles)
		{
			claims.Add(new (ClaimTypes.Role, userRole.Role.Name));
		}

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
			issuer: _configuration["Jwt:Issuer"],
			audience: _configuration["Jwt:Audience"],
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}

public class CredentialsRequest
{
	public string? UserName {get; set;}
	public string? Password {get; set;}
}

public class UpdateUsernameRequest
{
	public string? OldUserName { get; set; }
	public string? NewUserName { get; set; }
}

public class ChangePasswordRequest
{
	public string? UserName { get; set; }
	public string? OldPassword { get; set; }
	public string? NewPassword { get; set; }
}