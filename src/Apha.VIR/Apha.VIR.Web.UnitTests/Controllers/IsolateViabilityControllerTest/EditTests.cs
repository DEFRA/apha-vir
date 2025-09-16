using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateViabilityControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class EditTests
    {
        private readonly object _lock;
        private readonly IIsolateViabilityService _isolateViabilityService;
        private readonly IMapper _mapper;
        private readonly IsolateViabilityController _controller;
        private readonly ILookupService _lookupService;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public EditTests(AppRolesFixture fixture)
        {
            _lookupService = Substitute.For<ILookupService>();
            _isolateViabilityService = Substitute.For<IIsolateViabilityService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new IsolateViabilityController(_isolateViabilityService, _lookupService, _mapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task Edit_Get_ValidInput_ReturnsViewResult()
        {
            // Arrange
            var avNumber = "AV123";
            var isolate = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            var viabilityHistory = new List<IsolateViabilityInfoDto> { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } };
            var isolateViabilityModelList = new List<IsolateViabilityModel> { new IsolateViabilityModel { IsolateViabilityId = isolateViabilityId } };
            var MapviabilityList = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Test Viability" } };
            var MapStaffList = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Test Staff" } };

            _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolate).Returns(viabilityHistory);
            _lookupService.GetAllViabilityAsync().Returns(MapviabilityList);
            _lookupService.GetAllStaffAsync().Returns(MapStaffList);

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(Arg.Any<IEnumerable<IsolateViabilityInfoDto>>())
           .Returns(isolateViabilityModelList);

            _mapper.Map<IsolateViabilityModel>(Arg.Any<IsolateViabilityModel>()).Returns(isolateViabilityModelList.First());

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(Arg.Any<IEnumerable<IsolateViability>>()).Returns(isolateViabilityModelList);

            // Act
            var result = await _controller.Edit(avNumber, isolate, isolateViabilityId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            var model = Assert.IsType<IsolateViabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.IsolateViability);
            Assert.NotNull(model.ViabilityList);
            Assert.NotEmpty(model.ViabilityList);
            Assert.NotEmpty(model.ViabilityList);
            Assert.NotNull(model.CheckedByList);
            Assert.NotEmpty(model.CheckedByList);
            Assert.NotEmpty(model.CheckedByList);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Edit_Get_InvalidAVNumber_ReturnsBadRequest(string avNumber)
        {
            // Arrange
            var isolate = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            // Act
            var result = await _controller.Edit(avNumber, isolate, isolateViabilityId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_InvalidIsolate_ReturnsBadRequest()
        {
            // Arrange
            var avNumber = "AV123";
            var isolate = Guid.Empty;
            var isolateViabilityId = Guid.NewGuid();

            // Act
            var result = await _controller.Edit(avNumber, isolate, isolateViabilityId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_MultipleViabilityHistoriesFound_ReturnsViewResultWithFirstHistory()
        {
            // Arrange
            var avNumber = "AV123";
            var isolate = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();
            var viabilityHistory = new List<IsolateViabilityInfoDto>
                                    {
                                    new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId },
                                    new IsolateViabilityInfoDto { IsolateViabilityId = Guid.NewGuid() }
                                    };
            var isolateViabilityModelList = new List<IsolateViabilityModel>
            { new IsolateViabilityModel { IsolateViabilityId = isolateViabilityId } };

            _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolate).Returns(viabilityHistory);
            _lookupService.GetAllViabilityAsync().Returns(new List<LookupItemDto>());
            _lookupService.GetAllStaffAsync().Returns(new List<LookupItemDto>());

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(Arg.Any<IEnumerable<IsolateViabilityInfoDto>>())
            .Returns(isolateViabilityModelList);

            _mapper.Map<IsolateViabilityModel>(Arg.Any<IsolateViabilityModel>()).Returns(isolateViabilityModelList.First());

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(Arg.Any<IEnumerable<IsolateViability>>()).Returns(isolateViabilityModelList);

            // Act
            var result = await _controller.Edit(avNumber, isolate, isolateViabilityId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            var model = Assert.IsType<IsolateViabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.IsolateViability);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = Guid.NewGuid()
                }
            };

            var dto = new IsolateViabilityInfoDto();
            _mapper.Map<IsolateViabilityInfoDto>(model.IsolateViability).Returns(dto);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(IsolateViabilityController.History), redirectResult.ActionName);
            Assert.NotNull(redirectResult);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.True(redirectResult.RouteValues.ContainsKey("AVNumber"));
            Assert.Equal(model.IsolateViability.AVNumber, redirectResult.RouteValues["AVNumber"]);
            Assert.Equal(model.IsolateViability.IsolateViabilityIsolateId, redirectResult.RouteValues["Isolate"]);

            await _isolateViabilityService.Received(1).UpdateIsolateViabilityAsync(dto, "TestUser");
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
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