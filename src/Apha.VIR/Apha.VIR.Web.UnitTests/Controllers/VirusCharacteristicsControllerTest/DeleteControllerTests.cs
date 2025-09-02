using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsControllerTest
{
    public class DeleteControllerTests
    {
        private readonly IVirusCharacteristicService _service;
        private readonly IVirusCharacteristicListEntryService _listEntryService;
        private readonly IMapper _mapper;
        private readonly VirusCharacteristicsController _controller;

        public DeleteControllerTests()
        {
            _service = Substitute.For<IVirusCharacteristicService>();
            _listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new VirusCharacteristicsController(_service, _listEntryService, _mapper);
        }

        [Fact]
        public async Task Delete_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Delete(Guid.NewGuid(), Guid.NewGuid(), "dGVzdA==");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Delete_Valid_RedirectsToListEntries()
        {
            // Arrange
            var id = Guid.NewGuid();
            var characteristic = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });

            // Act
            var result = await _controller.Delete(id, characteristic, lastModified);

            // Assert
            await _listEntryService.Received(1).DeleteEntryAsync(id, Arg.Any<byte[]>());
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ListEntries", redirect.ActionName);
            Assert.Equal("VirusCharacteristics", redirect.ControllerName);

            Assert.NotNull(redirect.RouteValues);
            Assert.Equal(characteristic, redirect.RouteValues["characteristic"]);
        }
    }
}
