using AuthApiDemo.Domain.Entities;
using AuthApiDemo.Domain.Interfaces;

namespace AuthApiDemo.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserAuthRepository _userAuthRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepository,
            IUserAuthRepository userAuthRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _userAuthRepository = userAuthRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string password, string firstName, string lastName, string gender)
        {
            try
            {
                // Check if user already exists
                if (await _userAuthRepository.ExistsAsync(email))
                {
                    return (false, "Email already registered", null);
                }

                // Create user
                var user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Gender = gender,
                    Active = true
                };

                var createdUser = await _userRepository.CreateAsync(user);

                // Create password hash
                var (hash, salt) = _passwordHasher.HashPassword(password);

                // Create auth record
                var userAuth = new UserAuth
                {
                    UserId = createdUser.UserId,
                    Email = email,
                    PasswordHash = hash,
                    PasswordSalt = salt
                };

                await _userAuthRepository.CreateAsync(userAuth);

                return (true, "User registered successfully", createdUser);
            }
            catch (Exception ex)
            {
                return (false, $"Registration failed: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, User? User, string? Token, string? RefreshToken, DateTime? Expiration)> LoginAsync(string email, string password)
        {
            try
            {
                var userAuth = await _userAuthRepository.GetByEmailAsync(email);
                if (userAuth == null)
                {
                    return (false, "Invalid email or password", null, null, null, null);
                }

                // Check if account is locked
                if (userAuth.IsAccountLocked())
                {
                    return (false, "Account is temporarily locked due to multiple failed login attempts", null, null, null, null);
                }

                // Verify password
                if (!_passwordHasher.VerifyPassword(password, userAuth.PasswordHash, userAuth.PasswordSalt))
                {
                    userAuth.IncrementFailedAttempts();
                    await _userAuthRepository.UpdateAsync(userAuth);
                    return (false, "Invalid email or password", null, null, null, null);
                }

                // Check if user is active
                if (!userAuth.User.Active)
                {
                    return (false, "Account is deactivated", null, null, null, null);
                }

                // Update login info
                userAuth.UpdateLastLogin();
                await _userAuthRepository.UpdateAsync(userAuth);

                // Generate tokens
                var token = _jwtTokenService.GenerateToken(userAuth.User);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();
                var expiration = _jwtTokenService.GetTokenExpiration();

                return (true, "Login successful", userAuth.User, token, refreshToken, expiration);
            }
            catch (Exception ex)
            {
                return (false, $"Login failed: {ex.Message}", null, null, null, null);
            }
        }

        public async Task<(bool Success, string Message, string? Token, string? RefreshToken, DateTime? Expiration)> RefreshTokenAsync(int userId, string refreshToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || !user.Active)
                {
                    return (false, "User not found or inactive", null, null, null);
                }

                // In a real implementation, you would validate the refresh token against stored tokens
                // For now, we'll just generate new tokens
                var newToken = _jwtTokenService.GenerateToken(user);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
                var expiration = _jwtTokenService.GetTokenExpiration();

                return (true, "Tokens refreshed successfully", newToken, newRefreshToken, expiration);
            }
            catch (Exception ex)
            {
                return (false, $"Token refresh failed: {ex.Message}", null, null, null);
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var userAuth = await _userAuthRepository.GetByUserIdAsync(userId);
                if (userAuth == null)
                {
                    return false;
                }

                // Verify current password
                if (!_passwordHasher.VerifyPassword(currentPassword, userAuth.PasswordHash, userAuth.PasswordSalt))
                {
                    return false;
                }

                // Hash new password
                var (hash, salt) = _passwordHasher.HashPassword(newPassword);
                userAuth.PasswordHash = hash;
                userAuth.PasswordSalt = salt;

                await _userAuthRepository.UpdateAsync(userAuth);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            try
            {
                var userAuth = await _userAuthRepository.GetByEmailAsync(email);
                if (userAuth == null)
                {
                    return false;
                }

                // Hash new password
                var (hash, salt) = _passwordHasher.HashPassword(newPassword);
                userAuth.PasswordHash = hash;
                userAuth.PasswordSalt = salt;
                userAuth.ResetFailedAttempts(); // Reset any lockout

                await _userAuthRepository.UpdateAsync(userAuth);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}