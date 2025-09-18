using System.Reflection;
using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateAndTrayRelocationControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class IsolateAndTrayRelocationControllerTests
    {
        private readonly object _lock;
        private readonly IIsolateRelocateService _isolateRelocateService;
        private readonly ILookupService _lookupService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly IsolateAndTrayRelocationController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public IsolateAndTrayRelocationControllerTests(AppRolesFixture fixture)
        {
            // Create the CacheService substitute with the mocked dependencies
            _cacheService = Substitute.For<ICacheService>();

            _isolateRelocateService = Substitute.For<IIsolateRelocateService>();
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new IsolateAndTrayRelocationController(_isolateRelocateService, _lookupService, _cacheService, _mapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task Test_Relocation_ReturnsIsolateRelocationView_WhenPathContainsIsolate()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/relocation/isolate";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Mock the services for this scenario            
            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(new List<IsolateRelocateDto>());
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>())
                .Returns(new List<IsolateRelocateViewModel>());

            // Act
            var result = await _controller.Relocation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("IsolateRelocation", viewResult.ViewName);  // Ensure correct view name is returned
        }

        [Fact]
        public async Task Test_Relocation_ReturnsTrayRelocationView_WhenPathContainsTray()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/relocation/tray";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Mock the services for this scenario            
            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(new List<IsolateRelocateDto>());
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>())
                .Returns(new List<IsolateRelocateViewModel>());

            // Act
            var result = await _controller.Relocation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("TrayRelocation", viewResult.ViewName);  // Ensure correct view name is returned
        }

        [Fact]
        public async Task Test_Relocation_ReturnsNotFound_WhenPathDoesNotContainIsolateOrTray()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/relocation/invalid";
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Mock the services for this scenario           
            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(new List<IsolateRelocateDto>());
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>())
                .Returns(new List<IsolateRelocateViewModel>());

            // Act
            var result = await _controller.Relocation();

            // Assert
            Assert.IsType<NotFoundResult>(result);  // Ensure NotFound result is returned
        }

        [Fact]
        public async Task Search_ValidInput_ReturnsPartialViewResult()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                MinAVNumber = "AV00-01",
                MaxAVNumber = "AV00-10",
                SelectedFreezer = Guid.NewGuid(),
                SelectedTray = Guid.NewGuid()
            };
            var dtoList = new List<IsolateRelocateDto>();
            var viewModelList = new List<IsolateRelocateViewModel>();

            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(dtoList);
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>()).Returns(viewModelList);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Search(model);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_SearchResults", partialViewResult.ViewName);
            Assert.IsType<List<IsolateRelocateViewModel>>(partialViewResult.Model);
        }

        [Fact]
        public async Task Search_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Model error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Search(new IsolateRelocationViewModel());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var serializableError = Assert.IsType<SerializableError>(badRequestResult.Value);

            Assert.True(serializableError.ContainsKey("Error"));
        }

        [Fact]
        public async Task Search_EmptyResultSet_ReturnsEmptyList()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                MinAVNumber = "AV000-01",
                MaxAVNumber = "AV000-10",
                SelectedFreezer = Guid.NewGuid(),
                SelectedTray = Guid.NewGuid()
            };
            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(new List<IsolateRelocateDto>());
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>()).Returns(new List<IsolateRelocateViewModel>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Search(model);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var resultList = Assert.IsType<List<IsolateRelocateViewModel>>(partialViewResult.Model);
            Assert.Empty(resultList);
        }

        [Fact]
        public async Task Search_EmptyStringsForAVNumbers_ReturnsPartialViewResult()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                MinAVNumber = "",
                MaxAVNumber = "",
                SelectedFreezer = Guid.NewGuid(),
                SelectedTray = Guid.NewGuid()
            };
            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(new List<IsolateRelocateDto>());
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>()).Returns(new List<IsolateRelocateViewModel>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Search(model);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task Search_EmptyGuidsForFreezerAndTray_ReturnsPartialViewResult()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                MinAVNumber = "AV00-01",
                MaxAVNumber = "AV00-10",
                SelectedFreezer = Guid.Empty,
                SelectedTray = Guid.Empty
            };
            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new List<IsolateRelocateDto>());
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>())
                .Returns(new List<IsolateRelocateViewModel>());
            
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Search(model);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task Save_SuccessfulOperation_ReturnsJsonResult()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                SelectedNewFreezer = Guid.NewGuid(),
                SelectedNewTray = Guid.NewGuid(),
                SelectedNewIsolatedList = new List<IsolatedRelocationData>
                {
                    new IsolatedRelocationData {
                        IsolatedId = Guid.NewGuid(),
                        Well = "A1",
                        LastModified = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
                    } //Need to convert to byte array
                }
            };

            _isolateRelocateService.UpdateIsolateFreezeAndTrayAsync(Arg.Any<IsolateRelocateDto>())
            .Returns(Task.CompletedTask);

            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Save(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value!;
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(value));
            Assert.True(dict.ContainsKey("success"));

            // Verify that the service method was called once
            await _isolateRelocateService.Received(1).UpdateIsolateFreezeAndTrayAsync(Arg.Any<IsolateRelocateDto>());
        }

        [Fact]
        public async Task Save_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var model = new IsolateRelocationViewModel();
            _controller.ModelState.AddModelError("error", "Some error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Save(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Save_EmptySelectedNewIsolatedList_ReturnsBadRequest()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                SelectedNewFreezer = Guid.NewGuid(),
                SelectedNewTray = Guid.NewGuid(),
                SelectedNewIsolatedList = new List<IsolatedRelocationData>()
            };
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Save(model);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");
            var model = new IsolateRelocateViewModel();

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Edit_ValidModelState_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var model = new IsolateRelocateViewModel();

            _lookupService.GetAllFreezerAsync().Returns(new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Freezer1" } });
            _lookupService.GetAllTraysAsync().Returns(new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Tray1" } });

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.NotNull(_controller.ViewBag.FreezersList);
            Assert.NotNull(_controller.ViewBag.TrayList);
        }


        [Fact]
        public async Task Update_ValidModelState_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new IsolateRelocateViewModel
            {
                IsolateId = Guid.NewGuid(),
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                Well = "A1",
                LastModified = BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            };

            _isolateRelocateService.UpdateIsolateFreezeAndTrayAsync(Arg.Any<IsolateRelocateDto>())
            .Returns(Task.CompletedTask);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Update(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Isolate", redirectResult.ActionName);
            Assert.Equal("Relocation", redirectResult.ControllerName);

            await _isolateRelocateService.Received(1).UpdateIsolateFreezeAndTrayAsync(Arg.Any<IsolateRelocateDto>());
        }

        [Fact]
        public async Task GetTraysByFreezerId_ValidFreezerId_ReturnsJsonResult()
        {
            // Arrange
            var freezerId = Guid.NewGuid();
            var trays = new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Tray 1" },
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Tray 2" }
            };
            _lookupService.GetAllTraysByParentAsync(freezerId).Returns(trays);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.GetTraysByFreezerId(freezerId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var trayList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Equal(2, trayList.Count);
            Assert.Equal("Tray 1", trayList[0].Text);
            Assert.Equal("Tray 2", trayList[1].Text);
        }

        [Fact]
        public async Task GetTraysByFreezerId_NullFreezerId_ExecutesWithoutException()
        {
            // Arrange
            Guid? freezerId = null;
            _lookupService.GetAllTraysByParentAsync(freezerId).Returns(new List<LookupItemDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.GetTraysByFreezerId(freezerId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var trayList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Empty(trayList);
        }

        [Fact]
        public async Task GetTraysByFreezerId_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "Some error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.GetTraysByFreezerId(Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SearchIsolates_WhenFreezerAndTraySelected_ReturnsPartialView()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                SelectedFreezer = Guid.NewGuid(),
                SelectedTray = Guid.NewGuid(),
                MinAVNumber = "AV1",
                MaxAVNumber = "AV2"
            };

            var serviceResult = new List<IsolateRelocateDto>();
            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(serviceResult);

            var mappedResult = new List<IsolateRelocateViewModel>();
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>()).Returns(mappedResult);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.SearchIsolates(model);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_SearchIsolates", partialViewResult.ViewName);
            Assert.Equal(mappedResult, partialViewResult.Model);
        }

        [Fact]
        public async Task SearchIsolates_WhenFreezerOrTrayNotSelected_AddModelError()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                SelectedFreezer = Guid.NewGuid(),
                SelectedTray = null,
                MinAVNumber = "AV00-01",
                MaxAVNumber = "AV00-02"
            };
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.SearchIsolates(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

            Assert.True(modelState.ContainsKey(string.Empty));
            Assert.Equal("You must select both a freezer and tray.", ((string[])modelState[string.Empty])[0]);
        }

        [Fact]
        public async Task SearchIsolates_WhenModelStateInvalid_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "test error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.SearchIsolates(new IsolateRelocationViewModel());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SearchIsolates_WhenEmptyResultFromService_ReturnsEmptyList()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                SelectedFreezer = Guid.NewGuid(),
                SelectedTray = Guid.NewGuid(),
                MinAVNumber = "AV1",
                MaxAVNumber = "AV2"
            };

            var emptyServiceResult = new List<IsolateRelocateDto>();
            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(emptyServiceResult);

            var emptyMappedResult = new List<IsolateRelocateViewModel>();
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>()).Returns(emptyMappedResult);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.SearchIsolates(model);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_SearchIsolates", partialViewResult.ViewName);
            Assert.Empty((List<IsolateRelocateViewModel>)partialViewResult.Model!);
        }


        [Fact]
        public async Task RelocateTray_ValidModel_ReturnsSuccessResult()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                SelectedNewFreezer = Guid.NewGuid(),
                SelectedTray = Guid.NewGuid(),
                MinAVNumber = "AV00-01",
                MaxAVNumber = "AV00-02"
            };

            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(new List<IsolateRelocateDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.RelocateTray(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value!;
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(value));
            Assert.True(dict.ContainsKey("success"));

            await _isolateRelocateService.Received(1).UpdateIsolateFreezeAndTrayAsync(Arg.Any<IsolateRelocateDto>());
        }

        [Fact]
        public async Task RelocateTray_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.RelocateTray(new IsolateRelocationViewModel());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RelocateTray_NullSelectedNewFreezer_ReturnsBadRequest()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                SelectedNewFreezer = null,
                SelectedTray = Guid.NewGuid()
            };
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.RelocateTray(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);

            Assert.True(modelState.ContainsKey(string.Empty));
            Assert.Equal("You must select a Freezer for the Tray to be relocated into.", ((string[])modelState[string.Empty])[0]);
        }

        [Fact]
        public async Task RelocateTray_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                SelectedNewFreezer = Guid.NewGuid(),
                SelectedTray = Guid.NewGuid(),
                MinAVNumber = "AV00-01",
                MaxAVNumber = "AV00-02"
            };

            _isolateRelocateService.UpdateIsolateFreezeAndTrayAsync(Arg.Any<IsolateRelocateDto>())
            .Returns(Task.FromException(new Exception("Service error")));
            SetupMockUserAndRoles();
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.RelocateTray(model));
        }

        [Fact]
        public async Task Relocation_WhenSessionHasJsonData_PopulatesModelFromCache()
        {
            // Arrange
            var cachedModel = new IsolateRelocateViewModel { Freezer = Guid.NewGuid(), Tray = Guid.NewGuid() };
            var jsonData = JsonConvert.SerializeObject(cachedModel);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.HttpContext.Session = new TestSession();
            _controller.HttpContext.Session.SetString("isolateRelocateSessionModel", jsonData);

            _isolateRelocateService.GetIsolatesByCriteria(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
                .Returns(new List<IsolateRelocateDto>());

            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDto>>())
                .Returns(new List<IsolateRelocateViewModel>());

            _controller.HttpContext.Request.Path = "/relocation/isolate";

            // Act
            var result = await _controller.Relocation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("IsolateRelocation", viewResult.ViewName);
        }

        [Fact]
        public async Task Edit_WhenSessionIsNull_ReturnsViewWithModelAndLists()
        {
            // Arrange
            var model = new IsolateRelocateViewModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.HttpContext.Session = new TestSession();

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.NotNull(_controller.ViewBag.FreezersList);
            Assert.NotNull(_controller.ViewBag.TrayList);
        }

        [Fact]
        public async Task Update_WhenCalled_LoadsFreezersAndTraysAndReturnsEditView()
        {
            // Arrange
            var model = new IsolateRelocateViewModel();
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Update(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Isolate", redirectResult.ActionName); 
            Assert.Equal("Relocation", redirectResult.ControllerName);
        }

        private static void InvokeValidateIsolatedFields(IsolateRelocationViewModel model, ModelStateDictionary modelState)
        {
            var method = typeof(IsolateAndTrayRelocationController)
                .GetMethod("ValidateIsolatedFields", BindingFlags.NonPublic | BindingFlags.Static);

            method!.Invoke(null, new object[] { model, modelState });
        }

        [Fact]
        public void ValidateIsolatedFields_WhenInvalidAvNumbers_AddsModelErrors()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                MinAVNumber = "invalid",
                MaxAVNumber = "invalid"
            };
            var modelState = new ModelStateDictionary();

            // Act
            InvokeValidateIsolatedFields(model, modelState);

            // Assert
            Assert.True(modelState.ErrorCount >= 2);
        }

        [Fact]
        public void ValidateIsolatedFields_WhenValidAvNumbers_NoModelErrors()
        {
            // Arrange
            var model = new IsolateRelocationViewModel
            {
                MinAVNumber = "AV00-01",
                MaxAVNumber = "AV00-02"
            };
            var modelState = new ModelStateDictionary();

            // Act
            InvokeValidateIsolatedFields(model, modelState);

            // Assert
            Assert.Empty(modelState);
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.IsolateManager),
                    new Claim(ClaimTypes.Name, "TestUser")
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.IsolateManager, AppRoleConstant.IsolateViewer, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }

    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public IEnumerable<string> Keys => _sessionStorage.Keys;
        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;

        public void Clear() => _sessionStorage.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _sessionStorage.Remove(key);

        public void Set(string key, byte[] value) => _sessionStorage[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _sessionStorage.TryGetValue(key, out value!);

        public void SetString(string key, string value) => Set(key, System.Text.Encoding.UTF8.GetBytes(value));
        public string? GetString(string key) => TryGetValue(key, out var value) ? System.Text.Encoding.UTF8.GetString(value) : null;
       
    }

}
