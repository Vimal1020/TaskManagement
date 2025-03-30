using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Entities
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatusEnum Status { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public int CreatedBy { get; set; }

        public int AssignedUserId { get; set; }
        public ApplicationUser AssignedUser { get; set; }
    }
}
