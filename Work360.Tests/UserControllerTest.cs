using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Work360.Controller;
using Work360.Domain.DTO;
using Work360.Domain.Entity;
using Work360.Infrastructure.Context;
using Work360.Infrastructure.Services;
using Xunit;

namespace Work360.Tests
{
    public class UserControllerTests : IDisposable
    {
        private readonly Work360Context _context;
        private readonly Mock<IHateoasService> _mockHateoasService;
        private readonly Mock<ILogger<UserController>> _mockLogger;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<Work360Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Work360Context(options);
            _mockHateoasService = new Mock<IHateoasService>();
            _mockLogger = new Mock<ILogger<UserController>>();

            _controller = new UserController(_context, _mockHateoasService.Object, _mockLogger.Object);

            // Setup mock for HATEOAS service
            _mockHateoasService
                .Setup(x => x.GeneratePaginationLinks(It.IsAny<PagedResult<User>>(), It.IsAny<string>(), It.IsAny<IUrlHelper>()))
                .Returns(new List<LinkDto>());
        }

        [Fact]
        public async Task GetUsers_ReturnsOkResult_WithPaginatedUsers()
        {
            // Arrange
            SeedUsers();

            // Act
            var result = await _controller.GetUsers(1, 10);

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<User>>(okObjectResult.Value);

            Assert.Equal(3, pagedResult.Items.Count());
            Assert.Equal(3, pagedResult.TotalItems);
        }

        [Fact]
        public async Task GetUsers_ReturnsPaginatedResults_WhenPageSizeIsSpecified()
        {
            // Arrange
            SeedUsers();

            // Act
            var result = await _controller.GetUsers(1, 2);

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<User>>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var pagedResult = Assert.IsType<PagedResult<User>>(okObjectResult.Value);

            Assert.Equal(2, pagedResult.Items.Count());
            Assert.Equal(3, pagedResult.TotalItems);
            Assert.Equal(1, pagedResult.CurrentPage);
        }

        [Fact]
        public async Task GetUser_ReturnsOkResult_WhenUserExists()
        {
            // Arrange
            var user = new User { UserID = Guid.NewGuid(), Name = "Test User", Password = "password123" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetUser(user.UserID);

            // Assert
            var okResult = Assert.IsType<ActionResult<User>>(result);
            var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
            var returnedUser = Assert.IsType<User>(okObjectResult.Value);

            Assert.Equal(user.UserID, returnedUser.UserID);
            Assert.Equal("Test User", returnedUser.Name);
        }

        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _controller.GetUser(nonExistentId);

            // Assert
            var notFoundResult = Assert.IsType<ActionResult<User>>(result);
            Assert.IsType<NotFoundObjectResult>(notFoundResult.Result);
        }

        [Fact]
        public async Task PostUser_ReturnsCreatedAtAction_WithNewUser()
        {
            // Arrange
            var newUser = new User { UserID = Guid.NewGuid(), Name = "New User", Password = "newpassword" };

            // Act
            var result = await _controller.PostUser(newUser);

            // Assert
            var createdResult = Assert.IsType<ActionResult<User>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(createdResult.Result);
            var returnedUser = Assert.IsType<User>(createdAtActionResult.Value);

            Assert.Equal(newUser.UserID, returnedUser.UserID);
            Assert.Equal("New User", returnedUser.Name);

            // Verify user was added to database
            var userInDb = await _context.Users.FindAsync(newUser.UserID);
            Assert.NotNull(userInDb);
        }

        [Fact]
        public async Task PutUser_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var user = new User { UserID = Guid.NewGuid(), Name = "Original Name", Password = "password" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Name = "Updated Name";

            // Act
            var result = await _controller.PutUser(user.UserID, user);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify update in database
            var updatedUser = await _context.Users.FindAsync(user.UserID);
            Assert.Equal("Updated Name", updatedUser!.Name);
        }

        [Fact]
        public async Task PutUser_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var user = new User { UserID = Guid.NewGuid(), Name = "Test User", Password = "password" };

            // Act
            var result = await _controller.PutUser(Guid.NewGuid(), user);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentUser = new User { UserID = Guid.NewGuid(), Name = "Non Existent", Password = "password" };

            // Act
            var result = await _controller.PutUser(nonExistentUser.UserID, nonExistentUser);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenUserIsDeleted()
        {
            // Arrange
            var user = new User { UserID = Guid.NewGuid(), Name = "To Delete", Password = "password" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteUser(user.UserID);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify deletion in database
            var deletedUser = await _context.Users.FindAsync(user.UserID);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _controller.DeleteUser(nonExistentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        private void SeedUsers()
        {
            var users = new List<User>
            {
                new User { UserID = Guid.NewGuid(), Name = "User 1", Password = "pass1" },
                new User { UserID = Guid.NewGuid(), Name = "User 2", Password = "pass2" },
                new User { UserID = Guid.NewGuid(), Name = "User 3", Password = "pass3" }
            };

            _context.Users.AddRange(users);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}

