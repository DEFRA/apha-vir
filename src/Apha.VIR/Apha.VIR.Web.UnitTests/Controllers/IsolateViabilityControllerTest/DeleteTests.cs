using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateViabilityControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class DeleteTests
    {
        private readonly object _lock;
        private readonly IIsolateViabilityService _isolateViabilityService;
        private readonly ILookupService _lookupService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IsolateViabilityController _controller;        
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public DeleteTests(AppRolesFixture fixture)
        {
            _lookupService = Substitute.For<ILookupService>();
            _isolateViabilityService = Substitute.For<IIsolateViabilityService>();
            _cacheService = Substitute.For<ICacheService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new IsolateViabilityController(_isolateViabilityService, 
                _lookupService,
                _cacheService,
                _mapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task Delete_ValidInput_ReturnsRedirectToActionResult()
        {
            // Arrange
            var isolateViabilityId = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = isolateId,
                    IsolateViabilityId = isolateViabilityId,
                    LastModified = new byte[8]
                }
            };

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
            .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });

            _isolateViabilityService.DeleteIsolateViabilityAsync(Arg.Any<Guid>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Delete(isolateViabilityId, lastModified, avNumber, isolateId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(IsolateViabilityController.History), redirectResult.ActionName);

            Assert.NotNull(redirectResult);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.True(redirectResult.RouteValues.ContainsKey("AVNumber"));
            Assert.Equal(avNumber, redirectResult.RouteValues["AVNumber"]);
            Assert.Equal(isolateId, redirectResult.RouteValues["Isolate"]);

            await _isolateViabilityService.Received(1).DeleteIsolateViabilityAsync(
            Arg.Is<Guid>(g => g == isolateViabilityId),
            Arg.Is<byte[]>(b => b.SequenceEqual(Convert.FromBase64String(lastModified))),
            Arg.Is<string>(s => s == "TestUser")
            );
        }

        [Fact]
        public async Task Delete_ServiceThrowsException_ThrowsException()
        {
            // Arrange
            var isolateViabilityId = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = isolateId,
                    IsolateViabilityId = isolateViabilityId,
                    LastModified = new byte[8]
                }
            };

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
            .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });

            _isolateViabilityService.DeleteIsolateViabilityAsync(Arg.Any<Guid>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns(Task.FromException(new Exception("Service error")));
            SetupMockUserAndRoles();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(isolateViabilityId, lastModified, avNumber, isolateId));
        }

        [Fact]
        public async Task Delete_InvalidLastModified_ThrowsFormatException()
        {
            // Arrange
            var isolateViabilityId = Guid.NewGuid();
            var lastModified = "invalid_base64";
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = isolateId,
                    IsolateViabilityId = isolateViabilityId,
                    LastModified = new byte[8]
                }
            };

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
            .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });
            SetupMockUserAndRoles();

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() => _controller.Delete(isolateViabilityId, lastModified, avNumber, isolateId));
        }

        [Fact]
        public async Task Delete_InvalidModelState_ReturnsBadRequest()
        {
            var isolateId = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = isolateId,
                    IsolateViabilityId = isolateViabilityId,
                    LastModified = new byte[8]
                }
            };

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
            .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });

            SetupMockUserAndRoles();
            
            // Arrange
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.Delete(Guid.NewGuid(), "validBase64", "AV123", Guid.NewGuid());

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Delete_EmptyViabilityId_ShouldReturnBadRequest()
        {
            var isolateId = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = isolateId,
                    IsolateViabilityId = isolateViabilityId,
                    LastModified = new byte[8]
                }
            };

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
            .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });
            SetupMockUserAndRoles();
            
            // Arrange
            var result = await _controller.Delete(Guid.Empty, "validBase64", "AV123", Guid.NewGuid());

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Delete_UserNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var isolateViabilityId = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
                .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });

            // Simulate user not authorized
            AuthorisationUtil.AppRoles = new List<string>(); // No roles

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Delete(isolateViabilityId, lastModified, avNumber, isolateId));
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {    new Claim(ClaimTypes.Name, "TestUser"),
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
