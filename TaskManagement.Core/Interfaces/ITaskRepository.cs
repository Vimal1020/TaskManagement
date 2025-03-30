using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;

namespace TaskManagement.Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskEntity> GetTaskById(int id);
        Task AddAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task DeleteAsync(TaskEntity task);
        Task<(IEnumerable<TaskEntity> Items, int Total)> GetUserTasksAsync(int userId, int page, int pageSize, TaskStatusEnum? status);
        Task<List<TaskEntity>> GetTasksByUser(int userId, int pageNumber = 1, int pageSize = 10, string searchTerm = null);
    }
}
