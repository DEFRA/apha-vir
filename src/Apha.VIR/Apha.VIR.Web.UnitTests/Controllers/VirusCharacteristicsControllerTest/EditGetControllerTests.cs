using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.VirusCharacteristic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsControllerTest
{
    public class EditGetControllerTests
    {
        private readonly IVirusCharacteristicService _service;
        private readonly IVirusCharacteristicListEntryService _listEntryService;
        private readonly IMapper _mapper;
        private readonly VirusCharacteristicsController _controller;

        public EditGetControllerTests()
        {
            _service = Substitute.For<IVirusCharacteristicService>();
            _listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new VirusCharacteristicsController(_service, _listEntryService, _mapper);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Edit(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Edit_EntryIsNull_ReturnsAddView()
        {
            // Act
            var result = await _controller.Edit(Guid.NewGuid(), null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("VirusCharacteristicListEntryEdit", viewResult.ViewName);
            Assert.IsType<VirusCharacteristicListEntryEditViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Edit_EntryNotFound_ReturnsNotFound()
        {
            // Arrange
            _listEntryService.GetEntryByIdAsync(Arg.Any<Guid>()).Returns((VirusCharacteristicListEntryDTO?)null);

            // Act
            var result = await _controller.Edit(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_EntryFound_ReturnsEditView()
        {
            // Arrange
            var dto = new VirusCharacteristicListEntryDTO();
            var vm = new VirusCharacteristicListEntryEditViewModel();
            _listEntryService.GetEntryByIdAsync(Arg.Any<Guid>()).Returns(dto);
            _mapper.Map<VirusCharacteristicListEntryEditViewModel>(dto).Returns(vm);

            // Act
            var result = await _controller.Edit(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("VirusCharacteristicListEntryEdit", viewResult.ViewName);
            Assert.Equal(vm, viewResult.Model);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new VirusCharacteristicListEntryEditViewModel();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("VirusCharacteristicListEntryEdit", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Edit_AddEntry_RedirectsToListEntries()
        {
            // Arrange
            var model = new VirusCharacteristicListEntryEditViewModel { Id = Guid.Empty, VirusCharacteristicId = Guid.NewGuid(), Name = "Test" };
            var dto = new VirusCharacteristicListEntryDTO();
            _mapper.Map<VirusCharacteristicListEntryDTO>(model).Returns(dto);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            await _listEntryService.Received(1).AddEntryAsync(dto);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ListEntries", redirect.ActionName);
            Assert.Equal("VirusCharacteristics", redirect.ControllerName);
            Assert.NotNull(redirect.RouteValues); // Ensure RouteValues is not null
            Assert.Equal(model.VirusCharacteristicId, redirect.RouteValues["characteristic"]);
        }

        [Fact]
        public async Task Edit_UpdateEntry_RedirectsToListEntries()
        {
            // Arrange
            var model = new VirusCharacteristicListEntryEditViewModel { Id = Guid.NewGuid(), VirusCharacteristicId = Guid.NewGuid(), Name = "Test" };
            var dto = new VirusCharacteristicListEntryDTO();
            _mapper.Map<VirusCharacteristicListEntryDTO>(model).Returns(dto);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            await _listEntryService.Received(1).UpdateEntryAsync(dto);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ListEntries", redirect.ActionName);
            Assert.Equal("VirusCharacteristics", redirect.ControllerName);
            Assert.NotNull(redirect.RouteValues); // Ensure RouteValues is not null
            Assert.Equal(model.VirusCharacteristicId, redirect.RouteValues["characteristic"]);
        }
    }
}
