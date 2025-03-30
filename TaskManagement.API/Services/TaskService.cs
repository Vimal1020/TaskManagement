using TaskManagement.Core.Dto;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.API.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repository;
        private readonly ITaskCacheService _cacheService;

        public TaskService(ITaskRepository repository, ITaskCacheService cacheService)
        {
            _repository = repository;
            _cacheService = cacheService;
        }

        public async Task<TaskEntity> CreateTaskAsync(TaskEntity task, int currentUserId)
        {
            task.CreatedAt = DateTimeOffset.UtcNow;
            task.CreatedBy = currentUserId;
            await _repository.AddAsync(task);

            await _cacheService.InvalidateUserTasksCacheAsync(task.AssignedUserId);

            return task;
        }

        public async Task<TaskEntity> GetTaskByIdAsync(int id)
        {
            var task = await _repository.GetTaskById(id);
            return task ?? null;

        }

        public async Task<List<TaskEntity>> GetTasksByUser(int userId, int pageNumber = 1, int pageSize = 10, string searchTerm = null)
        {
            return await _repository.GetTasksByUser(userId, pageNumber, pageSize, searchTerm);
        }

        public async Task<bool> UpdateTaskAsync(int id, TaskEntity updatedTask, int currentUserId)
        {
            var task = await _repository.GetTaskById(id);
            if (task == null || task.CreatedBy != currentUserId)
                return false;

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.Status = updatedTask.Status;
            task.AssignedUserId = updatedTask.AssignedUserId;
            task.DueDate = updatedTask.DueDate;

            await _repository.UpdateAsync(task);

            await _cacheService.InvalidateUserTasksCacheAsync(task.AssignedUserId);

            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id, int currentUserId)
        {
            var task = await _repository.GetTaskById(id);
            if (task == null || task.CreatedBy != currentUserId)
                return false;

            await _repository.DeleteAsync(task);

            await _cacheService.InvalidateUserTasksCacheAsync(task.AssignedUserId);

            return true;
        }

        public async Task<PaginatedResult<TaskEntity>> GetUserTasksAsync(int userId, int page, int pageSize, TaskStatusEnum? status)
        {
            var cachedResult = await _cacheService.GetUserTasksAsync(userId, page, pageSize, status);
            if (cachedResult != null)
            {
                return cachedResult;
            }

            var (items, total) = await _repository.GetUserTasksAsync(userId, page, pageSize, status);

            var result = new PaginatedResult<TaskEntity>
            {
                Items = items.ToList(),
                Total = total,
                Page = page,
                PageSize = pageSize
            };

            await _cacheService.SetUserTasksAsync(userId, page, pageSize, status, result);

            return result;
        }
    }
}