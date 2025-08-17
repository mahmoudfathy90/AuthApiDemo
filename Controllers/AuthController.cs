using AuthApiDemo.Data;
using AuthApiDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Authorization;

namespace AuthApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if user already exists by email
                if (await _context.UserAuths.AnyAsync(ua => ua.Email == request.Email))
                    return BadRequest("Email already registered");

                // Create secure password hash
                var (passwordHash, passwordSalt) = CreateSecurePasswordHash(request.Password);

                // Create User record
                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Gender = request.Gender,
                    Active = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create UserAuth record
                var userAuth = new UserAuth
                {
                    UserId = user.UserId,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserAuths.Add(userAuth);
                var result = await _context.SaveChangesAsync();

                if (result <= 0)
                    throw new InvalidOperationException("Failed to save user authentication data. No rows were affected.");

                return Ok(new { 
                    message = "User registered successfully", 
                    email = user.Email,
                    userId = user.UserId 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate request
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Find user authentication by email
                var userAuth = await _context.UserAuths
                    .Include(ua => ua.User)
                    .FirstOrDefaultAsync(ua => ua.Email == request.Email);

                if (userAuth == null)
                    return Unauthorized(new LoginResponse 
                    { 
                        IsAuthenticated = false, 
                        Message = "Invalid email or password" 
                    });

                // Verify password
                if (!VerifySecurePassword(request.Password, userAuth.PasswordHash, userAuth.PasswordSalt))
                    return Unauthorized(new LoginResponse 
                    { 
                        IsAuthenticated = false, 
                        Message = "Invalid email or password" 
                    });

                // Check if user is active
                if (!userAuth.User.Active)
                    return Unauthorized(new LoginResponse 
                    { 
                        IsAuthenticated = false, 
                        Message = "Account is deactivated" 
                    });

                // Generate tokens
                var tokens = GenerateTokens(userAuth.User);

                // Update last login time
                userAuth.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var response = new LoginResponse
                {
                    Token = tokens.Token,
                    RefreshToken = tokens.RefreshToken,
                    Expiration = tokens.Expiration,
                    Email = userAuth.User.Email,
                    FirstName = userAuth.User.FirstName,
                    LastName = userAuth.User.LastName,
                    IsAuthenticated = true,
                    Message = "Login successful"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponse 
                { 
                    IsAuthenticated = false, 
                    Message = "Internal server error" 
                });
            }
        }

        [Authorize]
        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromQuery] string refreshToken)
        {
            try
            {
                // Get the current user from the controller base
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                // Find the user in the database
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.Active);

                if (user == null)
                {
                    return Unauthorized(new { message = "User not found or inactive" });
                }

                // Validate refresh token (in a real application, you would store and validate refresh tokens)
                // For now, we'll just generate new tokens
                var tokens = GenerateTokens(user);

                var response = new AuthResponse
                {
                    Token = tokens.Token,
                    RefreshToken = tokens.RefreshToken,
                    Expiration = tokens.Expiration
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        private (byte[] hash, byte[] salt) CreateSecurePasswordHash(string password)
        {
            // Generate a random salt using RandomNumberGenerator
            var salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Get password key from appsettings
            var passwordKey = _config["AppSettings:passwordKey"] ?? "default_password_key_123!";

            // Create password hash using KeyDerivation with PBKDF2
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 32
            );

            return (hash, salt);
        }

        private bool VerifySecurePassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            try
            {
                // Get password key from appsettings
                var passwordKey = _config["AppSettings:passwordKey"] ?? "default_password_key_123!";

                // Recreate hash using the same parameters
                var hash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: storedSalt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 10000,
                    numBytesRequested: 32
                );

                // Compare hashes
                return hash.SequenceEqual(storedHash);
            }
            catch
            {
                return false;
            }
        }

        private AuthResponse GenerateTokens(User user)
        {
            var jwtKey = _config["AppSettings:TokenKey"] ?? "super_secret_key_123!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(1);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            var token = new JwtSecurityToken(
                audience: null,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            return new AuthResponse
            {
                Token = jwt,
                RefreshToken = refreshToken,
                Expiration = expires
            };
        }
    }
}
