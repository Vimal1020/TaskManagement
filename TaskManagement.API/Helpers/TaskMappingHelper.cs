using TaskManagement.API.Generated;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;

namespace TaskManagement.API.Helpers
{
    public static class TaskMappingHelper
    {
        public static TaskEntity MapToDomain(TaskCreateUpdateDto dto)
        {
            return new TaskEntity
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = (TaskStatusEnum)Enum.Parse(typeof(TaskStatusEnum), dto.Status.ToString()),
                DueDate = dto.DueDate
            };
        }

        public static TaskResponseDto MapToDto(TaskEntity entity)
        {
            return new TaskResponseDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Status = (Status)Enum.Parse(typeof(Status), entity.Status.ToString()),
                AssignedUserId = entity.AssignedUserId,
                DueDate = entity.DueDate,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
