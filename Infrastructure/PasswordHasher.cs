using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using AuthApiDemo.Domain.Interfaces;

namespace AuthApiDemo.Infrastructure
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly IConfiguration _configuration;

        public PasswordHasher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (byte[] hash, byte[] salt) HashPassword(string password)
        {
            // Generate a random salt
            var salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Get password key from configuration
            var passwordKey = _configuration["AppSettings:passwordKey"] ?? "default_password_key_123!";

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

        public bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            try
            {
                // Get password key from configuration
                var passwordKey = _configuration["AppSettings:passwordKey"] ?? "default_password_key_123!";

                // Recreate hash using the same parameters
                var verifyHash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: 10000,
                    numBytesRequested: 32
                );

                // Compare hashes
                return hash.SequenceEqual(verifyHash);
            }
            catch
            {
                return false;
            }
        }
    }
}