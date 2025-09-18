using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicAssociationControllerTest
{
    public class GetVirusTypesTests
    {
        private readonly VirusCharacteristicAssociationController _controller;
        private readonly ILookupService _mockLookupService;
        public GetVirusTypesTests()
        {
            _mockLookupService = Substitute.For<ILookupService>();
            var mockCharacteristicService = Substitute.For<IVirusCharacteristicService>();
            var mockTypeCharacteristicService = Substitute.For<IVirusCharacteristicAssociationService>();
            _controller = new VirusCharacteristicAssociationController(
            _mockLookupService,
            mockCharacteristicService,
            mockTypeCharacteristicService);
        }
        [Fact]
        public async Task GetVirusTypes_ValidFamilyId_ReturnsJsonResult()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var expectedTypes = new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Type 1" },
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Type 2" }
            };
            _mockLookupService.GetAllVirusTypesByParentAsync(familyId).Returns(expectedTypes);

            // Act
            var result = await _controller.GetVirusTypes(familyId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var actualTypes = Assert.IsAssignableFrom<IEnumerable<LookupItemDto>>(jsonResult.Value);
            Assert.Equal(expectedTypes, actualTypes);
        }

        [Fact]
        public async Task GetVirusTypes_NoTypesFound_ReturnsEmptyJsonResult()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            _mockLookupService.GetAllVirusTypesByParentAsync(familyId).Returns(new List<LookupItemDto>());

            // Act
            var result = await _controller.GetVirusTypes(familyId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var actualTypes = Assert.IsAssignableFrom<IEnumerable<LookupItemDto>>(jsonResult.Value);
            Assert.Empty(actualTypes);
        }

        [Fact]
        public async Task GetVirusTypes_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("FamilyId", "Invalid Family ID");

            // Act
            var result = await _controller.GetVirusTypes(Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
