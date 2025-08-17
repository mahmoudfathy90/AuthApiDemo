using AuthApiDemo.Domain.Entities;

namespace AuthApiDemo.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string password, string firstName, string lastName, string gender);
        Task<(bool Success, string Message, User? User, string? Token, string? RefreshToken, DateTime? Expiration)> LoginAsync(string email, string password);
        Task<(bool Success, string Message, string? Token, string? RefreshToken, DateTime? Expiration)> RefreshTokenAsync(int userId, string refreshToken);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
    }
}