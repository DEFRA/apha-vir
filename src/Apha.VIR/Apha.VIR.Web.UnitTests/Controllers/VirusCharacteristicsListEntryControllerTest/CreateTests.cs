using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.VirusCharacteristic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsControllerTest
{
    public class CreateTests
    {
        private readonly IVirusCharacteristicService _service;
        private readonly IVirusCharacteristicListEntryService _listEntryService;
        private readonly IMapper _mapper;
        private readonly VirusCharacteristicsListEntryController _controller;

        public CreateTests()
        {
            _service = Substitute.For<IVirusCharacteristicService>();
            _listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new VirusCharacteristicsListEntryController(_service, _listEntryService, _mapper);
        }
        [Fact]
        public void CreateGet_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var service = Substitute.For<IVirusCharacteristicService>();
            var listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            var mapper = Substitute.For<IMapper>();
            var controller = new VirusCharacteristicsListEntryController(service, listEntryService, mapper);
            controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = controller.Create(Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateGet_Valid_ReturnsCreateView()
        {
            // Arrange
            var service = Substitute.For<IVirusCharacteristicService>();
            var listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            var mapper = Substitute.For<IMapper>();
            var controller = new VirusCharacteristicsListEntryController(service, listEntryService, mapper);
            var characteristicId = Guid.NewGuid();

            // Act
            var result = controller.Create(characteristicId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("CreateVirusCharacteristicEntry", viewResult.ViewName);
            var model = Assert.IsType<VirusCharacteristicListEntryModel>(viewResult.Model);
            Assert.Equal(Guid.Empty, model.Id);
            Assert.Equal(characteristicId, model.VirusCharacteristicId);
        }

        [Fact]
        public async Task CreatePost_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new VirusCharacteristicListEntryModel();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("CreateVirusCharacteristicEntry", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task CreatePost_ValidModel_AddsEntryAndRedirects()
        {
            // Arrange
            var model = new VirusCharacteristicListEntryModel { Id = Guid.Empty, VirusCharacteristicId = Guid.NewGuid(), Name = "Test" };
            var dto = new VirusCharacteristicListEntryDTO();
            _mapper.Map<VirusCharacteristicListEntryDTO>(model).Returns(dto);

            // Act
            var result = await _controller.Create(model);

            // Assert
            await _listEntryService.Received(1).AddEntryAsync(dto);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ListEntries", redirect.ActionName);
            Assert.NotNull(redirect.RouteValues);
            Assert.Equal(model.VirusCharacteristicId, redirect.RouteValues["characteristic"]);
        }
    }
}
