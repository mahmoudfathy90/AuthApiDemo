using AuthApiDemo.Domain.Entities;

namespace AuthApiDemo.Domain.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User> CreateUserAsync(string firstName, string lastName, string email, string gender, bool active = true);
        Task<User> UpdateUserAsync(int userId, string firstName, string lastName, string gender);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> UserExistsAsync(string email);
    }
}