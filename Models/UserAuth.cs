namespace AuthApiDemo.Models
{
    public class UserAuth
    {
        public int UserAuthId { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public byte[] PasswordHash { get; set; } = new byte[0];
        public byte[] PasswordSalt { get; set; } = new byte[0];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation property to User
        public User User { get; set; } = null!;
    }
}
