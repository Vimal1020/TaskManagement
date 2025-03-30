namespace TaskManagement.Core.Models.Responses
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
    }
}
