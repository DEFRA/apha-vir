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
        public async Task IsolateRelocation_ReturnsViewResultWithCorrectModel()
        {
            // Arrange
            var freezers = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Freezer 1" },
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Freezer 2" }
            };
            var trays = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Tray 1" },
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Tray 2" }
            };

            _lookupService.GetAllFreezerAsync().Returns(freezers);
            _lookupService.GetAllTraysAsync().Returns(trays);

            // Act
            var result = await _controller.IsolateRelocation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateRelocationViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.NotNull(model.SearchResults);
            Assert.Empty(model.SearchResults);           
            Assert.Equal(2, model.FreezersList!.Count);
            Assert.Equal(2, model.TraysList!.Count);
            Assert.Equal("Freezer 1", model.FreezersList[0].Text);
            Assert.Equal("Tray 1", model.TraysList[0].Text);

            await _lookupService.Received(1).GetAllFreezerAsync();
            await _lookupService.Received(1).GetAllTraysAsync();
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
            var dtoList = new List<IsolateRelocateDTO>();
            var viewModelList = new List<IsolateRelocateViewModel>();

            _isolateRelocateService.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(dtoList);
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDTO>>()).Returns(viewModelList);

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
            .Returns(new List<IsolateRelocateDTO>());
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDTO>>()).Returns(new List<IsolateRelocateViewModel>());

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
            .Returns(new List<IsolateRelocateDTO>());
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDTO>>()).Returns(new List<IsolateRelocateViewModel>());

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
                Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new List<IsolateRelocateDTO>());
            _mapper.Map<List<IsolateRelocateViewModel>>(Arg.Any<List<IsolateRelocateDTO>>())
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

            _isolateRelocateService.UpdateIsolateFreezeAndTrayAsync(Arg.Any<IsolateRelocateDTO>())
            .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Save(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic value = jsonResult.Value!;
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(value));
            Assert.True(dict.ContainsKey("success"));             

            // Verify that the service method was called once
            await _isolateRelocateService.Received(1).UpdateIsolateFreezeAndTrayAsync(Arg.Any<IsolateRelocateDTO>());
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
    }
}
