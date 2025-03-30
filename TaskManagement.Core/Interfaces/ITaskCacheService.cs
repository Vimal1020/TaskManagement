using TaskManagement.Core.Dto;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskCacheService
    {
        Task<PaginatedResult<TaskEntity>> GetUserTasksAsync(int userId, int page, int pageSize, TaskStatusEnum? status);
        Task SetUserTasksAsync(int userId, int page, int pageSize, TaskStatusEnum? status, PaginatedResult<TaskEntity> tasks);
        Task InvalidateUserTasksCacheAsync(int userId);
    }
}
