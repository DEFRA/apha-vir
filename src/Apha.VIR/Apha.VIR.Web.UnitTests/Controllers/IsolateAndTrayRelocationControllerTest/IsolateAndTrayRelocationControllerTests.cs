using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateAndTrayRelocationControllerTest
{
    public class IsolateAndTrayRelocationControllerTests
    {
        private readonly IIsolateRelocateService _isolateRelocateService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly IsolateAndTrayRelocationController _controller;

        public IsolateAndTrayRelocationControllerTests()
        {
            _isolateRelocateService = Substitute.For<IIsolateRelocateService>();
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new IsolateAndTrayRelocationController(_isolateRelocateService, _lookupService, _mapper);
        }

        [Fact]
        public async Task Relocation_WithIsolatePath_ReturnsIsolateRelocationView()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Path = "/relocation/isolate";

            // Act
            var result = await _controller.Relocation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("IsolateRelocation", viewResult.ViewName);
            await _lookupService.Received().GetAllFreezerAsync();
            await _lookupService.Received().GetAllTraysAsync();
        }

        [Fact]
        public async Task Relocation_WithTrayPath_ReturnsTrayRelocationView()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Path = "/relocation/tray";

            // Act
            var result = await _controller.Relocation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("TrayRelocation", viewResult.ViewName);
            await _lookupService.Received().GetAllFreezerAsync();
            await _lookupService.Received().GetAllTraysAsync();
        }

        [Fact]
        public async Task Relocation_WithInvalidPath_ReturnsNotFound()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };
            _controller.ControllerContext.HttpContext.Request.Path = "/relocation/invalid";

            // Act
            var result = await _controller.Relocation();

            // Assert
            Assert.IsType<NotFoundResult>(result);
            await _lookupService.Received().GetAllFreezerAsync();
            await _lookupService.Received().GetAllTraysAsync();
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

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.RelocateTray(model));
        }
    }
}
