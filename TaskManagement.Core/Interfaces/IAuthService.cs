using TaskManagement.Core.Models.Responses;

namespace TaskManagement.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(string email, string password, string firstName, string lastName, string role = "User");
        Task<AuthResponse> LoginAsync(string email, string password);
        Task InitializeRolesAsync();
    }
}
