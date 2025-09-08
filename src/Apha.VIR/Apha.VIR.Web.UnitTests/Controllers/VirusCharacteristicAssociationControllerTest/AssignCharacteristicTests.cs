using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicAssociationControllerTest
{
    public class AssignCharacteristicTests
    {
        private readonly IVirusCharacteristicAssociationService _mockTypeCharacteristicService;
        private readonly VirusCharacteristicAssociationController _controller;
        public AssignCharacteristicTests()
        {
            _mockTypeCharacteristicService = Substitute.For<IVirusCharacteristicAssociationService>();
            var mockLookupService = Substitute.For<ILookupService>();
            var mockCharacteristicService = Substitute.For<IVirusCharacteristicService>();

            _controller = new VirusCharacteristicAssociationController(
            mockLookupService,
            mockCharacteristicService,
            _mockTypeCharacteristicService
            );
        }

        [Fact]
        public async Task AssignCharacteristic_ValidModelState_ReturnsOkResult()
        {
            // Arrange
            var typeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();

            // Act
            var result = await _controller.AssignCharacteristic(typeId, characteristicId);

            // Assert
            Assert.IsType<OkResult>(result);
            await _mockTypeCharacteristicService.Received(1).AssignCharacteristicToTypeAsync(typeId, characteristicId);
        }

        [Fact]
        public async Task AssignCharacteristic_InvalidModelState_ReturnsBadRequestResult()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.AssignCharacteristic(Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
