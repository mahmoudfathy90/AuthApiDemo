using System.ComponentModel.DataAnnotations;

namespace AuthApiDemo.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Gender { get; set; } = string.Empty;
        
        public bool Active { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

        // Business methods
        public void Activate() => Active = true;
        public void Deactivate() => Active = false;
        public void UpdateInfo(string firstName, string lastName, string gender)
        {
            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public string GetFullName() => $"{FirstName} {LastName}";
    }
}