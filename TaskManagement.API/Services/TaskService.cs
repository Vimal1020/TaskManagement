using TaskManagement.Core.Dto;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.API.Services
{
    public class TaskService(ITaskCacheService taskCacheService, ITaskRepository _taskRepository) : ITaskService
    {
        public async Task<TaskEntity> CreateTaskAsync(TaskEntity task, int currentUserId)
        {
            task.CreatedAt = DateTimeOffset.UtcNow;
            task.CreatedBy = currentUserId;
            await _taskRepository.AddAsync(task);

            await taskCacheService.InvalidateUserTasksCacheAsync(task.AssignedUserId);

            return task;
        }

        public async Task<TaskEntity> GetTaskByIdAsync(int id)
        {
            var task = await _taskRepository.GetTaskById(id);
            return task ?? null;

        }

        public async Task<List<TaskEntity>> GetTasksByUser(int userId, int pageNumber = 1, int pageSize = 10, string searchTerm = null)
        {
            return await _taskRepository.GetTasksByUser(userId, pageNumber, pageSize, searchTerm);
        }

        public async Task<bool> UpdateTaskAsync(int id, TaskEntity updatedTask, int currentUserId)
        {
            var task = await _taskRepository.GetTaskById(id);
            if (task == null || task.CreatedBy != currentUserId)
                return false;

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.Status = updatedTask.Status;
            task.AssignedUserId = updatedTask.AssignedUserId;
            task.DueDate = updatedTask.DueDate;

            await _taskRepository.UpdateAsync(task);

            await taskCacheService.InvalidateUserTasksCacheAsync(task.AssignedUserId);

            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id, int currentUserId)
        {
            var task = await _taskRepository.GetTaskById(id);
            if (task == null || task.CreatedBy != currentUserId)
                return false;

            await _taskRepository.DeleteAsync(task);

            await taskCacheService.InvalidateUserTasksCacheAsync(task.AssignedUserId);

            return true;
        }

        public async Task<PaginatedResult<TaskEntity>> GetUserTasksAsync(int userId, int page, int pageSize, TaskStatusEnum? status)
        {
            var cachedResult = await taskCacheService.GetUserTasksAsync(userId, page, pageSize, status);
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var (items, total) = await _taskRepository.GetUserTasksAsync(userId, page, pageSize, status);

            var result = new PaginatedResult<TaskEntity>
            {
                Items = items.ToList(),
                Total = total,
                Page = page,
                PageSize = pageSize
            };

            await taskCacheService.SetUserTasksAsync(userId, page, pageSize, status, result);

            return result;
        }
    }
}