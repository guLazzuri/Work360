using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Work360.Controller;
using Work360.Domain.DTO;
using Work360.Domain.Entity;
using Work360.Domain.Enum;
using Work360.Infrastructure.Context;
using Work360.Infrastructure.Services;
using Xunit;

namespace Work360.Tests
{
    public class TasksControllerTests : IDisposable
    {
        private readonly Work360Context _context;
        private readonly Mock<IHateoasService> _mockHateoasService;
        private readonly Mock<ILogger<TasksController>> _mockLogger;
        private readonly TasksController _controller;
        private readonly Guid _testUserId;

        public TasksControllerTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<Work360Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Work360Context(options);
            _mockHateoasService = new Mock<IHateoasService>();
            _mockLogger = new Mock<ILogger<TasksController>>();

            _controller = new TasksController(_context, _mockHateoasService.Object, _mockLogger.Object);
            _testUserId = Guid.NewGuid();

            // Setup mock for HATEOAS service
            _mockHateoasService
                .Setup(x => x.GeneratePaginationLinks(It.IsAny<PagedResult<Tasks>>(), It.IsAny<string>(), It.IsAny<IUrlHelper>()))
                .Returns(new List<LinkDto>());
        }

        [Fact]
        public async Task GetTasks_ReturnsOkResult_WithPaginatedTasks()
        {
            // Arrange
            SeedTasks();

            // Act
            var result = await _controller.GetTaskss(1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Tasks>>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Tasks>>(okObjectResult.Value);

            Assert.Equal(3, pagedResult.Items.Count());
            Assert.Equal(3, pagedResult.TotalItems);
        }

        [Fact]
        public async Task GetTasks_ReturnsPaginatedResults_WhenPageSizeIsSpecified()
        {
            // Arrange
            SeedTasks();

            // Act
            var result = await _controller.GetTaskss(1, 2);

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Tasks>>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Tasks>>(okObjectResult.Value);

            Assert.Equal(2, pagedResult.Items.Count());
            Assert.Equal(3, pagedResult.TotalItems);
        }

        [Fact]
        public async Task GetTask_ReturnsOkResult_WhenTaskExists()
        {
            // Arrange
            var task = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "Test Task",
                Description = "Test Description",
                Priority = Priority.HIGH,
                EstimateMinutes = 60,
                TaskSituation = TaskSituation.OPEN
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTasks(task.TaskID);

            // Assert
            var okResult = Assert.IsType<ActionResult<Tasks>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var returnedTask = Assert.IsType<Tasks>(okObjectResult.Value);

            Assert.Equal(task.TaskID, returnedTask.TaskID);
            Assert.Equal("Test Task", returnedTask.Title);
        }

        [Fact]
        public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _controller.GetTasks(nonExistentId);

            // Assert
            var notFoundResult = Assert.IsType<ActionResult<Tasks>>(result);
            Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
        }

        [Fact]
        public async Task PostTask_ReturnsCreatedAtAction_WithNewTask()
        {
            // Arrange
            var newTask = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "New Task",
                Description = "New Description",
                Priority = Priority.MEDIUM,
                EstimateMinutes = 120
            };

            // Act
            var result = await _controller.PostTasks(newTask);

            // Assert
            var createdResult = Assert.IsType<ActionResult<Tasks>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(createdResult.Result);
            var returnedTask = Assert.IsType<Tasks>(createdAtActionResult.Value);

            Assert.Equal(newTask.TaskID, returnedTask.TaskID);
            Assert.Equal("New Task", returnedTask.Title);

            // Verify task was added to database
            var taskInDb = await _context.Tasks.FindAsync(newTask.TaskID);
            Assert.NotNull(taskInDb);
        }

        [Fact]
        public async Task PutTask_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var task = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "Original Title",
                Description = "Original Description",
                Priority = Priority.LOW,
                EstimateMinutes = 30
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Detach to avoid tracking issues
            _context.Entry(task).State = EntityState.Detached;

            task.Title = "Updated Title";

            // Act
            var result = await _controller.PutTasks(task.TaskID, task);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify update in database
            var updatedTask = await _context.Tasks.FindAsync(task.TaskID);
            Assert.Equal("Updated Title", updatedTask!.Title);
        }

        [Fact]
        public async Task PutTask_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var task = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "Test Task",
                Description = "Test Description",
                Priority = Priority.HIGH,
                EstimateMinutes = 60
            };

            // Act
            var result = await _controller.PutTasks(Guid.NewGuid(), task);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task EndTask_ReturnsNoContent_AndMarksTaskAsCompleted()
        {
            // Arrange
            var task = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "Task to Complete",
                Description = "Description",
                Priority = Priority.HIGH,
                EstimateMinutes = 60,
                TaskSituation = TaskSituation.IN_PROGRESS,
                CreatedTask = DateTime.Now.AddHours(-2)
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.EndTasks(task.TaskID);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify task is completed
            var completedTask = await _context.Tasks.FindAsync(task.TaskID);
            Assert.Equal(TaskSituation.COMPLETED, completedTask!.TaskSituation);
            Assert.NotNull(completedTask.FinalDateTask);
            Assert.True(completedTask.SpentMinutes > 0);
        }

        [Fact]
        public async Task EndTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _controller.EndTasks(nonExistentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTask_ReturnsNoContent_WhenTaskIsDeleted()
        {
            // Arrange
            var task = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "Task to Delete",
                Description = "Description",
                Priority = Priority.LOW,
                EstimateMinutes = 30
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteTasks(task.TaskID);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify deletion in database
            var deletedTask = await _context.Tasks.FindAsync(task.TaskID);
            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task TaskCreation_SetsDefaultValues_Correctly()
        {
            // Arrange
            var task = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "Task with Defaults",
                Description = "Description",
                Priority = Priority.MEDIUM,
                EstimateMinutes = 45
            };

            // Act
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Assert
            var savedTask = await _context.Tasks.FindAsync(task.TaskID);
            Assert.Equal(TaskSituation.OPEN, savedTask!.TaskSituation);
            Assert.Equal(0, savedTask.SpentMinutes);
            Assert.Null(savedTask.FinalDateTask);
            Assert.True(savedTask.CreatedTask <= DateTime.UtcNow);
        }

        private void SeedTasks()
        {
            var tasks = new List<Tasks>
            {
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Task 1",
                    Description = "Description 1",
                    Priority = Priority.HIGH,
                    EstimateMinutes = 60
                },
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Task 2",
                    Description = "Description 2",
                    Priority = Priority.MEDIUM,
                    EstimateMinutes = 90
                },
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Task 3",
                    Description = "Description 3",
                    Priority = Priority.LOW,
                    EstimateMinutes = 30
                }
            };

            _context.Tasks.AddRange(tasks);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}

