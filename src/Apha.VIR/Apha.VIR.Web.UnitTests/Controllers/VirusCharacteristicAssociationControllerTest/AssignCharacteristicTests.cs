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
    public class AssignCharacteristicTests
    {
        private readonly object _lock;
        private readonly IVirusCharacteristicAssociationService _mockTypeCharacteristicService;
        private readonly VirusCharacteristicAssociationController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public AssignCharacteristicTests(AppRolesFixture fixture)
        {
            _mockTypeCharacteristicService = Substitute.For<IVirusCharacteristicAssociationService>();
            var mockLookupService = Substitute.For<ILookupService>();
            var mockCharacteristicService = Substitute.For<IVirusCharacteristicService>();
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;

            _controller = new VirusCharacteristicAssociationController(
            mockLookupService,
            mockCharacteristicService,
            _mockTypeCharacteristicService
            );
        }

        [Fact]
        public async Task AssignCharacteristic_ValidModelState_ReturnsOkResult()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.AssignCharacteristic(typeId, characteristicId);

            // Assert
            Assert.IsType<OkResult>(result);
            await _mockTypeCharacteristicService.Received(1).AssignCharacteristicToTypeAsync(typeId, characteristicId);
        }

        [Fact]
        public async Task AssignCharacteristic_InvalidModelState_ReturnsBadRequestResult()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.AssignCharacteristic(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
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
