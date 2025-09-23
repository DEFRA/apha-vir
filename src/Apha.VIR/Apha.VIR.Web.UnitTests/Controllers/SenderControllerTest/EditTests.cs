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
    public class EditTests
    {
        private readonly object _lock;
        private readonly ISenderService _senderService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly SenderController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public EditTests(AppRolesFixture fixture)
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
        public async Task Edit_Get_ValidSenderId_ReturnsViewWithModel()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var SenderDto = new SenderDto { SenderId = senderId, SenderName = "Test Sender" };
            var senderViewModel = new SenderViewModel
            {
                SenderId = senderId,
                SenderName = "Test Sender",
                SenderAddress = "india",
                SenderOrganisation = "India"
            };
            var countryList = new List<SelectListItem> { new SelectListItem("Country", "1") };

            _senderService.GetSenderAsync(senderId).Returns(SenderDto);
            _mapper.Map<SenderViewModel>(SenderDto).Returns(senderViewModel);

            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDto>
            { new LookupItemDto { Id = Guid.NewGuid(), Name = "Country" } });

            // Act
            var result = await _controller.Edit(senderId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SenderViewModel>(viewResult.Model);
            Assert.Equal("EditSender", viewResult.ViewName);
            Assert.Equal(senderId, model.SenderId);
            Assert.Equal("Test Sender", model.SenderName);
            Assert.NotEmpty(model.CountryList!);
            await _senderService.Received(1).GetSenderAsync(senderId);
            await _lookupService.Received(1).GetAllCountriesAsync();
        }

        [Fact]
        public async Task Edit_Get_ReturnsBadRequest_WhenInvalidSenderId()
        {
            // Arrange
            var invalidSenderId = Guid.Empty;

            // Act
            var result = await _controller.Edit(invalidSenderId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_ReturnsBadRequest_WhenInvalidModelStat()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Edit(senderId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenSenderNotFound()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            _senderService.GetSenderAsync(senderId).Returns(new SenderDto { SenderId = Guid.Empty });

            // Act
            var result = await _controller.Edit(senderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Sender not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new SenderViewModel
            {
                SenderId = Guid.NewGuid(),
                SenderName = "Test Sender",
                SenderAddress = "India",
                SenderOrganisation = "India"
            };
            var SenderDto = new SenderDto();
            _mapper.Map<SenderDto>(model).Returns(SenderDto);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Edit(model);

            // Assert
            await _senderService.Received(1).UpdateSenderAsync(Arg.Is<SenderDto>(dto => dto == SenderDto));
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new SenderViewModel
            {
                SenderId = Guid.NewGuid(),
                SenderName = "",
                SenderAddress = "India",
                SenderOrganisation = "India"
            };

            _controller.ModelState.AddModelError("SenderName", "Required");
            var countryList = new List<SelectListItem>();
            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDto>());
            _mapper.Map<List<SelectListItem>>(Arg.Any<List<LookupItemDto>>()).Returns(countryList);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditSender", viewResult.ViewName);
            var returnedModel = Assert.IsType<SenderViewModel>(viewResult.Model);
            Assert.Equal(model, returnedModel);
            Assert.Equal(countryList, returnedModel.CountryList);
        }

        [Fact]
        public async Task Edit_Post_ThrowsUnauthorizedAccessException_WhenNotAuthorized()
        {
            // Arrange
            var model = new SenderViewModel
            {
                SenderId = Guid.NewGuid(),
                SenderName = "Test Sender",
                SenderAddress = "India",
                SenderOrganisation = "India"
            };
            // Simulate not authorized
            AuthorisationUtil.AppRoles = new List<string>();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Edit(model));
            Assert.Equal("Update not supported for Sender.", ex.Message);
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
