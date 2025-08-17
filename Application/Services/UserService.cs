using AuthApiDemo.Domain.Entities;
using AuthApiDemo.Domain.Interfaces;

namespace AuthApiDemo.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserAuthRepository _userAuthRepository;

        public UserService(IUserRepository userRepository, IUserAuthRepository userAuthRepository)
        {
            _userRepository = userRepository;
            _userAuthRepository = userAuthRepository;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _userRepository.GetActiveUsersAsync();
        }

        public async Task<User> CreateUserAsync(string firstName, string lastName, string email, string gender, bool active = true)
        {
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Gender = gender,
                Active = active
            };

            return await _userRepository.CreateAsync(user);
        }

        public async Task<User> UpdateUserAsync(int userId, string firstName, string lastName, string gender)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found", nameof(userId));
            }

            user.UpdateInfo(firstName, lastName, gender);
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            // First delete the auth record
            await _userAuthRepository.DeleteByUserIdAsync(userId);
            
            // Then delete the user
            return await _userRepository.DeleteAsync(userId);
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.Activate();
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.Deactivate();
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userRepository.ExistsAsync(email);
        }
    }
}