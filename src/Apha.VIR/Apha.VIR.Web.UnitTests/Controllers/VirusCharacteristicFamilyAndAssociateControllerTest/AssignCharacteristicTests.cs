using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicFamilyAndAssociateControllerTest
{
    public class AssignCharacteristicTests
    {
        private readonly IVirusTypeCharacteristicService _mockTypeCharacteristicService;
        private readonly VirusCharacteristicFamilyAndAssociateController _controller;
        public AssignCharacteristicTests() 
        {
            _mockTypeCharacteristicService = Substitute.For<IVirusTypeCharacteristicService>();
            var mockLookupService = Substitute.For<ILookupService>();
            var mockCharacteristicService = Substitute.For<IVirusCharacteristicService>();

            _controller = new VirusCharacteristicFamilyAndAssociateController(
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
