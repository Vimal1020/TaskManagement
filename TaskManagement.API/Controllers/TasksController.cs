using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Extensions;
using TaskManagement.API.Generated;
using TaskManagement.API.Helpers;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase, ITaskController
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Create a new task.
        /// POST: api/tasks
        /// Accessible by: Admin, Manager, User
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<TaskResponseDto> TasksPOSTAsync([FromBody] TaskCreateUpdateDto body)
        {
            var domainTask = TaskMappingHelper.MapToDomain(body);
            domainTask.AssignedUserId = User.GetUserId();
            var createdTask = await _taskService.CreateTaskAsync(domainTask, domainTask.AssignedUserId);
            return TaskMappingHelper.MapToDto(createdTask);
        }

        /// <summary>
        /// Retrieve a task by its ID.
        /// GET: api/tasks/{id}
        /// Accessible by: Admin, Manager, User
        /// </summary>
        [Authorize(Roles = "Admin,Manager,User")]
        [HttpGet("{id}")]
        public async Task<TaskResponseDto> TasksGETAsync(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id)
                ?? throw new KeyNotFoundException("Task not found");
            return TaskMappingHelper.MapToDto(task);
        }

        /// <summary>
        /// Update an existing task.
        /// PUT: api/tasks/{id}
        /// Accessible by: Admin, Manager
        /// </summary>
        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}")]
        public async Task TasksPUTAsync(int id, [FromBody] TaskCreateUpdateDto body)
        {
            var domainTask = TaskMappingHelper.MapToDomain(body);
            domainTask.AssignedUserId = User.GetUserId();

            var updated = await _taskService.UpdateTaskAsync(id, domainTask, domainTask.AssignedUserId);
            if (!updated) throw new KeyNotFoundException("Task not found");
        }

        /// <summary>
        /// Delete an existing task.
        /// DELETE: api/tasks/{id}
        /// Accessible by: Admin only
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task TasksDELETEAsync(int id)
        {
            var deleted = await _taskService.DeleteTaskAsync(id, User.GetUserId());
            if (!deleted) throw new KeyNotFoundException("Task not found");
        }

        /// <summary>
        /// Retrieve tasks for a specific user with pagination and optional status filter.
        /// GET: api/tasks/user/{userId}?page=1&pageSize=10&status=Active
        /// Accessible by: Admin, Manager, User
        /// </summary>
        [Authorize(Roles = "Admin,Manager,User")]
        [HttpGet("user/{userId}")]
        public async Task<PaginatedTasksResponse> UserAsync(
            int userId,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] Status? status)
        {
            TaskStatusEnum? domainStatus = status.HasValue
                ? (TaskStatusEnum)Enum.Parse(typeof(TaskStatusEnum), status.Value.ToString())
                : null;

            var result = await _taskService.GetUserTasksAsync(userId, page, pageSize, domainStatus);

            return new PaginatedTasksResponse
            {
                Items = result.Items.Select(TaskMappingHelper.MapToDto).ToList(),
                Total = result.Total,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }

        /// <summary>
        /// Retrieve tasks for a specific user with pagination and an optional search term.
        /// GET: api/tasks/user/search/{userId}?pageNumber=1&pageSize=10&searchTerm=keyword
        /// Accessible by: Admin, Manager, User
        /// </summary>
        [Authorize(Roles = "Admin,Manager,User")]
        [HttpGet("user/search/{userId}")]
        public async Task<List<TaskResponseDto>> GetTasksByUser(
            int userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null)
        {
            var tasks = await _taskService.GetTasksByUser(userId, pageNumber, pageSize, searchTerm);
            return tasks.Select(TaskMappingHelper.MapToDto).ToList();
        }
    }
}
