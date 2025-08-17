using AuthApiDemo.Domain.Entities;

namespace AuthApiDemo.Domain.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        DateTime GetTokenExpiration();
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
    }
}