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
    public class SubmissionDeleteTests
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

        public SubmissionDeleteTests(AppRolesFixture fixture)
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
        public async Task SubmissionDelete_NoSamplesOrIsolates_ReturnsSuccessJson()
        {
            // Arrange
            var avNumber = "AV12345";
            var submissionId = Guid.NewGuid();
            var lastModified = new byte[] { 0x00, 0x01, 0x02 };

            _mockSampleService.GetSamplesBySubmissionIdAsync(submissionId).Returns(new List<SampleDto>());
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfoDto>());
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.SubmissionDelete(avNumber, submissionId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.True(success);
            Assert.Equal("Submission deleted successfully.", message);         

            await _mockSubmissionService.Received(1).DeleteSubmissionAsync(submissionId, "TestUser", lastModified);
        }

        [Fact]
        public async Task SubmissionDelete_SamplesExist_ReturnsFailureJson()
        {
            // Arrange
            var avNumber = "AV12345";
            var submissionId = Guid.NewGuid();
            var lastModified = new byte[] { 0x00, 0x01, 0x02 };

            _mockSampleService.GetSamplesBySubmissionIdAsync(submissionId).Returns(new List<SampleDto> { new SampleDto() });
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfoDto>());
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.SubmissionDelete(avNumber, submissionId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.False(success);
            Assert.Equal("Unable to delete this Submission, it still has Isolates or Samples.", message);
         
            await _mockSubmissionService.DidNotReceive().DeleteSubmissionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<byte[]>());
        }

        [Fact]
        public async Task SubmissionDelete_IsolatesExist_ReturnsFailureJson()
        {
            // Arrange
            var avNumber = "AV12345";
            var submissionId = Guid.NewGuid();
            var lastModified = new byte[] { 0x00, 0x01, 0x02 };

            _mockSampleService.GetSamplesBySubmissionIdAsync(submissionId).Returns(new List<SampleDto>());
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfoDto> { new IsolateInfoDto() });
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.SubmissionDelete(avNumber, submissionId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.False(success);
            Assert.Equal("Unable to delete this Submission, it still has Isolates or Samples.", message);
            await _mockSubmissionService.DidNotReceive().DeleteSubmissionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<byte[]>());
        }

        [Fact]
        public async Task SubmissionDelete_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.SubmissionDelete("AV12345", Guid.NewGuid(), new byte[] { 0x00 });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
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
