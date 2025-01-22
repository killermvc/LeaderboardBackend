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
            return BadRequest("User already exists");
        }

		var user = new User
		{
			Username = request.UserName,
			PasswordHash = BCryptClass.HashPassword(request.Password)
		};
		await _userRepository.AddUserAsync(user);
		return Ok("User registered succesfully");
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] CredentialsRequest request)
	{
		User? user = await _userRepository.GetUserByNameAsync(request.UserName!);

		if(user == null || !BCryptClass.Verify(request.Password, user.PasswordHash))
		{
			return Unauthorized("Invalid credentials");
		}

		string token = GenerateJwtToken(user);

		return Ok(new {Token = token});
	}

	private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
			new (JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };

		foreach(UserRole role in user.Roles)
		{
			claims.Add(new (ClaimTypes.Role, Enum.GetName<UserRole>(role)!));
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