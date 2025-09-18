using System.Security.Claims;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SenderControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class DeleteTests
    {
        private readonly object _lock;
        private readonly ISenderService _senderService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly SenderController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public DeleteTests(AppRolesFixture fixture)
        {
            _senderService = Substitute.For<ISenderService>();
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SenderController(_senderService, _lookupService, _mapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }


        [Fact]
        public async Task Delete_DeleteSenderAndRedirectToIndex_WhenValidModel()
        {
            // Arrange
            var model = new SenderViewModel { SenderName = "Test Sender", SenderAddress = "test", SenderOrganisation = "India" };
            var senderId = Guid.NewGuid();
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Delete(model, senderId);

            // Assert
            await _senderService.Received(1).DeleteSenderAsync(senderId);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(SenderController.Index), redirectResult.ActionName);
        }

        [Fact]
        public async Task Delete_ReturnsViewWithError_WhenInvalidModelState()
        {
            // Arrange
            var model = new SenderViewModel { SenderName = "Test Sender", SenderAddress = "test", SenderOrganisation = "India" };
            var senderId = Guid.NewGuid();
            _controller.ModelState.AddModelError("error", "some error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Delete(model, senderId);

            // Assert
            await _lookupService.Received(1).GetAllCountriesAsync();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditSender", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Delete_ReturnsViewWithModel_WhenEmptySenderId()
        {
            // Arrange
            var model = new SenderViewModel { SenderName = "Test Sender", SenderAddress = "test", SenderOrganisation = "India" };
            var senderId = Guid.Empty;
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Delete(model, senderId);

            // Assert
            await _lookupService.Received(1).GetAllCountriesAsync();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditSender", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
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
