using System.Diagnostics;
using System.Text;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.HomeControllerTest
{
    public class HomeControllerTests
    {
        private readonly IConfiguration _mockConfiguration;
        private readonly ISystemInfoService _mockSystemInfoService;
        private readonly HomeController _controller;
        public HomeControllerTests()
        {
            _mockConfiguration = Substitute.For<IConfiguration>();
            _mockSystemInfoService = Substitute.For<ISystemInfoService>();
            _controller = new HomeController(_mockConfiguration, _mockSystemInfoService);

            // Setup HttpContext and Session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = Substitute.For<ISession>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public void Constructor_WithValidDependencies_Succeeds()
        {
            // Arrange
            var config = Substitute.For<IConfiguration>();
            var sysInfo = Substitute.For<ISystemInfoService>();

            // Act
            var controller = new HomeController(config, sysInfo);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullSystemInfoService_ThrowsArgumentNullException()
        {
            // Arrange
            var config = Substitute.For<IConfiguration>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new HomeController(config, null!));
            Assert.Equal("sysInfoService", ex.ParamName);
        }

        [Fact]
        public void Index_SetsViewBagUserMgmtUrlCorrectly()
        {
            // Arrange
            var expectedUrl = "http://example.com";
            _mockConfiguration["URL:UserMgmt"].Returns(expectedUrl);

            // Act
            _controller.Index();

            // Assert
            Assert.Equal(expectedUrl, _controller.ViewBag.UserMgmtUrl);
        }

        [Fact]
        public void Index_DoesNotSetSessionWhenEnvironmentNameIsNotNull()
        {
            // Arrange
            var expectedUrl = "http://example.com";
            _mockConfiguration["URL:UserMgmt"].Returns(expectedUrl);
       
            // Setup TryGetValue to simulate existing "EnvironmentName" session key
            byte[] valueBytes = Encoding.UTF8.GetBytes("Existing Environment");
            _controller.ControllerContext.HttpContext.Session.TryGetValue("EnvironmentName", out Arg.Any<byte[]>()!)
                   .Returns(call =>
                   {
                       // Set the out argument
                       call[1] = valueBytes;
                       return true;
                   });

            // Act
             _controller.Index();

            // Assert
            _controller.ControllerContext.HttpContext.Session.DidNotReceive().Set(Arg.Any<string>(), Arg.Any<byte[]>());
        }

        [Fact]
        public void Index_SetsSessionWhenEnvironmentNameIsNul1l()
        {
            // Arrange
            var expectedUrl = "http://example.com";
            _mockConfiguration["URL:UserMgmt"].Returns(expectedUrl);

            // Simulate missing "EnvironmentName" in session
            _controller.ControllerContext.HttpContext.Session
                .TryGetValue("EnvironmentName", out Arg.Any<byte[]>()!)
                .Returns(call =>
                {
                    // Set out argument to null
                    call[1] = null!;
                    return false;
                });

            // Act
            _controller.Index();

            // Assert: Session.Set should be called because EnvironmentName was missing
            _controller.ControllerContext.HttpContext.Session
                .Received(1)
                .Set(Arg.Is<string>(s => s == "EnvironmentName"), Arg.Any<byte[]>());
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

        [Fact]
        public void Error_ReturnsErrorViewModelWithTraceIdentifier()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.TraceIdentifier = "trace-123";

            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal("trace-123", model.RequestId);
        }

        [Fact]
        public void Error_ReturnsErrorViewModelWithActivityId_WhenAvailable()
        {
            // Arrange
            var activity = new Activity("test");
            activity.Start();
            activity.AddTag("id", "activity-123");

            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId);
            activity.Stop();
        }
    }
}