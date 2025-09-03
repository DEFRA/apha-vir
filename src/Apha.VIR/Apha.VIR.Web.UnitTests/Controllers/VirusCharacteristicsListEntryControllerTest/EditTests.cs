using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.VirusCharacteristic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsControllerTest
{
    public class EditTests
    {
        private readonly IVirusCharacteristicService _service;
        private readonly IVirusCharacteristicListEntryService _listEntryService;
        private readonly IMapper _mapper;
        private readonly VirusCharacteristicsListEntryController _controller;

        public EditTests()
        {
            _service = Substitute.For<IVirusCharacteristicService>();
            _listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new VirusCharacteristicsListEntryController(_service, _listEntryService, _mapper);
        }

        [Fact]
        public async Task EditGet_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Edit(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task EditGet_EntryIsNull_ReturnsNotFound()
        {
            // Arrange
            _listEntryService.GetEntryByIdAsync(Arg.Any<Guid>()).Returns((VirusCharacteristicListEntryDTO?)null);

            // Act
            var result = await _controller.Edit(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditGet_EntryFound_ReturnsEditView()
        {
            // Arrange
            var dto = new VirusCharacteristicListEntryDTO();
            var vm = new VirusCharacteristicListEntryModel();
            _listEntryService.GetEntryByIdAsync(Arg.Any<Guid>()).Returns(dto);
            _mapper.Map<VirusCharacteristicListEntryModel>(dto).Returns(vm);

            // Act
            var result = await _controller.Edit(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditVirusCharacteristicEntry", viewResult.ViewName);
            Assert.Equal(vm, viewResult.Model);
        }
        [Fact]
        public async Task EditPost_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new VirusCharacteristicListEntryModel();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditVirusCharacteristicEntry", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task EditPost_ValidModel_UpdatesEntryAndRedirects()
        {
            // Arrange
            var model = new VirusCharacteristicListEntryModel { Id = Guid.NewGuid(), VirusCharacteristicId = Guid.NewGuid(), Name = "Test" };
            var dto = new VirusCharacteristicListEntryDTO();
            _mapper.Map<VirusCharacteristicListEntryDTO>(model).Returns(dto);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            await _listEntryService.Received(1).UpdateEntryAsync(dto);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ListEntries", redirect.ActionName);
            Assert.NotNull(redirect.RouteValues);
            Assert.Equal(model.VirusCharacteristicId, redirect.RouteValues["characteristic"]);
        }
    }
}
