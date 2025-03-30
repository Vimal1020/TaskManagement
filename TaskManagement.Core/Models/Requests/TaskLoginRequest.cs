using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Core.Models.Requests
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
