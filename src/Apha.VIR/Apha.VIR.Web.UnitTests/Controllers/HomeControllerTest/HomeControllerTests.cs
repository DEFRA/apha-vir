using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.HomeControllerTest
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;
        private readonly IConfiguration _mockConfiguration;
        private readonly ISystemInfoService _mockSysInfoService;
        public HomeControllerTests()
        {
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockSysInfoService = Substitute.For<ISystemInfoService>();
            _controller = new HomeController(_mockConfiguration, _mockSysInfoService);
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