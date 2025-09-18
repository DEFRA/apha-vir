using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.Lookup;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.LookupControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class CreateTests
    {
        private readonly object _lock;
        private readonly ILookupService _mockLookupService;
        private readonly IMapper _mockMapper;
        private readonly LookupController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public CreateTests(AppRolesFixture fixture)
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new LookupController(_mockLookupService, _mockMapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task Create_Get_ValidInput_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupResult = new LookupDto { Id = lookupId, Name = "Test Lookup" };
            var lookupViewModel = new LookupViewModel { Id = lookupId, Name = "Test Lookup" };

            _mockLookupService.GetLookupByIdAsync(lookupId).Returns(lookupResult);
            _mockMapper.Map<LookupViewModel>(lookupResult).Returns(lookupViewModel);

            // Act
            var result = await _controller.Create(lookupId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CreateLookupItem", result.ViewName);
            var model = Assert.IsType<LookupItemViewModel>(result.Model);
            Assert.Equal(lookupId, model.LookupId);
            Assert.False(model.ShowErrorSummary);

            await _mockLookupService.Received(1).GetLookupByIdAsync(lookupId);
            _mockMapper.Received(1).Map<LookupViewModel>(lookupResult);
        }

        [Fact]
        public async Task Create_Get_InvalidInput_ReturnsBadRequest()
        {
            // Arrange
            var invalidLookupId = Guid.Empty;

            // Act
            var result = await _controller.Create(invalidLookupId) as BadRequestObjectResult;

            // Assert

            Assert.NotNull(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey(""));
            Assert.Equal("Invalid parameters.", ((string[])modelState[""])[0]);
        }

        [Fact]
        public async Task Create_Get_ValidInput_WithParent_ReturnsViewWithParentList()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var parentId = Guid.NewGuid();
            var lookupResult = new LookupDto { Id = lookupId, Name = "Test Lookup", Parent = parentId };
            var lookupViewModel = new LookupViewModel { Id = lookupId, Name = "Test Lookup", Parent = parentId };
            var parentItems = new[] { new LookupItemDto { Id = Guid.NewGuid(), Name = "Parent Item" } };
            var parentItemModels = new[] { new LookupItemModel { Id = Guid.NewGuid(), Name = "Parent Item" } };

            _mockLookupService.GetLookupByIdAsync(lookupId).Returns(lookupResult);
            _mockMapper.Map<LookupViewModel>(lookupResult).Returns(lookupViewModel);
            _mockLookupService.GetLookupItemParentListAsync(parentId).Returns(parentItems);
            _mockMapper.Map<IEnumerable<LookupItemModel>>(parentItems).Returns(parentItemModels);

            // Act
            var result = await _controller.Create(lookupId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CreateLookupItem", result.ViewName);
            var model = Assert.IsType<LookupItemViewModel>(result.Model);
            Assert.Equal(lookupId, model.LookupId);
            Assert.True(model.ShowParent);
            Assert.NotNull(model.LookupParentList);
            Assert.Single(model.LookupParentList);

            await _mockLookupService.Received(1).GetLookupByIdAsync(lookupId);
            await _mockLookupService.Received(1).GetLookupItemParentListAsync(parentId);
            _mockMapper.Received(1).Map<LookupViewModel>(lookupResult);
            _mockMapper.Received(1).Map<IEnumerable<LookupItemModel>>(parentItems);
        }

        [Fact]
        public async Task Create_Post_ModelStateInvalid_ReturnsViewWithErrors()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var model = new LookupItemViewModel { LookupId = lookupId, LookupItem = new LookupItemModel() };
            _controller.ModelState.AddModelError("", "Test error");

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
            // Act
            var result = await _controller.Create(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CreateLookupItem", result.ViewName);
            var returnedModel = Assert.IsType<LookupItemViewModel>(result.Model);
            Assert.Equal(lookupId, returnedModel.LookupId);
            Assert.False(returnedModel.ShowErrorSummary);
        }

        [Fact]
        public async Task Create_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel { Name = "Test Item" }
            };

            var dto = new LookupItemDto();
            _mockMapper.Map<LookupItemDto>(model.LookupItem).Returns(dto);

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

            // Act
            var result = await _controller.Create(model);

            // Assert
            await _mockLookupService.Received(1).InsertLookupItemAsync(model.LookupId, dto);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewResult()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel { Name = "" } // Invalid model
            };

            _controller.ModelState.AddModelError("LookkupItem.Name", "Name is required");

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
            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Equal("CreateLookupItem", viewResult?.ViewName);
        }

        [Fact]
        public async Task Create_Post_ModelStateValid_ValidatesModel()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel { Name = "Test Item" }
            };

            var Items = new[] { new LookupItemDto { Id = Guid.NewGuid(), Name = "Parent Item" } };

            _mockLookupService.GetAllLookupItemsAsync(model.LookupId).Returns(Items);
            _mockMapper.Map<IEnumerable<LookupItemModel>>(Arg.Any<List<LookupItemDto>>())
                .Returns(new List<LookupItemModel>());

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

            // Act
            await _controller.Create(model);

            // Assert
            await _mockLookupService.Received(1).GetAllLookupItemsAsync(model.LookupId);
        }

        [Fact]
        public async Task Create_Post_ModelValidationFails_ReturnsViewWithErrors()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel { Name = "Duplicate" },
                ShowParent = true
            };

            var existingItems = new List<LookupItemModel> { new LookupItemModel { Name = "Duplicate" } };
            _mockLookupService.GetAllLookupItemsAsync(model.LookupId).Returns(new List<LookupItemDto>());
            _mockMapper.Map<IEnumerable<LookupItemModel>>(Arg.Any<List<LookupItemDto>>()).Returns(existingItems);

            var lookup = new LookupViewModel { Id = model.LookupId, Parent = Guid.NewGuid() };
            var LookupDto = new LookupDto { Id = model.LookupId, Parent = Guid.NewGuid() };
            _mockLookupService.GetLookupByIdAsync(model.LookupId).Returns(LookupDto);
            _mockMapper.Map<LookupViewModel>(Arg.Any<object>()).Returns(lookup);

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
            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.Equal("CreateLookupItem", viewResult?.ViewName);
            Assert.True(_controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Create_Post_UserNotInValidRole_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel { Name = "Test Item" }
            };

            // Simulate a user with no roles
            var claimsIdentity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Create(model));
        }
    }
}
