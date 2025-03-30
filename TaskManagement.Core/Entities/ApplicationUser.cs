using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Core.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        [PersonalData]
        [StringLength(100)]
        public string FirstName { get; set; }

        [PersonalData]
        [StringLength(100)]
        public string LastName { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
    }

    public class ApplicationRole : IdentityRole<int>
    {
        public string Description { get; set; } = "";
    }
}