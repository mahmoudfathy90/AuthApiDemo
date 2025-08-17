namespace AuthApiDemo.Application.DTOs
{
    public class LoginResultDto
    {
        public bool IsAuthenticated { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? Expiration { get; set; }
        public UserDto? User { get; set; }
    }
}