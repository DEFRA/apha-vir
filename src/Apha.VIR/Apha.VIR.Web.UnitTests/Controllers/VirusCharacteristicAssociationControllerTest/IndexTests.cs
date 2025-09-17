using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.VirusCharacteristic;
using Apha.VIR.Web.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicAssociationControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class IndexTests
    {
        private readonly object _lock;
        private readonly VirusCharacteristicAssociationController _controller;
        private readonly ILookupService _lookupService;
        private readonly IVirusCharacteristicService _characteristicService;
        private readonly IVirusCharacteristicAssociationService _typeCharacteristicService;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public IndexTests(AppRolesFixture fixture)
        {
            _lookupService = Substitute.For<ILookupService>();
            _characteristicService = Substitute.For<IVirusCharacteristicService>();
            _typeCharacteristicService = Substitute.For<IVirusCharacteristicAssociationService>();
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
            _controller = new VirusCharacteristicAssociationController(_lookupService, _characteristicService, _typeCharacteristicService);
        }

        [Fact]
        public async Task Index_ValidFamilyIdAndTypeId_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var typeId = Guid.NewGuid();
            var families = new List<LookupItemDto> { new LookupItemDto { Id = familyId, Name = "Family1" } };
            var virusTypes = new List<LookupItemDto> { new LookupItemDto { Id = typeId, Name = "Type1" } };
            var characteristics = new List<VirusCharacteristicDto> { new VirusCharacteristicDto { Id = Guid.NewGuid(), Name = "Characteristic1" } };

            _lookupService.GetAllVirusFamiliesAsync().Returns(families);
            _lookupService.GetAllVirusTypesByParentAsync(familyId).Returns(virusTypes);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, false).Returns(characteristics);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, true).Returns(new List<VirusCharacteristicDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Index(familyId, typeId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicAssociationViewModel>(viewResult.Model);
            Assert.Equal(familyId, model.SelectedFamilyId);
            Assert.Equal(typeId, model.SelectedVirusTypeId);
            Assert.Equal(families, model.VirusFamilies);
            Assert.Equal(virusTypes, model.VirusTypes);
            Assert.Equal(characteristics, model.CharacteristicsPresent);
            Assert.Empty(model.CharacteristicsAbsent);
        }

        [Fact]
        public async Task Index_ValidFamilyIdNullTypeId_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var typeId = Guid.NewGuid();
            var families = new List<LookupItemDto> { new LookupItemDto { Id = familyId, Name = "Family1" } };
            var virusTypes = new List<LookupItemDto> { new LookupItemDto { Id = typeId, Name = "Type1" } };
            var characteristics = new List<VirusCharacteristicDto> { new VirusCharacteristicDto { Id = Guid.NewGuid(), Name = "Characteristic1" } };

            _lookupService.GetAllVirusFamiliesAsync().Returns(families);
            _lookupService.GetAllVirusTypesByParentAsync(familyId).Returns(virusTypes);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, false).Returns(characteristics);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, true).Returns(new List<VirusCharacteristicDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Index(familyId, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicAssociationViewModel>(viewResult.Model);
            Assert.Equal(familyId, model.SelectedFamilyId);
            Assert.Equal(typeId, model.SelectedVirusTypeId);
            Assert.Equal(families, model.VirusFamilies);
            Assert.Equal(virusTypes, model.VirusTypes);
            Assert.Equal(characteristics, model.CharacteristicsPresent);
            Assert.Empty(model.CharacteristicsAbsent);
        }

        [Fact]
        public async Task Index_NullFamilyIdValidTypeId_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var familyId = Guid.NewGuid();
            var typeId = Guid.NewGuid();
            var families = new List<LookupItemDto> { new LookupItemDto { Id = familyId, Name = "Family1" } };
            var virusTypes = new List<LookupItemDto>();
            var characteristics = new List<VirusCharacteristicDto>();

            _lookupService.GetAllVirusFamiliesAsync().Returns(families);
            _lookupService.GetAllVirusTypesByParentAsync(familyId).Returns(virusTypes);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, false).Returns(characteristics);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(typeId, true).Returns(new List<VirusCharacteristicDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Index(null, typeId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicAssociationViewModel>(viewResult.Model);
            Assert.Equal(familyId, model.SelectedFamilyId);
            Assert.Equal(typeId, model.SelectedVirusTypeId);
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
            var families = new List<LookupItemDto> { new LookupItemDto { Id = familyId, Name = "Family1" } };
            var virusTypes = new List<LookupItemDto>();
            var characteristics = new List<VirusCharacteristicDto>();

            _lookupService.GetAllVirusFamiliesAsync().Returns(families);
            _lookupService.GetAllVirusTypesByParentAsync(familyId).Returns(virusTypes);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(Arg.Any<Guid?>(), false).Returns(characteristics);
            _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(Arg.Any<Guid?>(), true).Returns(new List<VirusCharacteristicDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicAssociationViewModel>(viewResult.Model);
            Assert.Equal(familyId, model.SelectedFamilyId);
            Assert.Null(model.SelectedVirusTypeId);
            Assert.Equal(families, model.VirusFamilies);
            Assert.Empty(model.VirusTypes);
            Assert.Empty(model.CharacteristicsPresent);
            Assert.Empty(model.CharacteristicsAbsent);
        }

        [Fact]
        public async Task Index_FamiliesIsEmpty_ReturnsViewWithNullSelectedFamilyIdAndEmptyTypes()
        {
            // Arrange
            var families = new List<LookupItemDto>();
            _lookupService.GetAllVirusFamiliesAsync().Returns(families);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicAssociationViewModel>(viewResult.Model);

            Assert.Null(model.SelectedFamilyId);
            Assert.Null(model.SelectedVirusTypeId);
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
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Index(null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.LookupDataManager)
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
