using Leaderboard.Models;

namespace Leaderboard.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
