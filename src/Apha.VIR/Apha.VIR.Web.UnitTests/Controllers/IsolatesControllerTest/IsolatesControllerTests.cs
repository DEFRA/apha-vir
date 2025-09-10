using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolatesControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class IsolatesControllerTests
    {
        private readonly object _lock;
        private readonly IIsolatesService _mockIsolatesService;
        private readonly ILookupService _mockLookupService;
        private readonly IIsolateViabilityService _mockIsolateViabilityService;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IMapper _mockMapper;
        private readonly IsolatesController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public IsolatesControllerTests(AppRolesFixture fixture)
        {
            _mockIsolatesService = Substitute.For<IIsolatesService>();
            _mockLookupService = Substitute.For<ILookupService>();
            _mockIsolateViabilityService = Substitute.For<IIsolateViabilityService>();
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new IsolatesController(_mockIsolatesService,
                _mockLookupService,
                _mockIsolateViabilityService,
                _mockSubmissionService,
                _mockSampleService,
                _mockMapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task ViewIsolateDetails_InvalidModelState_ReturnsErrorView()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.ViewIsolateDetails(Guid.NewGuid());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
        }

        [Fact]
        public async Task ViewIsolateDetails_ValidModelState_ReturnsIsolateDetailsView()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var isolateDetails = new IsolateFullDetailDTO
            {
                IsolateDetails = new IsolateInfoDTO(),
                IsolateViabilityDetails = new List<IsolateViabilityInfoDTO>(),
                IsolateDispatchDetails = new List<IsolateDispatchInfoDTO>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfoDTO>()
            };
            var isolateViewModel = new IsolateDetailsViewModel
            {
                IsolateDetails = new IsolateDetails(),
                IsolateViabilityDetails = new List<IsolateViabilityCheckInfo>(),
                IsolateDispatchDetails = new List<IsolateDispatchInfo>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfo>()
            };

            _mockIsolatesService.GetIsolateFullDetailsAsync(isolateId).Returns(isolateDetails);
            _mockMapper.Map<IsolateDetailsViewModel>(isolateDetails).Returns(isolateViewModel);

            // Act
            var result = await _controller.ViewIsolateDetails(isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("IsolateDetails", viewResult.ViewName);
            Assert.Equal(isolateViewModel, viewResult.Model);

            await _mockIsolatesService.Received(1).GetIsolateFullDetailsAsync(isolateId);
            _mockMapper.Received(1).Map<IsolateDetailsViewModel>(isolateDetails);
        }

        [Fact]
        public async Task ViewIsolateDetails_ServiceThrowsException_ThrowsException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            _mockIsolatesService.GetIsolateFullDetailsAsync(isolateId).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.ViewIsolateDetails(isolateId));
        }

        [Fact]
        public async Task GetVirusTypesByVirusFamily_NullOrEmptyVirusFamilyId_ReturnsAllVirusTypes()
        {
            // Arrange
            var virusTypes = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type 1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type 2" }
            };
            _mockLookupService.GetAllVirusTypesAsync().Returns(virusTypes);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.GetVirusTypesByVirusFamily(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectList = Assert.IsType<List<CustomSelectListItem>>(jsonResult.Value);
            Assert.Equal(2, selectList.Count);
            Assert.Equal(virusTypes[0].Id.ToString(), selectList[0].Value);
            Assert.Equal(virusTypes[0].Name, selectList[0].Text);
            Assert.Equal(virusTypes[1].Id.ToString(), selectList[1].Value);
            Assert.Equal(virusTypes[1].Name, selectList[1].Text);
            await _mockLookupService.Received(1).GetAllVirusTypesAsync();
            await _mockLookupService.DidNotReceive().GetAllVirusTypesByParentAsync(Arg.Any<Guid>());
        }

        [Fact]
        public async Task GetVirusTypesByVirusFamily_ValidVirusFamilyId_ReturnsFilteredVirusTypes()
        {
            // Arrange
            Guid virusFamilyId = Guid.NewGuid();
            var virusTypes = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type 1" }
            };
            _mockLookupService.GetAllVirusTypesByParentAsync(virusFamilyId).Returns(virusTypes);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.GetVirusTypesByVirusFamily(virusFamilyId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectList = Assert.IsType<List<CustomSelectListItem>>(jsonResult.Value);
            Assert.Single(selectList);
            Assert.Equal(virusTypes[0].Id.ToString(), selectList[0].Value);
            Assert.Equal(virusTypes[0].Name, selectList[0].Text);
            await _mockLookupService.Received(1).GetAllVirusTypesByParentAsync(virusFamilyId);
            await _mockLookupService.DidNotReceive().GetAllVirusTypesAsync();
        }

        [Fact]
        public async Task GetTraysByFeezer_NullOrEmptyVirusFamilyId_ReturnsAllVirusTypes()
        {
            // Arrange
            var virusTypes = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type 1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type 2" }
            };
            _mockLookupService.GetAllTraysAsync().Returns(virusTypes);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.GetTraysByFeezer(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Equal(2, selectList.Count);
            Assert.Equal(virusTypes[0].Id.ToString(), selectList[0].Value);
            Assert.Equal(virusTypes[0].Name, selectList[0].Text);
            Assert.Equal(virusTypes[1].Id.ToString(), selectList[1].Value);
            Assert.Equal(virusTypes[1].Name, selectList[1].Text);
            await _mockLookupService.Received(1).GetAllTraysAsync();
            await _mockLookupService.DidNotReceive().GetAllTraysByParentAsync(Arg.Any<Guid>());
        }

        [Fact]
        public async Task GetTraysByFeezer_ValidVirusFamilyId_ReturnsFilteredVirusTypes()
        {
            // Arrange
            Guid virusFamilyId = Guid.NewGuid();
            var virusTypes = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type 1" }
            };
            _mockLookupService.GetAllTraysByParentAsync(virusFamilyId).Returns(virusTypes);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.GetTraysByFeezer(virusFamilyId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Single(selectList);
            Assert.Equal(virusTypes[0].Id.ToString(), selectList[0].Value);
            Assert.Equal(virusTypes[0].Name, selectList[0].Text);
            await _mockLookupService.Received(1).GetAllTraysByParentAsync(virusFamilyId);
            await _mockLookupService.DidNotReceive().GetAllTraysAsync();
        }

        [Fact]
        public async Task GenerateNomenclature_ValidInput_ReturnsNomenclature()
        {
            // Arrange
            string avNumber = "AV001";
            Guid sampleId = Guid.NewGuid();
            string virusType = "TestVirus";
            string yearOfIsolation = "2023";
            string expectedNomenclature = "TestVirus/Host/Country/RefNumber/2023";

            _mockIsolatesService.GenerateNomenclature(avNumber, sampleId, virusType, yearOfIsolation)
            .Returns(expectedNomenclature);

            // Act
            var result = await _controller.GenerateNomenclature(avNumber, sampleId, virusType, yearOfIsolation);

            // Assert
            Assert.Equal(expectedNomenclature, result);
        }

        [Fact]
        public async Task GenerateNomenclature_CallsServiceWithCorrectParameters()
        {
            // Arrange
            string avNumber = "AV001";
            Guid sampleId = Guid.NewGuid();
            string virusType = "TestVirus";
            string yearOfIsolation = "2023";

            // Act
            await _controller.GenerateNomenclature(avNumber, sampleId, virusType, yearOfIsolation);

            // Assert
            await _mockIsolatesService.Received(1).GenerateNomenclature(avNumber, sampleId, virusType, yearOfIsolation);
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.IsolateManager)
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.IsolateManager, AppRoleConstant.IsolateViewer, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
