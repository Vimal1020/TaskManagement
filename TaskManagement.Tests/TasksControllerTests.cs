using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManagement.API.Controllers;
using TaskManagement.API.Generated;
using TaskManagement.API.Helpers;
using TaskManagement.Core.Dto;
using TaskManagement.Core.Entities;
using TaskManagement.Core.Enums;
using TaskManagement.Core.Interfaces;

namespace TaskManagement.Tests
{
    [TestFixture]
    [Order(2)]
    public class TasksControllerTests
    {
        private Mock<ITaskService> _taskServiceMock;
        private TasksController _tasksController;
        private int _userId = 1;

        [SetUp]
        public void Setup()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _tasksController = new TasksController(_taskServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                new Claim(ClaimTypes.Role, "User")
            }, "TestAuthentication"));

            _tasksController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        [Order(1)]
        public async Task TasksPOSTAsync_ReturnsCreatedTaskDto()
        {
            var createDto = new TaskCreateUpdateDto
            {
                Title = "Test Task",
                Description = "Test Description",
                DueDate = DateTimeOffset.Now.AddDays(1),
                Status = Status.InProgress
            };

            var domainTask = TaskMappingHelper.MapToDomain(createDto);
            domainTask.Id = 1;
            domainTask.AssignedUserId = _userId;

            _taskServiceMock
                .Setup(x => x.CreateTaskAsync(It.IsAny<TaskEntity>(), _userId))
                .ReturnsAsync(domainTask);

            // Act
            var result = await _tasksController.TasksPOSTAsync(createDto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo(createDto.Title));
        }

        [Test]
        [Order(2)]
        public async Task TasksGETAsync_ReturnsTaskDto_WhenTaskExists()
        {
            // Arrange
            var taskId = 1;
            var domainTask = new TaskEntity
            {
                Id = taskId,
                Title = "Test Task",
                Status = TaskStatusEnum.InProgress,
                AssignedUserId = _userId
            };

            _taskServiceMock
                .Setup(x => x.GetTaskByIdAsync(taskId))
                .ReturnsAsync(domainTask);

            // Act
            var result = await _tasksController.TasksGETAsync(taskId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(taskId));
        }

        [Test]
        [Order(3)]
        public async Task TasksPUTAsync_UpdatesTask_WhenTaskExists()
        {
            // Arrange
            var taskId = 1;
            var updateDto = new TaskCreateUpdateDto
            {
                Title = "Updated Task",
                Status = Status.Completed
            };

            _taskServiceMock
                .Setup(x => x.UpdateTaskAsync(taskId, It.IsAny<TaskEntity>(), _userId))
                .ReturnsAsync(true);

            // Act & Assert
            Assert.DoesNotThrowAsync(async () =>
                await _tasksController.TasksPUTAsync(taskId, updateDto));
        }

        [Test]
        [Order(4)]
        public async Task TasksDELETEAsync_DeletesTask_WhenTaskExists()
        {
            // Arrange
            var taskId = 1;
            _taskServiceMock
                .Setup(x => x.DeleteTaskAsync(taskId, _userId))
                .ReturnsAsync(true);

            // Act & Assert
            Assert.DoesNotThrowAsync(async () =>
                await _tasksController.TasksDELETEAsync(taskId));
        }

        [Test]
        [Order(5)]
        public async Task UserAsync_ReturnsPaginatedTasksResponse()
        {
            // Arrange
            var serviceResult = new PaginatedResult<TaskEntity>
            {
                Items = new List<TaskEntity>
                {
                    new TaskEntity
                    {
                        Id = 1,
                        Title = "Test Task",
                        Status = TaskStatusEnum.InProgress,
                        AssignedUserId = _userId
                    }
                },
                Total = 1,
                Page = 1,
                PageSize = 10
            };

            _taskServiceMock
                .Setup(x => x.GetUserTasksAsync(_userId, 1, 10, It.IsAny<TaskStatusEnum?>()))
                .ReturnsAsync(serviceResult);

            // Act
            var result = await _tasksController.UserAsync(
                _userId, 1, 10, Status.InProgress);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Items.First().Title, Is.EqualTo("Test Task"));
        }

        [Test]
        [Order(6)]
        public async Task GetTasksByUser_ReturnsListOfTaskResponseDto()
        {
            // Arrange
            var userId = _userId;
            int pageNumber = 1;
            int pageSize = 10;
            string searchTerm = "Test";

            // Create a list of sample tasks.
            var tasks = new List<TaskEntity>
              {
                new TaskEntity { Id = 1, Title = "Test Task 1", Status = TaskStatusEnum.InProgress, AssignedUserId = userId },
                new TaskEntity { Id = 2, Title = "Test Task 2", Status = TaskStatusEnum.Pending, AssignedUserId = userId }
               };

            _taskServiceMock
                .Setup(x => x.GetTasksByUser(userId, pageNumber, pageSize, searchTerm))
                .ReturnsAsync(tasks);

            var result = await _tasksController.GetTasksByUser(userId, pageNumber, pageSize, searchTerm);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(tasks.Count));

            Assert.That(result.First().Title, Is.EqualTo(tasks.First().Title));
        }
    }
}
