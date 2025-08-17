using System.ComponentModel.DataAnnotations;

namespace AuthApiDemo.Models
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}