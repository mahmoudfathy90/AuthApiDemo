using System.ComponentModel.DataAnnotations;

namespace AuthApiDemo.Domain.Entities
{
    public class UserAuth
    {
        public int UserAuthId { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        
        [Required]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsLocked { get; set; } = false;
        
        public int FailedLoginAttempts { get; set; } = 0;
        
        public DateTime? LockedUntil { get; set; }
        
        // Navigation property to User
        public User User { get; set; } = null!;

        // Business methods
        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            ResetFailedAttempts();
        }

        public void IncrementFailedAttempts()
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= 5)
            {
                LockAccount(TimeSpan.FromMinutes(15));
            }
        }

        public void ResetFailedAttempts()
        {
            FailedLoginAttempts = 0;
            IsLocked = false;
            LockedUntil = null;
        }

        public void LockAccount(TimeSpan lockDuration)
        {
            IsLocked = true;
            LockedUntil = DateTime.UtcNow.Add(lockDuration);
        }

        public bool IsAccountLocked()
        {
            if (!IsLocked) return false;
            if (LockedUntil.HasValue && DateTime.UtcNow > LockedUntil.Value)
            {
                ResetFailedAttempts();
                return false;
            }
            return true;
        }
    }
}