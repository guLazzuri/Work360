using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NuGet.ContentModel;
using Work360.Domain.DTO;
using Work360.Infrastructure.Services;
using Xunit;

namespace Work360.WTests
{
    public class HateoasServiceTests
    {
        private readonly HateoasService _service;
        private readonly Mock<IUrlHelper> _mockUrlHelper;

        public HateoasServiceTests()
        {
            _service = new HateoasService();
            _mockUrlHelper = new Mock<IUrlHelper>();
        }

        [Fact]
        public void GenerateResourceLinks_ReturnsThreeLinks()
        {
            // Arrange
            var resourceName = "User";
            var resourceId = Guid.NewGuid();

            _mockUrlHelper
                .Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GenerateResourceLinks(resourceName, resourceId, _mockUrlHelper.Object);

            // Assert
            Assert.Equal(3, links.Count);
        }

        [Fact]
        public void GenerateResourceLinks_ContainsSelfLink()
        {
            // Arrange
            var resourceName = "User";
            var resourceId = Guid.NewGuid();

            _mockUrlHelper
                .Setup(x => x.Link("GetUser", It.IsAny<object>()))
                .Returns("http://localhost/api/user/" + resourceId);

            _mockUrlHelper
                .Setup(x => x.Link(It.IsNotIn("GetUser"), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GenerateResourceLinks(resourceName, resourceId, _mockUrlHelper.Object);

            // Assert
            var selfLink = links.FirstOrDefault(l => l.Rel == "self");
            Assert.NotNull(selfLink);
            Assert.Equal("GET", selfLink.Method);
        }

        [Fact]
        public void GenerateResourceLinks_ContainsUpdateLink()
        {
            // Arrange
            var resourceName = "User";
            var resourceId = Guid.NewGuid();

            _mockUrlHelper
                .Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GenerateResourceLinks(resourceName, resourceId, _mockUrlHelper.Object);

            // Assert
            var updateLink = links.FirstOrDefault(l => l.Rel == "update");
            Assert.NotNull(updateLink);
            Assert.Equal("PUT", updateLink.Method);
        }

        [Fact]
        public void GenerateResourceLinks_ContainsDeleteLink()
        {
            // Arrange
            var resourceName = "User";
            var resourceId = Guid.NewGuid();

            _mockUrlHelper
                .Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GenerateResourceLinks(resourceName, resourceId, _mockUrlHelper.Object);

            // Assert
            var deleteLink = links.FirstOrDefault(l => l.Rel == "delete");
            Assert.NotNull(deleteLink);
            Assert.Equal("DELETE", deleteLink.Method);
        }

        [Fact]
        public void GeneratePaginationLinks_ReturnsEmptyList_WhenNoNavigationNeeded()
        {
            // Arrange
            var pagedResult = new PagedResult<string>
            {
                Items = new List<string> { "item1", "item2" },
                CurrentPage = 1,
                PageSize = 10,
                TotalItems = 2
            };

            _mockUrlHelper
                .Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GeneratePaginationLinks(pagedResult, "User", _mockUrlHelper.Object);

            // Assert
            Assert.Empty(links); // No prev/next links when on single page
        }

        [Fact]
        public void GeneratePaginationLinks_ContainsNextAndLastLinks_WhenHasNextPage()
        {
            // Arrange
            var pagedResult = new PagedResult<string>
            {
                Items = new List<string> { "item1", "item2" },
                CurrentPage = 1,
                PageSize = 2,
                TotalItems = 10
            };

            _mockUrlHelper
                .Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GeneratePaginationLinks(pagedResult, "User", _mockUrlHelper.Object);

            // Assert
            Assert.Contains(links, l => l.Rel == "next");
            Assert.Contains(links, l => l.Rel == "last");
        }

        [Fact]
        public void GeneratePaginationLinks_ContainsPrevAndFirstLinks_WhenHasPreviousPage()
        {
            // Arrange
            var pagedResult = new PagedResult<string>
            {
                Items = new List<string> { "item1", "item2" },
                CurrentPage = 2,
                PageSize = 2,
                TotalItems = 10
            };

            _mockUrlHelper
                .Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GeneratePaginationLinks(pagedResult, "User", _mockUrlHelper.Object);

            // Assert
            Assert.Contains(links, l => l.Rel == "prev");
            Assert.Contains(links, l => l.Rel == "first");
        }

        [Fact]
        public void GeneratePaginationLinks_AllLinksUseGetMethod()
        {
            // Arrange
            var pagedResult = new PagedResult<string>
            {
                Items = new List<string> { "item1", "item2" },
                CurrentPage = 2,
                PageSize = 2,
                TotalItems = 10
            };

            _mockUrlHelper
                .Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GeneratePaginationLinks(pagedResult, "User", _mockUrlHelper.Object);

            // Assert
            Assert.All(links, link => Assert.Equal("GET", link.Method));
        }

        [Fact]
        public void GeneratePaginationLinks_NextLinkPointsToCorrectPage()
        {
            // Arrange
            var pagedResult = new PagedResult<string>
            {
                Items = new List<string> { "item1", "item2" },
                CurrentPage = 2,
                PageSize = 2,
                TotalItems = 10
            };

            _mockUrlHelper
                .Setup(x => x.Link("GetUsers", It.Is<object>(o =>
                    o.GetType().GetProperty("pageNumber")!.GetValue(o)!.Equals(3))))
                .Returns("http://localhost/api/users?pageNumber=3&pageSize=2");

            _mockUrlHelper
                .Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GeneratePaginationLinks(pagedResult, "User", _mockUrlHelper.Object);

            // Assert
            var nextLink = links.FirstOrDefault(l => l.Rel == "next");
            Assert.NotNull(nextLink);
        }

        [Fact]
        public void GeneratePaginationLinks_PrevLinkPointsToCorrectPage()
        {
            // Arrange
            var pagedResult = new PagedResult<string>
            {
                Items = new List<string> { "item1", "item2" },
                CurrentPage = 3,
                PageSize = 2,
                TotalItems = 10
            };

            _mockUrlHelper
                .Setup(x => x.Link("GetUsers", It.Is<object>(o =>
                    o.GetType().GetProperty("pageNumber")!.GetValue(o)!.Equals(2))))
                .Returns("http://localhost/api/users?pageNumber=2&pageSize=2");

            _mockUrlHelper
                .Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/api/test");

            // Act
            var links = _service.GeneratePaginationLinks(pagedResult, "User", _mockUrlHelper.Object);

            // Assert
            var prevLink = links.FirstOrDefault(l => l.Rel == "prev");
            Assert.NotNull(prevLink);
        }
    }
}

