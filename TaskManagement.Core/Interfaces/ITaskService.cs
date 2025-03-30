using TaskManagement.Core.Dto;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskService
    {
        Task<TaskEntity> CreateTaskAsync(TaskEntity task, int currentUserId);
        Task<TaskEntity> GetTaskByIdAsync(int id);
        Task<bool> UpdateTaskAsync(int id, TaskEntity updatedTask, int currentUserId);
        Task<bool> DeleteTaskAsync(int id, int currentUserId);
        Task<PaginatedResult<TaskEntity>> GetUserTasksAsync(int userId, int page, int pageSize, TaskStatusEnum? status);

        Task<List<TaskEntity>> GetTasksByUser(int userId, int pageNumber = 1, int pageSize = 10, string searchTerm = null);
    }
}
