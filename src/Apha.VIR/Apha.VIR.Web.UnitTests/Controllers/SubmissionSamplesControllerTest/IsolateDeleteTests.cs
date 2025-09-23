using System.Security.Claims;
using System.Text.Json;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SubmissionSamplesControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class IsolateDeleteTests
    {
        private readonly object _lock;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IIsolatesService _mockIsolatesService;
        private readonly IIsolateDispatchService _mockIsolatesDispatchService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mockMapper;
        private readonly SubmissionSamplesController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public IsolateDeleteTests(AppRolesFixture fixture)
        {
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _mockIsolatesService = Substitute.For<IIsolatesService>();
            _mockIsolatesDispatchService = Substitute.For<IIsolateDispatchService>();
            _cacheService = Substitute.For<ICacheService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new SubmissionSamplesController(_mockSubmissionService, 
                _mockSampleService, 
                _mockIsolatesService, 
                _mockIsolatesDispatchService, 
                _cacheService,
                _mockMapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task IsolateDelete_SuccessfulDeletion_ReturnsJsonResult()
        {
            // Arrange
            string avNumber = "AV123";
            Guid isolateId = Guid.NewGuid();
            byte[] lastModified = new byte[] { 0x00, 0x01, 0x02 };

            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber)
            .Returns(new[] { new IsolateInfoDto { IsolateId = isolateId } });

            _mockIsolatesDispatchService.GetIsolateDispatchRecordCountAsync(isolateId)
            .Returns(0);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.IsolateDelete(avNumber, isolateId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.True(success);
            Assert.Equal("Isolate deleted successfully.", message);
           
            await _mockIsolatesService.Received(1).DeleteIsolateAsync(isolateId, "TestUser", lastModified);
        }

        [Fact]
        public async Task IsolateDelete_IsolateWithDispatches_ReturnsJsonResultWithError()
        {
            // Arrange
            string avNumber = "AV123";
            Guid isolateId = Guid.NewGuid();
            byte[] lastModified = new byte[] { 0x00, 0x01, 0x02 };

            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber)
            .Returns(new[] { new IsolateInfoDto { IsolateId = isolateId } });

            _mockIsolatesDispatchService.GetIsolateDispatchRecordCountAsync(isolateId)
            .Returns(1);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.IsolateDelete(avNumber, isolateId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.False(success);
            Assert.Equal("Isolate cannot be deleted as it has one or more dispatches recorded against it.", message);           

            await _mockIsolatesService.DidNotReceive().DeleteIsolateAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<byte[]>());
        }

        [Fact]
        public async Task IsolateDelete_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.IsolateDelete("AV123", Guid.NewGuid(), new byte[] { 0x00 });

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task IsolateDelete_IsolateNotFound_ReturnsJsonResultWithSuccess()
        {
            // Arrange
            string avNumber = "AV123";
            Guid isolateId = Guid.NewGuid();
            byte[] lastModified = new byte[] { 0x00, 0x01, 0x02 };

            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber)
            .Returns(Array.Empty<IsolateInfoDto>());
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.IsolateDelete(avNumber, isolateId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.True(success);
            Assert.Equal("Isolate deleted successfully.", message);
           
            await _mockIsolatesService.Received(1).DeleteIsolateAsync(isolateId, "TestUser", lastModified);
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.IsolateDeleter),
                    new Claim(ClaimTypes.Name, "TestUser")
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.IsolateManager, AppRoleConstant.IsolateViewer, AppRoleConstant.IsolateDeleter };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
