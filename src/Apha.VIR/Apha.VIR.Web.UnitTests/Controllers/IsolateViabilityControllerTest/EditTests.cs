using System.Security.Claims;
using System.Text;
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
            var isolateViabilityModelList = new List<IsolateViabilityModel> { new IsolateViabilityModel { IsolateViabilityId = isolateViabilityId, Viable=Guid.NewGuid(), CheckedById=Guid.NewGuid(), DateChecked = DateTime.Now,IsolateViabilityIsolateId=Guid.NewGuid() } };
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
            { new IsolateViabilityModel {IsolateViabilityId = isolateViabilityId, Viable=Guid.NewGuid(), CheckedById = Guid.NewGuid(), DateChecked = DateTime.Now,IsolateViabilityIsolateId=Guid.NewGuid() } };

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
            var isolateId = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = isolateId,
                    IsolateViabilityId = isolateViabilityId,
                    Viable=Guid.NewGuid(),
                    CheckedById =Guid.NewGuid(),
                    DateChecked = DateTime.Now,
                    LastModified = new byte[8]
                }
            };

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
            .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });

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

        [Fact]
        public async Task Edit_Post_ValidateModel_LastModifiedEmpty_AddsModelError()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = isolateId,
                    IsolateViabilityId = isolateViabilityId,
                    CheckedById = Guid.NewGuid(),
                    Viable=Guid.NewGuid(),
                    LastModified = Array.Empty<byte>(),
                    DateChecked = DateTime.Now
                }
            };

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
                .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });

            SetupMockUserAndRoles();

            // Act
            // Pass empty lastModified to trigger ModelState error
            var result = await _controller.Edit(model);

            // Assert
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState, kvp =>kvp.Value != null && kvp.Value.Errors != null &&
        kvp.Value.Errors.Any(e => e.ErrorMessage != null && e.ErrorMessage.Contains("Last Modified cannot be empty.")));
        }

        [Fact]
        public async Task Edit_Post_ModelStateInvalid_ReturnsEditViewWithModel()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = isolateId,
                    IsolateViabilityId = isolateViabilityId,
                    CheckedById=Guid.NewGuid(),
                    Viable=Guid.NewGuid(),
                    DateChecked = DateTime.Now,
                    LastModified = new byte[8]
                }
            };

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
                .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });

            // Simulate user authorized
            SetupMockUserAndRoles();

            // Force ModelState to be invalid after ValidateModel
            _controller.ModelState.AddModelError("key", "error");

            var viabilities = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Viable" } };
            var staffs = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Staff" } };
            _lookupService.GetAllViabilityAsync().Returns(viabilities);
            _lookupService.GetAllStaffAsync().Returns(staffs);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            var returnedModel = Assert.IsType<IsolateViabilityViewModel>(viewResult.Model);
            Assert.NotNull(returnedModel.ViabilityList);
            Assert.NotNull(returnedModel.CheckedByList);
        }

        [Fact]
        public async Task Edit_Post_UserNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = isolateId,
                    IsolateViabilityId = isolateViabilityId,
                    CheckedById = Guid.NewGuid(),
                    Viable=Guid.NewGuid(),
                    DateChecked = DateTime.Now,
                    LastModified = new byte[8]
                }
            };

            _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId)
                .Returns(new[] { new IsolateViabilityInfoDto { IsolateViabilityId = isolateViabilityId } });

            _mapper.Map<IsolateViabilityInfoDto>(model.IsolateViability).Returns(new IsolateViabilityInfoDto());

            // Simulate user not authorized
            AuthorisationUtil.AppRoles = new List<string>(); // No roles

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Edit(model));
        }


        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.Administrator),
                    new Claim(ClaimTypes.Name, "TestUser")
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}