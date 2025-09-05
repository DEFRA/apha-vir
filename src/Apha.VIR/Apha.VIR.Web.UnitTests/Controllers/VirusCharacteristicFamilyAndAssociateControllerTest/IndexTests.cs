using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.VirusCharacteristic;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicFamilyAndAssociateControllerTest
{
    public class IndexTests
    {
        private readonly VirusCharacteristicFamilyAndAssociateController _controller;
        private readonly ILookupService _lookupService;
        private readonly IVirusCharacteristicService _characteristicService;
        private readonly IVirusTypeCharacteristicService _typeCharacteristicService;
        public IndexTests()
        {
            _lookupService = Substitute.For<ILookupService>();
            _characteristicService = Substitute.For<IVirusCharacteristicService>();
            _typeCharacteristicService = Substitute.For<IVirusTypeCharacteristicService>();
            _controller = new VirusCharacteristicFamilyAndAssociateController(_lookupService, _characteristicService, _typeCharacteristicService);
        }

        [Fact]
        public async Task Index_ValidFamilyIdAndTypeId_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var typeId = Guid.NewGuid();
            var families = new List<LookupItemDTO> { new LookupItemDTO { Id = familyId, Name = "Family1" } };
            var types = new List<LookupItemDTO> { new LookupItemDTO { Id = typeId, Name = "Type1" } };
            var characteristics = new List<VirusCharacteristicDTO> { new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic1" } };

            _lookupService.GetAllVirusFamiliesAsync().Returns(families);
            _lookupService.GetAllVirusTypesByParentAsync(familyId).Returns(types);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, false).Returns(characteristics);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, true).Returns(new List<VirusCharacteristicDTO>());

            // Act
            var result = await _controller.Index(familyId, typeId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusFamilyAndTypeViewModel>(viewResult.Model);
            Assert.Equal(familyId, model.SelectedFamilyId);
            Assert.Equal(typeId, model.SelectedTypeId);
            Assert.Equal(families, model.VirusFamilies);
            Assert.Equal(types, model.VirusTypes);
            Assert.Equal(characteristics, model.CharacteristicsPresent);
            Assert.Empty(model.CharacteristicsAbsent);
        }

        [Fact]
        public async Task Index_ValidFamilyIdNullTypeId_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var typeId = Guid.NewGuid();
            var families = new List<LookupItemDTO> { new LookupItemDTO { Id = familyId, Name = "Family1" } };
            var types = new List<LookupItemDTO> { new LookupItemDTO { Id = typeId, Name = "Type1" } };
            var characteristics = new List<VirusCharacteristicDTO> { new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic1" } };

            _lookupService.GetAllVirusFamiliesAsync().Returns(families);
            _lookupService.GetAllVirusTypesByParentAsync(familyId).Returns(types);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, false).Returns(characteristics);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, true).Returns(new List<VirusCharacteristicDTO>());

            // Act
            var result = await _controller.Index(familyId, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusFamilyAndTypeViewModel>(viewResult.Model);
            Assert.Equal(familyId, model.SelectedFamilyId);
            Assert.Equal(typeId, model.SelectedTypeId);
            Assert.Equal(families, model.VirusFamilies);
            Assert.Equal(types, model.VirusTypes);
            Assert.Equal(characteristics, model.CharacteristicsPresent);
            Assert.Empty(model.CharacteristicsAbsent);
        }

        [Fact]
        public async Task Index_NullFamilyIdValidTypeId_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var typeId = Guid.NewGuid();
            var families = new List<LookupItemDTO> { new LookupItemDTO { Id = familyId, Name = "Family1" } };
            var types = new List<LookupItemDTO>();
            var characteristics = new List<VirusCharacteristicDTO>();

            _lookupService.GetAllVirusFamiliesAsync().Returns(families);
            _lookupService.GetAllVirusTypesByParentAsync(familyId).Returns(types);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, false).Returns(characteristics);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, true).Returns(new List<VirusCharacteristicDTO>());

            // Act
            var result = await _controller.Index(null, typeId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusFamilyAndTypeViewModel>(viewResult.Model);
            Assert.Equal(familyId, model.SelectedFamilyId);
            Assert.Equal(typeId, model.SelectedTypeId);
            Assert.Equal(families, model.VirusFamilies);
            Assert.Empty(model.VirusTypes);
            Assert.Empty(model.CharacteristicsPresent);
            Assert.Empty(model.CharacteristicsAbsent);
        }

        [Fact]
        public async Task Index_NullFamilyIdNullTypeId_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var families = new List<LookupItemDTO> { new LookupItemDTO { Id = familyId, Name = "Family1" } };
            var types = new List<LookupItemDTO>();
            var characteristics = new List<VirusCharacteristicDTO>();

            _lookupService.GetAllVirusFamiliesAsync().Returns(families);
            _lookupService.GetAllVirusTypesByParentAsync(familyId).Returns(types);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(Arg.Any<Guid?>(), false).Returns(characteristics);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(Arg.Any<Guid?>(), true).Returns(new List<VirusCharacteristicDTO>());

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusFamilyAndTypeViewModel>(viewResult.Model);
            Assert.Equal(familyId, model.SelectedFamilyId);
            Assert.Null(model.SelectedTypeId);
            Assert.Equal(families, model.VirusFamilies);
            Assert.Empty(model.VirusTypes);
            Assert.Empty(model.CharacteristicsPresent);
            Assert.Empty(model.CharacteristicsAbsent);
        }

        [Fact]
        public async Task Index_FamiliesIsEmpty_ReturnsViewWithNullSelectedFamilyIdAndEmptyTypes()
        {
            // Arrange
            var families = new List<LookupItemDTO>(); 
            _lookupService.GetAllVirusFamiliesAsync().Returns(families);

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusFamilyAndTypeViewModel>(viewResult.Model);

            Assert.Null(model.SelectedFamilyId); 
            Assert.Null(model.SelectedTypeId);  
            Assert.Empty(model.VirusFamilies);   
            Assert.Empty(model.VirusTypes);      
            Assert.Empty(model.CharacteristicsPresent); 
            Assert.Empty(model.CharacteristicsAbsent);  
        }


        [Fact]
        public async Task Index_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
