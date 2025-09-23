using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SenderControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class CreateTests
    {
        private readonly object _lock;
        private readonly ISenderService _senderService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly SenderController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public CreateTests(AppRolesFixture fixture)
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
        public async Task Create_Get_ReturnsViewWithModel()
        {
            // Arrange
            var countries = new List<SelectListItem> { new SelectListItem { Value = "1", Text = "Country" } };

            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDto>
            { new LookupItemDto { Id = Guid.NewGuid(), Name = "Country" } });

            _mapper.Map<IEnumerable<SelectListItem>>(Arg.Any<IEnumerable<LookupItemDto>>()).Returns(countries);

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SenderViewModel>(viewResult.Model);
            Assert.Equal("CreateSender", viewResult.ViewName);
            Assert.NotNull(model.CountryList);
            Assert.Single(model.CountryList);
        }

        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenValidModel()
        {
            // Arrange
            var model = new SenderViewModel { SenderName = "Test Sender", SenderAddress = "test", SenderOrganisation = "India" };
            _mapper.Map<SenderDto>(model).Returns(new SenderDto());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(SenderController.Index), redirectResult.ActionName);
            await _senderService.Received(1).AddSenderAsync(Arg.Any<SenderDto>());
        }

        [Fact]
        public async Task Create_Postl_ReturnsViewWithModelWithError_WhenInvalidMode()
        {
            // Arrange
            var model = new SenderViewModel { SenderName = "", SenderAddress = "test", SenderOrganisation = "India" };

            _controller.ModelState.AddModelError("SenderName", "Required");
            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("CreateSender", viewResult.ViewName);
            Assert.IsType<SenderViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_RedirectToIndex_WhenEmptyModel()
        {
            // Arrange
            SenderViewModel model = null!;
            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", viewResult.ActionName);
        }

        [Fact]
        public async Task Create_Get_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Create();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }
        [Fact]
        public async Task Create_Post_ThrowsUnauthorizedAccessException_WhenNotAuthorized()
        {
            // Arrange
            var model = new SenderViewModel { SenderName = "Test Sender", SenderAddress = "test", SenderOrganisation = "India" };
            // Simulate not authorized
            AuthorisationUtil.AppRoles = new List<string>(); 

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Create(model));
            Assert.Equal("Insert not supported for Sender.", ex.Message);
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
