using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicAssociationControllerTest
{
    public class RemoveCharacteristicTests
    {
        private readonly VirusCharacteristicAssociationController _controller;
        private readonly ILookupService _mockLookupService;
        private readonly IVirusCharacteristicService _mockCharacteristicService;
        private readonly IVirusCharacteristicAssociationService _mockTypeCharacteristicService;
        public RemoveCharacteristicTests()
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _mockCharacteristicService = Substitute.For<IVirusCharacteristicService>();
            _mockTypeCharacteristicService = Substitute.For<IVirusCharacteristicAssociationService>();
            _controller = new VirusCharacteristicAssociationController(
            _mockLookupService,
            _mockCharacteristicService,
            _mockTypeCharacteristicService);
        }

        [Fact]
        public async Task RemoveCharacteristic_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = await _controller.RemoveCharacteristic(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RemoveCharacteristic_ValidRequest_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();

            // Act
            await _controller.RemoveCharacteristic(typeId, characteristicId);

            // Assert
            await _mockTypeCharacteristicService.Received(1).RemoveCharacteristicFromTypeAsync(typeId, characteristicId);
        }

        [Fact]
        public async Task RemoveCharacteristic_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();

            // Act
            var result = await _controller.RemoveCharacteristic(typeId, characteristicId);

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}
