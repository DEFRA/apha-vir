using System.Security.Claims;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsListEntryControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class DeleteTests
    {
        private readonly object _lock;
        private readonly IVirusCharacteristicService _service;
        private readonly IVirusCharacteristicListEntryService _listEntryService;
        private readonly IMapper _mapper;
        private readonly VirusCharacteristicsListEntryController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public DeleteTests(AppRolesFixture fixture)
        {
            _service = Substitute.For<IVirusCharacteristicService>();
            _listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new VirusCharacteristicsListEntryController(_service, _listEntryService, _mapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task Delete_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Delete(Guid.NewGuid(), Guid.NewGuid(), "dGVzdA==");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Delete_Valid_RedirectsToListEntries()
        {
            // Arrange
            var id = Guid.NewGuid();
            var characteristic = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });

            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Delete(id, characteristic, lastModified);

            // Assert
            await _listEntryService.Received(1).DeleteEntryAsync(id, Arg.Any<byte[]>());
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ListEntries", redirect.ActionName);
            Assert.Equal("VirusCharacteristicsListEntry", redirect.ControllerName);

            Assert.NotNull(redirect.RouteValues);
            Assert.Equal(characteristic, redirect.RouteValues["characteristic"]);
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
