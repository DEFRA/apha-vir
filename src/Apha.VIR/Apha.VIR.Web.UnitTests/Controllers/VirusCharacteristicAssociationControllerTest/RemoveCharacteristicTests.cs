using System.Security.Claims;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicAssociationControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class RemoveCharacteristicTests
    {
        private readonly object _lock;
        private readonly VirusCharacteristicAssociationController _controller;
        private readonly ILookupService _mockLookupService;
        private readonly IVirusCharacteristicService _mockCharacteristicService;
        private readonly IVirusCharacteristicAssociationService _mockTypeCharacteristicService;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public RemoveCharacteristicTests(AppRolesFixture fixture)
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _mockCharacteristicService = Substitute.For<IVirusCharacteristicService>();
            _mockTypeCharacteristicService = Substitute.For<IVirusCharacteristicAssociationService>();
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
            _controller = new VirusCharacteristicAssociationController(
            _mockLookupService,
            _mockCharacteristicService,
            _mockTypeCharacteristicService);
        }

        [Fact]
        public async Task RemoveCharacteristic_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "test error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.RemoveCharacteristic(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RemoveCharacteristic_ValidRequest_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();
            SetupMockUserAndRoles();
            // Act
            await _controller.RemoveCharacteristic(typeId, characteristicId);

            // Assert
            await _mockTypeCharacteristicService.Received(1).RemoveCharacteristicFromTypeAsync(typeId, characteristicId);
        }

        [Fact]
        public async Task RemoveCharacteristic_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.RemoveCharacteristic(typeId, characteristicId);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.LookupDataManager)
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
