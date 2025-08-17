using AuthApiDemo.Domain.Entities;

namespace AuthApiDemo.Domain.Interfaces
{
    public interface IUserAuthRepository
    {
        Task<UserAuth?> GetByEmailAsync(string email);
        Task<UserAuth?> GetByUserIdAsync(int userId);
        Task<UserAuth> CreateAsync(UserAuth userAuth);
        Task<UserAuth> UpdateAsync(UserAuth userAuth);
        Task<bool> DeleteAsync(int userAuthId);
        Task<bool> DeleteByUserIdAsync(int userId);
        Task<bool> ExistsAsync(string email);
    }
}