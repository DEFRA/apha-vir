using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Diagnostics;

namespace Apha.VIR.Web.UnitTests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;
        private readonly IConfiguration _mockConfiguration;

        public HomeControllerTests()
        {
            _mockConfiguration = Substitute.For<IConfiguration>();
            _controller = new HomeController(_mockConfiguration);
        }

        [Fact]
        public void Index_ReturnsViewResult_WithCorrectUserMgmtUrl()
        {
            // Arrange
            var expectedUrl = "https://example.com/usermgmt";
            _mockConfiguration["URL:UserMgmt"].Returns(expectedUrl);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUrl, result.ViewData["UserMgmtUrl"]);
        }

        [Fact]
        public void Index_ThrowsInvalidOperationException_WhenUserMgmtUrlIsNotConfigured()
        {
            // Arrange
            _mockConfiguration["URL:UserMgmt"].Returns((string?)null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _controller.Index());
            Assert.Equal("Azure Entra Group/Role Management URL configuration setting was not found", exception.Message);
        }
    }
}