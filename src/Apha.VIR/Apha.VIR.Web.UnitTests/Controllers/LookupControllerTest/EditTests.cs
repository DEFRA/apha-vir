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
    public class EditTests
    {
        private readonly object _lock;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly LookupController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public EditTests(AppRolesFixture fixture)
        {
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new LookupController(_lookupService, _mapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task Edit_Get_ValidInputs_ReturnsViewWithViewModel()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var lookup = new LookupViewModel { Id = lookupId, Parent = Guid.NewGuid() };
            var lookupItem = new LookupItemModel { Id = lookupItemId };

            _lookupService.GetLookupByIdAsync(lookupId).Returns(new LookupDto());
            _lookupService.GetLookupItemAsync(lookupId, lookupItemId).Returns(new LookupItemDto());
            _mapper.Map<LookupViewModel>(Arg.Any<LookupDto>()).Returns(lookup);
            _mapper.Map<LookupItemModel>(Arg.Any<LookupItemDto>()).Returns(lookupItem);

            // Act
            var result = await _controller.Edit(lookupId, lookupItemId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EditLookupItem", result.ViewName);
            Assert.IsType<LookupItemViewModel>(result.Model);
        }

        [Fact]
        public async Task Edit_Get_InvalidLookupId_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Edit(Guid.Empty, Guid.NewGuid()) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey(""));
            Assert.Equal("Invalid parameters.", ((string[])modelState[""])[0]);
        }
       
        [Fact]
        public async Task Edit_Get_ShowParentTrue_PopulatesLookupParentListWithSelectListItems()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var parentId = Guid.NewGuid();

            var lookup = new LookupViewModel
            {
                Id = lookupId,
                Parent = parentId,
                AlternateName = false,
                Smsrelated = false,
                ReadOnly = false
            };

            var lookupItem = new LookupItemModel { Id = lookupItemId };

            // Mock parent items that will be returned by GetLookupItemPresents
            var parentItems = new List<LookupItemModel>
    {
        new LookupItemModel { Id = Guid.NewGuid(), Name = "Parent Item 1" },
        new LookupItemModel { Id = Guid.NewGuid(), Name = "Parent Item 2" }
    };

            _lookupService.GetLookupByIdAsync(lookupId).Returns(new LookupDto { Parent = parentId });
            _lookupService.GetLookupItemAsync(lookupId, lookupItemId).Returns(new LookupItemDto());
            _mapper.Map<LookupViewModel>(Arg.Any<LookupDto>()).Returns(lookup);
            _mapper.Map<LookupItemModel>(Arg.Any<LookupItemDto>()).Returns(lookupItem);

            // Mock the GetLookupItemPresents method call
            _lookupService.GetLookupItemParentListAsync(parentId).Returns(parentItems.Select(p => new LookupItemDto { Id = p.Id, Name = p.Name }).ToList());
            _mapper.Map<IEnumerable<LookupItemModel>>(Arg.Any<IEnumerable<LookupItemDto>>()).Returns(parentItems);

            // Act
            var result = await _controller.Edit(lookupId, lookupItemId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EditLookupItem", result.ViewName);
            var viewModel = Assert.IsType<LookupItemViewModel>(result.Model);

            // Verify the LookupParentList is populated with SelectListItems
            Assert.NotNull(viewModel.LookupParentList);
            Assert.Equal(2, viewModel.LookupParentList.Count);
            Assert.Equal(parentItems[0].Id.ToString(), viewModel.LookupParentList[0].Value);
            Assert.Equal("Parent Item 1", viewModel.LookupParentList[0].Text);
            Assert.Equal(parentItems[1].Id.ToString(), viewModel.LookupParentList[1].Value);
            Assert.Equal("Parent Item 2", viewModel.LookupParentList[1].Text);

            Assert.True(viewModel.ShowParent);
            Assert.Equal(lookupId, viewModel.LookupId);
            Assert.Equal(lookupItem, viewModel.LookupItem);
        }
        // ...existing code...

        [Fact]
        public async Task Edit_Get_InvalidLookupItemId_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Edit(Guid.NewGuid(), Guid.Empty) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey(""));
            Assert.Equal("Invalid parameters.", ((string[])modelState[""])[0]);
        }

        [Fact]
        public async Task Edit_Get_GetLookupsByIdAsyncThrowsException_ThrowsException()
        {
            // Arrange
            _lookupService.GetLookupByIdAsync(Arg.Any<Guid>()).Returns(Task.FromException<LookupDto>(new Exception("Service exception")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Edit(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task Edit_Get_GetLookupItemAsyncThrowsException_ThrowsException()
        {
            // Arrange
            _lookupService.GetLookupByIdAsync(Arg.Any<Guid>()).Returns(new LookupDto());
            _lookupService.GetLookupItemAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromException<LookupItemDto>(new Exception("Service exception")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Edit(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Edit_Get_DifferentCurrentPageValues_ReturnsViewWithCorrectViewModel(int currentPage)
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var lookup = new LookupViewModel { Id = lookupId };
            var lookupItem = new LookupItemModel { Id = lookupItemId };

            _lookupService.GetLookupByIdAsync(lookupId).Returns(new LookupDto());
            _lookupService.GetLookupItemAsync(lookupId, lookupItemId).Returns(new LookupItemDto());
            _mapper.Map<LookupViewModel>(Arg.Any<LookupDto>()).Returns(lookup);
            _mapper.Map<LookupItemModel>(Arg.Any<LookupItemDto>()).Returns(lookupItem);

            // Act
            var result = await _controller.Edit(lookupId, lookupItemId, currentPage) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EditLookupItem", result.ViewName);
            var viewModel = Assert.IsType<LookupItemViewModel>(result.Model);
            Assert.Equal(lookupId, viewModel.LookupId);
            Assert.Equal(lookupItem, viewModel.LookupItem);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
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

            var lookup = new LookupViewModel { Id = lookupId, Parent = Guid.NewGuid() };
            var lookupItem = new LookupItemModel { Id = lookupItemId };

            _lookupService.GetAllLookupItemsAsync(lookupId).Returns(lookupListdto);
            _mapper.Map<IEnumerable<LookupItemModel>>(lookupListdto).Returns(lookuitemList);


            var dto = new LookupItemDto();
            _mapper.Map<LookupItemDto>(model.LookupItem).Returns(dto);

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
            var result = await _controller.Edit(model);

            // Assert
            await _lookupService.Received(1).UpdateLookupItemAsync(model.LookupId, dto);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("LookupList", redirectResult.ActionName);
        }


        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            var model = new LookupItemViewModel { LookupItem = new LookupItemModel() };
            _controller.ModelState.AddModelError("Error", "Test error");

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
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditLookupItem", viewResult.ViewName);
        }

        [Fact]
        public async Task Edit_Post_ShowParentTrue_PopulatesLookupParentList()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                ShowParent = true,
                LookupItem = new LookupItemModel()
            };
            _controller.ModelState.AddModelError("Error", "Test error");

            var lookupResult = new LookupDto { Parent = Guid.NewGuid() };
            var lookupViewModel = new LookupViewModel { Parent = Guid.NewGuid() };
            _lookupService.GetLookupByIdAsync(model.LookupId).Returns(lookupResult);
            _mapper.Map<LookupViewModel>(lookupResult).Returns(lookupViewModel);

            var parentItems = new List<LookupItemDto>();
            _lookupService.GetLookupItemParentListAsync(Arg.Any<Guid>()).Returns(parentItems);
            _mapper.Map<IEnumerable<LookupItemModel>>(parentItems).Returns(new List<LookupItemModel>());

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
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultModel = Assert.IsType<LookupItemViewModel>(viewResult.Model);
            Assert.NotNull(resultModel.LookupParentList);
        }

        [Fact]
        public async Task Edit_Post_ValidateModelAddsErrors_ReturnsViewResult()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel()
            };

            _lookupService.GetAllLookupItemsAsync(model.LookupId).Returns(new List<LookupItemDto>());
            _mapper.Map<IEnumerable<LookupItemModel>>(Arg.Any<IEnumerable<LookupItemDto>>()).Returns(new List<LookupItemModel>());

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
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditLookupItem", viewResult.ViewName);
            Assert.True(_controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Edit_Post_UpdateLookupItemAsyncThrowsException_ReturnsViewResult()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel()
            };
            var dto = new LookupItemDto();
            _mapper.Map<LookupItemDto>(model.LookupItem).Returns(dto);
            _lookupService.UpdateLookupItemAsync(model.LookupId, dto).Returns(Task.FromException(new Exception("Test exception")));

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
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditLookupItem", viewResult.ViewName);
        }

      
        [Fact]
        public async Task Edit_Post_UserNotInValidRole_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var model = new LookupItemViewModel
            {
                LookupId = lookupId,
                LookupItem = new LookupItemModel
                {
                    Id = lookupItemId,
                    Name = "Test",
                }
            };

            // Mock the lookup service to return a lookup with a name
            var lookupDto = new LookupDto { Name = "TestLookup" };
            var lookupViewModel = new LookupViewModel { Name = "TestLookup" };
            _lookupService.GetLookupByIdAsync(lookupId).Returns(lookupDto);
            _mapper.Map<LookupViewModel>(lookupDto).Returns(lookupViewModel);

            // Simulate a user with no roles (or invalid roles)
            lock (_lock)
            {
                var claimsIdentity = new ClaimsIdentity();
                var user = new ClaimsPrincipal(claimsIdentity);
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Edit(model));
            Assert.Contains("Not authorised to update entry in TestLookup list.", exception.Message);

            // Verify the service was called to get the lookup for the error message
            await _lookupService.Received(1).GetLookupByIdAsync(lookupId);
            _mapper.Received(1).Map<LookupViewModel>(lookupDto);
        }
     
    }
}
