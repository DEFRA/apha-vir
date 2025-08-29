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
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsControllerTest
{
    public class ListEntriesControllerTests
    {
        private readonly IVirusCharacteristicService _service;
        private readonly IVirusCharacteristicListEntryService _listEntryService;
        private readonly IMapper _mapper;
        private readonly VirusCharacteristicsController _controller;

        public ListEntriesControllerTests()
        {
            _service = Substitute.For<IVirusCharacteristicService>();
            _listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new VirusCharacteristicsController(_service, _listEntryService, _mapper);
        }
        [Fact]
        public async Task ListEntries_Valid_ReturnsViewWithViewModel_AllPropertiesSet()
        {
            // Arrange
            var charId = Guid.NewGuid();
            var entryId = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3, 4 };
            var entryName = "Test Entry";

            var characteristic = new VirusCharacteristicDTO { Id = charId, Name = "Test" };
            var entryDto = new VirusCharacteristicListEntryDTO
            {
                Id = entryId,
                VirusCharacteristicId = charId,
                Name = entryName,
                LastModified = lastModified
            };
            var entryViewModel = new VirusCharacteristicListEntryViewModel
            {
                Id = entryId,
                VirusCharacteristicId = charId,
                Name = entryName,
                LastModified = lastModified
            };

            _service.GetAllVirusCharacteristicsAsync().Returns(new List<VirusCharacteristicDTO> { characteristic });
            _listEntryService.GetEntriesByCharacteristicIdAsync(charId).Returns(new List<VirusCharacteristicListEntryDTO> { entryDto });
            _mapper.Map<List<VirusCharacteristicListEntryViewModel>>(Arg.Any<List<VirusCharacteristicListEntryDTO>>())
                .Returns(new List<VirusCharacteristicListEntryViewModel> { entryViewModel });

            // Act
            var result = await _controller.ListEntries(charId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("VirusCharacteristicListEntries", viewResult.ViewName);
            var model = Assert.IsType<VirusCharacteristicListEntriesViewModel>(viewResult.Model);

            // Assert all properties are set in the view model
            var entry = Assert.Single(model.Entries);
            Assert.Equal(entryId, entry.Id);
            Assert.Equal(charId, entry.VirusCharacteristicId);
            Assert.Equal(entryName, entry.Name);
            Assert.Equal(lastModified, entry.LastModified);
        }

        [Fact]
        public async Task ListEntries_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.ListEntries(Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ListEntries_NullCharacteristic_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.ListEntries(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ListEntries_Valid_ReturnsViewWithViewModel()
        {
            // Arrange
            var charId = Guid.NewGuid();
            var characteristic = new VirusCharacteristicDTO { Id = charId, Name = "Test" };
            _service.GetAllVirusCharacteristicsAsync().Returns(new List<VirusCharacteristicDTO> { characteristic });
            _listEntryService.GetEntriesByCharacteristicIdAsync(charId).Returns(new List<VirusCharacteristicListEntryDTO>());
            _mapper.Map<List<VirusCharacteristicListEntryViewModel>>(Arg.Any<List<VirusCharacteristicListEntryDTO>>()).Returns(new List<VirusCharacteristicListEntryViewModel>());

            // Act
            var result = await _controller.ListEntries(charId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("VirusCharacteristicListEntries", viewResult.ViewName);
            Assert.IsType<VirusCharacteristicListEntriesViewModel>(viewResult.Model);
        }
    }
}
