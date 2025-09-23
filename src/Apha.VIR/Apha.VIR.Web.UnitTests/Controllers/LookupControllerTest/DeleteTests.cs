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
    public class DeleteTests
    {
        private readonly object _lock;
        private readonly LookupController _controller;
        private readonly ILookupService _mockLookupService;
        private readonly IMapper _mockMapper;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public DeleteTests(AppRolesFixture fixture)
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new LookupController(_mockLookupService, _mockMapper);
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

            // Mock the lookup service to return a lookup with a name for the exception message
            var lookupDto = new LookupDto { Name = "TestLookup" };
            var lookupViewModel = new LookupViewModel { Name = "TestLookup" };
            _mockLookupService.GetLookupByIdAsync(lookupId).Returns(lookupDto);
            _mockMapper.Map<LookupViewModel>(lookupDto).Returns(lookupViewModel);

            var lookupListdto = new List<LookupItemDto> { new LookupItemDto { Id = lookupItemId } };
            var lookuitemList = new List<LookupItemModel> { new LookupItemModel { Id = lookupItemId } };

            _mockLookupService.IsLookupItemInUseAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
            _mockMapper.Map<LookupItemDto>(Arg.Any<LookupItemModel>()).Returns(new LookupItemDto());

            _mockLookupService.GetAllLookupItemsAsync(lookupId).Returns(lookupListdto);
            _mockMapper.Map<IEnumerable<LookupItemModel>>(lookupListdto).Returns(lookuitemList);

            lock (_lock)
            {
                // Simulate a user with no roles
                var claimsIdentity = new ClaimsIdentity();
                var user = new ClaimsPrincipal(claimsIdentity);
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            //Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Delete(model));
            Assert.Contains("Not authorised to delete entry in TestLookup list.", exception.Message);

            // Verify the service was called to get the lookup for the error message
            await _mockLookupService.Received(1).GetLookupByIdAsync(lookupId);
            _mockMapper.Received(1).Map<LookupViewModel>(lookupDto);
        }


        // ...existing code...
        [Fact]
        public async Task Delete_InvalidModelStateWithShowParent_ReturnsViewWithLookupParentList()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var model = new LookupItemViewModel
            {
                LookupId = lookupId,
                LookupItem = new LookupItemModel(),
                ShowParent = true
            };

            

            // Mock the lookup service and mapper for the ShowParent scenario
            var lookupDto = new LookupDto { Name = "TestLookup", Parent = Guid.NewGuid() };
            var lookupViewModel = new LookupViewModel { Name = "TestLookup", Parent = lookupDto.Parent };
            var parentItems = new List<LookupItemModel>
    {
        new LookupItemModel { Id = Guid.NewGuid(), Name = "Parent Item 1" },
        new LookupItemModel { Id = Guid.NewGuid(), Name = "Parent Item 2" }
    };

            _mockLookupService.GetLookupByIdAsync(lookupId).Returns(lookupDto);
            _mockMapper.Map<LookupViewModel>(lookupDto).Returns(lookupViewModel);

            // Mock GetLookupItemPresents method (assuming it calls GetLookupItemParentListAsync)
            var parentItemDtos = parentItems.Select(p => new LookupItemDto { Id = p.Id, Name = p.Name }).ToList();
            _mockLookupService.GetLookupItemParentListAsync(lookupDto.Parent.Value).Returns(parentItemDtos);
            _mockMapper.Map<IEnumerable<LookupItemModel>>(parentItemDtos).Returns(parentItems);

            // Mock ValidateModel to add an error (this simulates validation failure)
            _mockLookupService.IsLookupItemInUseAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
            _mockMapper.Map<LookupItemDto>(Arg.Any<LookupItemModel>()).Returns(new LookupItemDto());

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
            Assert.Equal("EditLookupItem", viewResult.ViewName);

            var viewModel = Assert.IsType<LookupItemViewModel>(viewResult.Model);
            // ShowErrorSummary should be true because ModelState was initially valid, then ValidateModel was called
            Assert.True(viewModel.ShowErrorSummary);
            Assert.NotNull(viewModel.LookupParentList);
            Assert.Equal(2, viewModel.LookupParentList.Count);
            Assert.Equal(parentItems[0].Id.ToString(), viewModel.LookupParentList[0].Value);
            Assert.Equal("Parent Item 1", viewModel.LookupParentList[0].Text);
            Assert.Equal(parentItems[1].Id.ToString(), viewModel.LookupParentList[1].Value);
            Assert.Equal("Parent Item 2", viewModel.LookupParentList[1].Text);
        }
    
    }
}
