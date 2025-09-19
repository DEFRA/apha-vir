using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateDispatchControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class IsolateDispatchControllerDeleteTests
    {
        private readonly object _lock;
        private readonly IIsolateDispatchService _mockIsolateDispatchService;
        private readonly ILookupService _mockLookupService;
        private readonly IIsolatesService _mockIsolatesService;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IMapper _mockMapper;
        private readonly IsolateDispatchController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public IsolateDispatchControllerDeleteTests(AppRolesFixture fixture)
        {
            _mockIsolateDispatchService = Substitute.For<IIsolateDispatchService>();
            _mockLookupService = Substitute.For<ILookupService>();
            _mockIsolatesService = Substitute.For<IIsolatesService>();
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _mockMapper = Substitute.For<IMapper>();
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;

            _controller = new IsolateDispatchController(_mockIsolateDispatchService,
                _mockLookupService,
                _mockIsolatesService,
                _mockSubmissionService,
                _mockSampleService,
                _mockMapper);
        }

        [Fact]
        public async Task Delete_ValidInput_ReturnsRedirectToActionResult()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var avnumber = "AV123";
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var model = new IsolateDispatchEditViewModel
            {
                Avnumber = avnumber,
                DispatchId = dispatchId,
                DispatchIsolateId = isolateId,
                ViabilityId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid(),
                DispatchedById = Guid.NewGuid(),
                LastModified = new byte[8]
            };
            _mockIsolateDispatchService.GetDispatchesHistoryAsync(avnumber, isolateId)
                .Returns(new[] { new IsolateDispatchInfoDto { DispatchId = dispatchId } });
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Delete(dispatchId, lastModified, isolateId, avnumber);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("History", redirectResult.ActionName);
            Assert.Equal(avnumber, redirectResult!.RouteValues?["AVNumber"]);
            Assert.Equal(isolateId, redirectResult!.RouteValues?["IsolateId"]);

            await _mockIsolateDispatchService.Received(1).DeleteDispatchAsync(
            dispatchId,
            Arg.Is<byte[]>(b => Convert.ToBase64String(b) == lastModified),
            "TestUser"
            );
        }

        [Fact]
        public async Task Delete_EmptyDispatchId_ReturnsBadRequest()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var avnumber = "AV123";
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var model = new IsolateDispatchEditViewModel
            {
                Avnumber = avnumber,
                DispatchId = dispatchId,
                DispatchIsolateId = isolateId,
                ViabilityId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid(),
                DispatchedById = Guid.NewGuid(),
                LastModified = new byte[8]
            };

            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Delete(dispatchId, lastModified, isolateId, avnumber);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Delete_InvalidLastModified_ReturnsBadRequest(string lastModified)
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var avnumber = "AV123";
            var model = new IsolateDispatchEditViewModel
            {
                Avnumber = avnumber,
                DispatchId = dispatchId,
                DispatchIsolateId = isolateId,
                ViabilityId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid(),
                DispatchedById = Guid.NewGuid(),
                LastModified = new byte[8]
            };
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Delete(dispatchId, lastModified, isolateId, avnumber);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Delete_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var isolateId = Guid.NewGuid();
            const string avNumber = "AV123";

            _controller.ModelState.AddModelError("Error", "Sample error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Delete(dispatchId, lastModified, isolateId, avNumber);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Delete_ValidInput_CallsDeleteDispatchAsyncWithCorrectParameters()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var avnumber = "AV123";
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var model = new IsolateDispatchEditViewModel
            {
                Avnumber = avnumber,
                DispatchId = dispatchId,
                DispatchIsolateId = isolateId,
                ViabilityId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid(),
                DispatchedById = Guid.NewGuid(),
                LastModified = new byte[8]
            };
            _mockIsolateDispatchService.GetDispatchesHistoryAsync(avnumber, isolateId)
                .Returns(new[] { new IsolateDispatchInfoDto { DispatchId = dispatchId } });

            SetupMockUserAndRoles();
            // Act
            await _controller.Delete(dispatchId, lastModified, isolateId, avnumber);

            // Assert
            await _mockIsolateDispatchService.Received(1).DeleteDispatchAsync(
            dispatchId,
            Arg.Is<byte[]>(b => Convert.ToBase64String(b) == lastModified),
            "TestUser"
            );
        }

        [Theory]
        [InlineData("search", "AV000000-01")]
        [InlineData("SEARCH", "AV000000-01")]
        [InlineData("SeArCh", "AV000000-01")]
        public void CancelAction_WhenSourceIsSearch_ReturnsRedirectToSearchRepository(string source, string avnumber)
        {
            // Act
            var result = _controller.CancelAction(source, avnumber);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Search", redirectResult.ActionName);
            Assert.Equal("SearchRepository", redirectResult.ControllerName);
        }

        [Theory]
        [InlineData("summary", "AV000000-01")]
        [InlineData("SUMMARY", "AV000000-01")]
        [InlineData("SuMmArY", "AV000000-01")]
        public void CancelAction_WhenSourceIsSummary_ReturnsRedirectToSummaryIndex(string source, string avnumbr)
        {
            // Act
            var result = _controller.CancelAction(source, avnumbr);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("SubmissionSamples", redirectResult.ControllerName);
        }

        [Theory]
        [InlineData("other", "AV000000-00")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void CancelAction_WhenSourceIsOther_ReturnsRedirectToIsolateDispatchCreate(string source, string avnumber)
        {
            // Act
            var result = _controller.CancelAction(source, avnumber);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirectResult.ActionName);
            Assert.Equal("IsolateDispatch", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Delete_Unauthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var avnumber = "AV123";
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            // Remove Administrator role
            AuthorisationUtil.AppRoles = new List<string> { AppRoleConstant.IsolateManager };
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Delete(dispatchId, lastModified, isolateId, avnumber));
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {   new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, AppRoleConstant.Administrator)
                    
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
