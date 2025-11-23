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
    public class ReportControllerTests : IDisposable
    {
        private readonly Work360Context _context;
        private readonly Mock<IHateoasService> _mockHateoasService;
        private readonly Mock<ILogger<ReportController>> _mockLogger;
        private readonly ReportController _controller;
        private readonly Guid _testUserId;

        public ReportControllerTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<Work360Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Work360Context(options);
            _mockHateoasService = new Mock<IHateoasService>();
            _mockLogger = new Mock<ILogger<ReportController>>();

            _controller = new ReportController(_context, _mockHateoasService.Object, _mockLogger.Object);
            _testUserId = Guid.NewGuid();

            // Setup mock for HATEOAS service
            _mockHateoasService
                .Setup(x => x.GeneratePaginationLinks(It.IsAny<PagedResult<Report>>(), It.IsAny<string>(), It.IsAny<IUrlHelper>()))
                .Returns(new List<LinkDto>());
        }

        [Fact]
        public async Task GetReport_ReturnsOkResult_WithReportData()
        {
            // Arrange
            SeedTestData();
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            // Act
            var result = await _controller.GetReport(_testUserId, startDate, endDate, 1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<Report>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Report>>(okObjectResult.Value);

            Assert.NotNull(pagedResult.Item);
            Assert.Equal(_testUserId, pagedResult.Item.UserID);
        }

        [Fact]
        public async Task GetReport_CalculatesCompletedTasksCorrectly()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            // Add tasks
            var tasks = new List<Tasks>
            {
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Completed Task 1",
                    Description = "Description",
                    Priority = Priority.HIGH,
                    EstimateMinutes = 60,
                    TaskSituation = TaskSituation.COMPLETED,
                    CreatedTask = new DateTime(2024, 1, 15),
                    FinalDateTask = new DateTime(2024, 1, 16)
                },
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Completed Task 2",
                    Description = "Description",
                    Priority = Priority.MEDIUM,
                    EstimateMinutes = 90,
                    TaskSituation = TaskSituation.COMPLETED,
                    CreatedTask = new DateTime(2024, 1, 20),
                    FinalDateTask = new DateTime(2024, 1, 21)
                }
            };
            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReport(_testUserId, startDate, endDate, 1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<Report>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Report>>(okObjectResult.Value);

            Assert.Equal(2, pagedResult.Item.CompletedTasks);
        }

        [Fact]
        public async Task GetReport_CalculatesInProgressTasksCorrectly()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            // Add tasks
            var tasks = new List<Tasks>
            {
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "In Progress Task 1",
                    Description = "Description",
                    Priority = Priority.HIGH,
                    EstimateMinutes = 60,
                    TaskSituation = TaskSituation.IN_PROGRESS,
                    CreatedTask = new DateTime(2024, 1, 15),
                    FinalDateTask = new DateTime(2024, 1, 20)
                },
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Completed Task",
                    Description = "Description",
                    Priority = Priority.MEDIUM,
                    EstimateMinutes = 90,
                    TaskSituation = TaskSituation.COMPLETED,
                    CreatedTask = new DateTime(2024, 1, 20),
                    FinalDateTask = new DateTime(2024, 1, 21)
                }
            };
            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReport(_testUserId, startDate, endDate, 1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<Report>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Report>>(okObjectResult.Value);

            Assert.Equal(1, pagedResult.Item.InProgressTasks);
            Assert.Equal(1, pagedResult.Item.CompletedTasks);
        }

        [Fact]
        public async Task GetReport_CalculatesCompletionPercentageCorrectly()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            // Add tasks: 3 completed out of 4 = 75%
            var tasks = new List<Tasks>
            {
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Task 1",
                    Description = "Description",
                    Priority = Priority.HIGH,
                    EstimateMinutes = 60,
                    TaskSituation = TaskSituation.COMPLETED,
                    CreatedTask = new DateTime(2024, 1, 15),
                    FinalDateTask = new DateTime(2024, 1, 20)
                },
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Task 2",
                    Description = "Description",
                    Priority = Priority.HIGH,
                    EstimateMinutes = 60,
                    TaskSituation = TaskSituation.COMPLETED,
                    CreatedTask = new DateTime(2024, 1, 15),
                    FinalDateTask = new DateTime(2024, 1, 20)
                },
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Task 3",
                    Description = "Description",
                    Priority = Priority.HIGH,
                    EstimateMinutes = 60,
                    TaskSituation = TaskSituation.COMPLETED,
                    CreatedTask = new DateTime(2024, 1, 15),
                    FinalDateTask = new DateTime(2024, 1, 20)
                },
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Task 4",
                    Description = "Description",
                    Priority = Priority.HIGH,
                    EstimateMinutes = 60,
                    TaskSituation = TaskSituation.OPEN,
                    CreatedTask = new DateTime(2024, 1, 15),
                    FinalDateTask = new DateTime(2024, 1, 20)
                }
            };
            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReport(_testUserId, startDate, endDate, 1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<Report>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Report>>(okObjectResult.Value);

            Assert.Equal(75.0, pagedResult.Item.CompletionPercentage);
        }

        [Fact]
        public async Task GetReport_ReturnsZeroCompletionPercentage_WhenNoTasks()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            // Act
            var result = await _controller.GetReport(_testUserId, startDate, endDate, 1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<Report>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Report>>(okObjectResult.Value);

            Assert.Equal(0, pagedResult.Item.CompletionPercentage);
        }

        [Fact]
        public async Task GetReport_CountsFinishedMeetingsCorrectly()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            var meetings = new List<Meeting>
            {
                new Meeting
                {
                    MeetingID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Meeting 1",
                    Description = "Description",
                    StartDate = new DateTime(2024, 1, 15),
                    EndDate = new DateTime(2024, 1, 15, 1, 0, 0),
                    MinutesDuration = 60
                },
                new Meeting
                {
                    MeetingID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Meeting 2",
                    Description = "Description",
                    StartDate = new DateTime(2024, 1, 20),
                    EndDate = new DateTime(2024, 1, 20, 1, 30, 0),
                    MinutesDuration = 90
                },
                new Meeting
                {
                    MeetingID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Meeting 3 - Not Finished",
                    Description = "Description",
                    StartDate = new DateTime(2024, 1, 25),
                    EndDate = null
                }
            };
            _context.Meetings.AddRange(meetings);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReport(_testUserId, startDate, endDate, 1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<Report>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Report>>(okObjectResult.Value);

            Assert.Equal(2, pagedResult.Item.FinishedMeetings);
        }

        [Fact]
        public async Task GetReport_CalculatesFocusMinutesCorrectly()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            var events = new List<Events>
            {
                new Events
                {
                    EventID = Guid.NewGuid(),
                    UserID = _testUserId,
                    EventType = EventType.END_FOCUS_SESSION,
                    StartDate = new DateTime(2024, 1, 15),
                    EndDate = new DateTime(2024, 1, 15, 1, 0, 0),
                    Duration = 60
                },
                new Events
                {
                    EventID = Guid.NewGuid(),
                    UserID = _testUserId,
                    EventType = EventType.END_FOCUS_SESSION,
                    StartDate = new DateTime(2024, 1, 20),
                    EndDate = new DateTime(2024, 1, 20, 1, 30, 0),
                    Duration = 90
                },
                new Events
                {
                    EventID = Guid.NewGuid(),
                    UserID = _testUserId,
                    EventType = EventType.START_FOCUS_SESSION,
                    StartDate = new DateTime(2024, 1, 25),
                    Duration = 45
                }
            };
            _context.Events.AddRange(events);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReport(_testUserId, startDate, endDate, 1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<Report>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Report>>(okObjectResult.Value);

            Assert.Equal(150, pagedResult.Item.FocusMinutes); // 60 + 90 = 150 (only END_FOCUS_SESSION events)
        }

        [Fact]
        public async Task GetReport_FiltersDataByDateRange()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            // Task inside range
            var taskInRange = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "In Range",
                Description = "Description",
                Priority = Priority.HIGH,
                EstimateMinutes = 60,
                TaskSituation = TaskSituation.COMPLETED,
                CreatedTask = new DateTime(2024, 1, 15),
                FinalDateTask = new DateTime(2024, 1, 20)
            };

            // Task outside range
            var taskOutOfRange = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "Out of Range",
                Description = "Description",
                Priority = Priority.HIGH,
                EstimateMinutes = 60,
                TaskSituation = TaskSituation.COMPLETED,
                CreatedTask = new DateTime(2024, 2, 15),
                FinalDateTask = new DateTime(2024, 2, 20)
            };

            _context.Tasks.AddRange(taskInRange, taskOutOfRange);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReport(_testUserId, startDate, endDate, 1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<Report>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Report>>(okObjectResult.Value);

            Assert.Equal(1, pagedResult.Item.CompletedTasks); // Only the task in range
        }

        [Fact]
        public async Task GetReport_FiltersDataByUserId()
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);
            var otherUserId = Guid.NewGuid();

            // Task for test user
            var taskForTestUser = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = _testUserId,
                Title = "Test User Task",
                Description = "Description",
                Priority = Priority.HIGH,
                EstimateMinutes = 60,
                TaskSituation = TaskSituation.COMPLETED,
                CreatedTask = new DateTime(2024, 1, 15),
                FinalDateTask = new DateTime(2024, 1, 20)
            };

            // Task for other user
            var taskForOtherUser = new Tasks
            {
                TaskID = Guid.NewGuid(),
                UserID = otherUserId,
                Title = "Other User Task",
                Description = "Description",
                Priority = Priority.HIGH,
                EstimateMinutes = 60,
                TaskSituation = TaskSituation.COMPLETED,
                CreatedTask = new DateTime(2024, 1, 15),
                FinalDateTask = new DateTime(2024, 1, 20)
            };

            _context.Tasks.AddRange(taskForTestUser, taskForOtherUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReport(_testUserId, startDate, endDate, 1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<Report>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<Report>>(okObjectResult.Value);

            Assert.Equal(1, pagedResult.Item.CompletedTasks);
            Assert.Equal(_testUserId, pagedResult.Item.UserID);
        }

        private void SeedTestData()
        {
            var tasks = new List<Tasks>
            {
                new Tasks
                {
                    TaskID = Guid.NewGuid(),
                    UserID = _testUserId,
                    Title = "Task 1",
                    Description = "Description",
                    Priority = Priority.HIGH,
                    EstimateMinutes = 60,
                    TaskSituation = TaskSituation.COMPLETED,
                    CreatedTask = new DateTime(2024, 1, 15),
                    FinalDateTask = new DateTime(2024, 1, 20)
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

