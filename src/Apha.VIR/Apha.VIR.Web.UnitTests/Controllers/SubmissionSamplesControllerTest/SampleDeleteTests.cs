using System.Security.Claims;
using System.Text.Json;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
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
    public class SampleDeleteTests
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

        public SampleDeleteTests(AppRolesFixture fixture)
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
        public async Task SampleDelete_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.SampleDelete("AV123", Guid.NewGuid(), Array.Empty<byte>());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SampleDelete_IsolateExistsForSample_ReturnsJsonWithFailure()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();
            var sampleId = Guid.NewGuid();
            var isolates = new List<IsolateInfoDto> { new IsolateInfoDto { IsolateId = isolateId, IsolateSampleId = sampleId } };
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolates);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.SampleDelete(avNumber, sampleId, Array.Empty<byte>());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.False(success);
            Assert.Equal("You cannot delete this Sample as it still has Isolates recorded against it.", message);
        }

        [Fact]
        public async Task SampleDelete_SuccessfulDeletion_ReturnsJsonWithSuccess()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var lastModified = Array.Empty<byte>();
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfoDto>());
            _mockSampleService.DeleteSampleAsync(sampleId, "testUser", lastModified).Returns(Task.CompletedTask);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.SampleDelete(avNumber, sampleId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.True(success);
            Assert.Equal("Sample deleted successfully.", message);
            await _mockSampleService.Received(1).DeleteSampleAsync(sampleId, "TestUser", lastModified);
        }

        [Fact]
        public async Task SampleDelete_SampleDoesNotExist_ReturnsJsonWithSuccess()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var lastModified = Array.Empty<byte>();
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfoDto>());
            _mockSampleService.DeleteSampleAsync(sampleId, "TestUser", lastModified).Returns(Task.CompletedTask);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.SampleDelete(avNumber, sampleId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.True(success);
            Assert.Equal("Sample deleted successfully.", message);
            await _mockSampleService.Received(1).DeleteSampleAsync(sampleId, "TestUser", lastModified);
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.IsolateDeleter),
                    new Claim(ClaimTypes.Name, "TestUser"),
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.IsolateManager, AppRoleConstant.IsolateViewer, AppRoleConstant.IsolateDeleter };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
