using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.Lookup;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.LookupControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class DeleteTests
    {
        private readonly object _lock;
        private readonly LookupController _controller;
        private readonly ILookupService _mockLookupService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mockMapper;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public DeleteTests(AppRolesFixture fixture)
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _cacheService = Substitute.For<ICacheService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new LookupController(_mockLookupService, 
                _cacheService,                
                _mockMapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }


        [Fact]
        public async Task Delete_ValidModel_ReturnsRedirectToActionResult()
        {
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = lookupId,
                LookupItem = new LookupItemModel
                {
                    Id = lookupItemId,
                    Name = "Test",

                }
            };

            var lookupListdto = new List<LookupItemDto> { new LookupItemDto { Id = lookupItemId } };
            var lookuitemList = new List<LookupItemModel> { new LookupItemModel { Id = lookupItemId } };

            _mockLookupService.IsLookupItemInUseAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
            _mockMapper.Map<LookupItemDto>(Arg.Any<LookupItemModel>()).Returns(new LookupItemDto());

            _mockLookupService.GetAllLookupItemsAsync(lookupId).Returns(lookupListdto);
            _mockMapper.Map<IEnumerable<LookupItemModel>>(lookupListdto).Returns(lookuitemList);

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
            var result = await _controller.Delete(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("LookupList", redirectResult.ActionName);
        }

        [Fact]
        public async Task Delete_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            var model = new LookupItemViewModel { LookupItem = new LookupItemModel() };
            _controller.ModelState.AddModelError("error", "some error");

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
            var result = await _controller.Delete(model);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Delete_ItemInUse_ReturnsViewResultWithError()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel { Id = Guid.NewGuid() }
            };
            _mockLookupService.IsLookupItemInUseAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

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
            var result = await _controller.Delete(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task Delete_ServiceThrowsException_ReturnsViewResultWithError()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel { Id = Guid.NewGuid() }
            };
            _mockLookupService.IsLookupItemInUseAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
            _mockMapper.Map<LookupItemDto>(Arg.Any<LookupItemModel>()).Returns(new LookupItemDto());
            _mockLookupService.DeleteLookupItemAsync(Arg.Any<Guid>(), Arg.Any<LookupItemDto>())
            .Returns(Task.FromException(new Exception("Test exception")));

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
            var result = await _controller.Delete(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task Delete_UserNotInValidRole_ThrowsUnauthorizedAccessException()
        {
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = lookupId,
                LookupItem = new LookupItemModel
                {
                    Id = lookupItemId,
                    Name = "Test",

                }
            };

            var lookupListdto = new List<LookupItemDto> { new LookupItemDto { Id = lookupItemId } };
            var lookuitemList = new List<LookupItemModel> { new LookupItemModel { Id = lookupItemId } };

            _mockLookupService.IsLookupItemInUseAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
            _mockMapper.Map<LookupItemDto>(Arg.Any<LookupItemModel>()).Returns(new LookupItemDto());

            _mockLookupService.GetAllLookupItemsAsync(lookupId).Returns(lookupListdto);
            _mockMapper.Map<IEnumerable<LookupItemModel>>(lookupListdto).Returns(lookuitemList);

            // Simulate a user with no roles
            var claimsIdentity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(claimsIdentity);
            _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

            //Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Delete(model));
        }
    }
}
