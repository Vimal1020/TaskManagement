using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.Requests
{
    public class TaskLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; init; }

        [Required]
        public string Password { get; init; }
    }
}
