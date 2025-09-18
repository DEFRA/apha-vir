using System.Security.Claims;
using Apha.VIR.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.AccountControllerTest
{
    public class AccountControllerTests
    {
        private readonly ILogger<AccountController> _mockLogger;
        private readonly IConfiguration _mockConfiguration;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockLogger = Substitute.For<ILogger<AccountController>>();
            _mockConfiguration = Substitute.For<IConfiguration>();
            _controller = new AccountController(_mockLogger, _mockConfiguration);
        }

        [Fact]
        public void AccessDenied_ValidModelState_ReturnsViewResult()
        {
            // Arrange
            string returnUrl = "/home";
            SetupUserIdentity("testuser");
            _mockConfiguration["ExceptionTypes:Authorization"].Returns("TestErrorType");

            // Act
            var result = _controller.AccessDenied(returnUrl);

            // Assert
            Assert.IsType<ViewResult>(result);
            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<UnauthorizedAccessException>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
            Assert.Equal(returnUrl, _controller.ViewBag.ReturnUrl);
        }

        [Fact]
        public void AccessDenied_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Model error");
            SetupUserIdentity("testuser");

            // Act
            var result = _controller.AccessDenied(string.Empty);

            // Assert
            Assert.IsType<ViewResult>(result);
            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<UnauthorizedAccessException>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Fact]
        public void AccessDenied_NullAuthorizationConfig_UsesDefaultErrorType()
        {
            // Arrange
            SetupUserIdentity("testuser");
            _mockConfiguration["ExceptionTypes:Authorization"].Returns((string?)null);

            // Act
            var result = _controller.AccessDenied("/home");

            // Assert
            Assert.IsType<ViewResult>(result);
            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<UnauthorizedAccessException>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Fact]
        public void AccessDenied_NullReturnUrl_HandlesNullGracefully()
        {
            // Arrange
            SetupUserIdentity("testuser");

            // Act
            var result = _controller.AccessDenied(string.Empty);

            // Assert
            Assert.IsType<ViewResult>(result);
            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<UnauthorizedAccessException>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
            Assert.Equal(string.Empty, _controller.ViewBag.ReturnUrl);
        }

        [Fact]
        public void AccessDenied_NullUserIdentity_HandlesNullGracefully()
        {
            // Arrange
            SetupUserIdentity(null);

            // Act
            var result = _controller.AccessDenied("/home");

            // Assert
            Assert.IsType<ViewResult>(result);
            _mockLogger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<UnauthorizedAccessException>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        private void SetupUserIdentity(string? username)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username ?? string.Empty)
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
    }
}
