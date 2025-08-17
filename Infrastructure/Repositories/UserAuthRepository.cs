using Microsoft.EntityFrameworkCore;
using AuthApiDemo.Domain.Entities;
using AuthApiDemo.Domain.Interfaces;
using AuthApiDemo.Infrastructure.Data;

namespace AuthApiDemo.Infrastructure.Repositories
{
    public class UserAuthRepository : IUserAuthRepository
    {
        private readonly AppDbContext _context;

        public UserAuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserAuth?> GetByEmailAsync(string email)
        {
            return await _context.UserAuths
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(ua => ua.Email == email);
        }

        public async Task<UserAuth?> GetByUserIdAsync(int userId)
        {
            return await _context.UserAuths
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(ua => ua.UserId == userId);
        }

        public async Task<UserAuth> CreateAsync(UserAuth userAuth)
        {
            _context.UserAuths.Add(userAuth);
            await _context.SaveChangesAsync();
            return userAuth;
        }

        public async Task<UserAuth> UpdateAsync(UserAuth userAuth)
        {
            _context.UserAuths.Update(userAuth);
            await _context.SaveChangesAsync();
            return userAuth;
        }

        public async Task<bool> DeleteAsync(int userAuthId)
        {
            var userAuth = await _context.UserAuths.FindAsync(userAuthId);
            if (userAuth == null) return false;

            _context.UserAuths.Remove(userAuth);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteByUserIdAsync(int userId)
        {
            var userAuth = await _context.UserAuths.FirstOrDefaultAsync(ua => ua.UserId == userId);
            if (userAuth == null) return false;

            _context.UserAuths.Remove(userAuth);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> ExistsAsync(string email)
        {
            return await _context.UserAuths.AnyAsync(ua => ua.Email == email);
        }
    }
}