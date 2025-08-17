namespace AuthApiDemo.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public DateTime Expiration { get; set; }
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public bool IsAuthenticated { get; set; }
        public string Message { get; set; } = "";
    }
}
