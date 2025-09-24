using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateDispatchControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class IsolateDispatchControllerCreateTests
    {
        private readonly object _lock;
        private readonly IIsolateDispatchService _mockIsolateDispatchService;        
        private readonly ILookupService _mockLookupService;
        private readonly IIsolatesService _mockIsolatesService;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mockMapper;
        private readonly IsolateDispatchController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public IsolateDispatchControllerCreateTests(AppRolesFixture fixture)
        {
            _mockIsolateDispatchService = Substitute.For<IIsolateDispatchService>();
            _mockLookupService = Substitute.For<ILookupService>();
            _mockIsolatesService = Substitute.For<IIsolatesService>();
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _cacheService = Substitute.For<ICacheService>();
            _mockMapper = Substitute.For<IMapper>();
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;

            _controller = new IsolateDispatchController(_mockIsolateDispatchService, 
                _mockLookupService, 
                _mockIsolatesService, 
                _mockSubmissionService, 
                _mockSampleService,
                _cacheService,
                _mockMapper);
        }

        [Fact]
        public async Task Create_ValidInput_ReturnsViewWithCorrectViewModel()
        {
            // Arrange
            string avNumber = "AV001";
            Guid isolateId = Guid.NewGuid();
            Guid viabilityId = Guid.NewGuid();
            string source = "search";

            var isolateInfo = new IsolateInfoDto
            {
                NoOfAliquots = 5,
                Nomenclature = "Test Nomenclature",
                ValidToIssue = true,
                MaterialTransferAgreement = true,
                IsMixedIsolate = false
            };

            var lastViability = new IsolateViabilityDto { Viable = viabilityId };

            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(avNumber, isolateId).Returns(isolateInfo);
            _mockIsolateDispatchService.GetLastViabilityByIsolateAsync(isolateId).Returns(lastViability);

            SetupLookupServices();

            // Act
            var result = await _controller.Create(avNumber, isolateId, source) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var viewModel = Assert.IsType<IsolateDispatchCreateViewModel>(result.Model);
            Assert.Equal(avNumber, viewModel.Avnumber);
            Assert.Equal(isolateId, viewModel.DispatchIsolateId);
            Assert.Equal(source, viewModel.Source);
            Assert.Equal(5, viewModel.NoOfAliquots);
            Assert.Equal("Test Nomenclature", viewModel.Nomenclature);
            Assert.True(viewModel.ValidToIssue);
            Assert.Equal(true, viewModel.MaterialTransferAgreement);
            Assert.Equal(viabilityId, viewModel.ViabilityId);
            Assert.False(viewModel.IsDispatchDisabled);
            Assert.Empty(viewModel.WarningMessages);
        }

        [Fact]
        public async Task Create_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            string avNumber = "AV001";
            Guid isolateId = Guid.NewGuid();
            Guid viabilityId = Guid.NewGuid();

            var isolateInfo = new IsolateInfoDto
            {
                NoOfAliquots = 1,
                Nomenclature = "Test Nomenclature",
                ValidToIssue = false,
                MaterialTransferAgreement = true,
                IsMixedIsolate = true
            };

            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(avNumber, isolateId).Returns(isolateInfo);

            // Act
            var result = await _controller.Create(avNumber, isolateId, "search") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<IsolateDispatchCreateViewModel>(result.Model);
        }

        [Fact]
        public async Task Create_NoAliquotsAvailable_SetsWarningMessageAndDisablesDispatch()
        {
            // Arrange
            string avNumber = "AV001";
            Guid isolateId = Guid.NewGuid();
            string source = "search";

            var isolateInfo = new IsolateInfoDto
            {
                NoOfAliquots = 1,
                ValidToIssue = true,
                IsMixedIsolate = false
            };

            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(avNumber, isolateId).Returns(isolateInfo);
            SetupLookupServices();

            // Act
            var result = await _controller.Create(avNumber, isolateId, source) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var viewModel = Assert.IsType<IsolateDispatchCreateViewModel>(result.Model);
            Assert.True(viewModel.IsDispatchDisabled);
            Assert.Contains(viewModel.WarningMessages, m => m.Contains("insufficient aliquots"));
        }

        [Theory]
        [InlineData("search", "IsolateDispatch", "Confirmation")]
        [InlineData("summary", "SubmissionSamples", "Index")]
        [InlineData("other", "IsolateDispatch", "Create")]
        public async Task Create_DifferentSources_RedirectsCorrectly(string source, string expectedController, string expectedAction)
        {
            // Arrange
            var dispatchModel = new IsolateDispatchCreateViewModel
            {
                Avnumber = "AV001",
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquots = 4,
                NoOfAliquotsToBeDispatched = 1,
                DispatchedDate = DateTime.Now.AddDays(-2),
                ValidToIssue = true,
                Source = source
            };

            var dispatchRecordDto = new IsolateDispatchInfoDto
            {
                Avnumber = "AV001",
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquots = 4,
                NoOfAliquotsToBeDispatched = 1,
                DispatchedDate = DateTime.Now.AddDays(-2)
            };

            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(new IsolateInfoDto());
            _mockIsolateDispatchService.AddDispatchAsync(Arg.Any<IsolateDispatchInfoDto>(), Arg.Any<string>()).Returns(Task.CompletedTask);
            _mockMapper.Map<IsolateDispatchInfoDto>(dispatchModel).Returns(dispatchRecordDto);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(dispatchModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAction, result.ActionName);
            Assert.Equal(expectedController, result.ControllerName);
        }

        private void SetupLookupServices()
        {
            _mockLookupService.GetAllViabilityAsync().Returns(new List<LookupItemDto>());
            _mockLookupService.GetAllWorkGroupsAsync().Returns(new List<LookupItemDto>());
            _mockLookupService.GetAllStaffAsync().Returns(new List<LookupItemDto>());
        }

        [Fact]
        public async Task Create_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var dispatchModel = new IsolateDispatchCreateViewModel
            {
                Avnumber = "AV123",
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquotsToBeDispatched = 1,
                NoOfAliquots = 2,
                ValidToIssue = true,
                DispatchedDate = DateTime.Now.AddDays(-1),
                Source = "search"
            };

            var isolateInfo = new IsolateInfoDto
            {
                NoOfAliquots = 2,
                ValidToIssue = true
            };

            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(Arg.Any<string>(), Arg.Any<Guid>())
            .Returns(isolateInfo);

            _mockIsolateDispatchService.GetLastViabilityByIsolateAsync(Arg.Any<Guid>())
            .Returns(new IsolateViabilityDto { Viable = Guid.NewGuid() });

            _mockMapper.Map<IsolateDispatchInfoDto>(Arg.Any<IsolateDispatchCreateViewModel>())
            .Returns(new IsolateDispatchInfoDto());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(dispatchModel);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Confirmation", redirectToActionResult.ActionName);
            Assert.Equal("IsolateDispatch", redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task Create_InvalidModel_ReturnsViewResult()
        {
            // Arrange
            var dispatchModel = new IsolateDispatchCreateViewModel
            {
                Avnumber = "AV123",
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquotsToBeDispatched = 3,
                NoOfAliquots = 2,
                ValidToIssue = false,
                DispatchedDate = DateTime.Now.AddDays(1)
            };

            _controller.ModelState.AddModelError("NoOfAliquotsToBeDispatched", "Invalid number of aliquots");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(dispatchModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<IsolateDispatchCreateViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Create_ValidModel_CallsAddDispatchAsync()
        {
            // Arrange
            var dispatchModel = new IsolateDispatchCreateViewModel
            {
                Avnumber = "AV123",
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquotsToBeDispatched = 1,
                NoOfAliquots = 2,
                ValidToIssue = true,
                DispatchedDate = DateTime.Now.AddDays(-1),
                Source = "search"
            };

            var isolateInfo = new IsolateInfoDto
            {
                NoOfAliquots = 2,
                ValidToIssue = true
            };

            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(Arg.Any<string>(), Arg.Any<Guid>())
            .Returns(isolateInfo);

            _mockIsolateDispatchService.GetLastViabilityByIsolateAsync(Arg.Any<Guid>())
            .Returns(new IsolateViabilityDto { Viable = Guid.NewGuid() });

            var mappedDispatchInfo = new IsolateDispatchInfoDto();
            _mockMapper.Map<IsolateDispatchInfoDto>(Arg.Any<IsolateDispatchCreateViewModel>())
            .Returns(mappedDispatchInfo);
            SetupMockUserAndRoles();
            // Act
            await _controller.Create(dispatchModel);

            // Assert
            await _mockIsolateDispatchService.Received(1).AddDispatchAsync(Arg.Is<IsolateDispatchInfoDto>(d => d == mappedDispatchInfo), Arg.Is<string>(u => u == "TestUser"));
        }

        [Theory]
        [InlineData("search", "Confirmation", "IsolateDispatch")]
        [InlineData("summary", "Index", "SubmissionSamples")]
        [InlineData("other", "Create", "IsolateDispatch")]
        public async Task Create_ValidModel_ReturnsCorrectRedirectBasedOnSource(string source, string expectedAction, string expectedController)
        {
            // Arrange
            var dispatchModel = new IsolateDispatchCreateViewModel
            {
                Avnumber = "AV123",
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquotsToBeDispatched = 1,
                NoOfAliquots = 2,
                ValidToIssue = true,
                DispatchedDate = DateTime.Now.AddDays(-1),
                Source = source
            };

            var isolateInfo = new IsolateInfoDto
            {
                NoOfAliquots = 2,
                ValidToIssue = true
            };

            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(Arg.Any<string>(), Arg.Any<Guid>())
            .Returns(isolateInfo);

            _mockIsolateDispatchService.GetLastViabilityByIsolateAsync(Arg.Any<Guid>())
            .Returns(new IsolateViabilityDto { Viable = Guid.NewGuid() });

            _mockMapper.Map<IsolateDispatchInfoDto>(Arg.Any<IsolateDispatchCreateViewModel>())
            .Returns(new IsolateDispatchInfoDto());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(dispatchModel);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(expectedAction, redirectToActionResult.ActionName);
            Assert.Equal(expectedController, redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task Create_NullIsolateInfo_PopulatesModelCorrectly()
        {
            // Arrange
            var dispatchModel = new IsolateDispatchCreateViewModel
            {
                Avnumber = "AV123",
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquots = 0,
                NoOfAliquotsToBeDispatched = 1,
                ValidToIssue = false,
                DispatchedDate = DateTime.Now.AddDays(-1)
            };

            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(Arg.Any<string>(), Arg.Any<Guid>())
            .Returns((IsolateInfoDto?)null!);

            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(dispatchModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchCreateViewModel>(viewResult.Model);
            Assert.Equal(0, model.NoOfAliquots);
            Assert.False(model.ValidToIssue);
            Assert.Null(model.ViabilityId);
        }

        [Fact]
        public async Task Create_Post_NoAliquots_AddsModelError()
        {
            // Arrange
            var dispatchModel = new IsolateDispatchCreateViewModel
            {
                Avnumber = "AV123",
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquots = null,
                NoOfAliquotsToBeDispatched = 1,
                ValidToIssue = true,
                DispatchedDate = DateTime.Now.AddDays(-1),
                Source = "search"
            };
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(dispatchModel);
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(_controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Create_CheckFieldsVisibility_SetsIsFieldInVisible_WhenSampleTypeIsRNA()
        {
            // Arrange
            string avNumber = "AV001";
            Guid isolateId = Guid.NewGuid();
            string source = "search";
            var isolateInfo = new IsolateInfoDto
            {
                NoOfAliquots = 2,
                ValidToIssue = true,
                IsMixedIsolate = false
            };
            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(avNumber, isolateId).Returns(isolateInfo);
            SetupLookupServices();

            // Setup for CheckFieldsVisibility to return false (SampleTypeName == "RNA")
            var submissionId = Guid.NewGuid();
            _mockSubmissionService.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(new SubmissionDto { SubmissionId = submissionId });
            _mockIsolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns(new IsolateDto { IsolateSampleId = Guid.NewGuid() });
            _mockSampleService.GetSamplesBySubmissionIdAsync(submissionId).Returns(new List<SampleDto> { new SampleDto { SampleId = Guid.NewGuid(), SampleTypeName = "RNA" } });

            // Act
            var result = await _controller.Create(avNumber, isolateId, source) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model); 
            var viewModel = Assert.IsType<IsolateDispatchCreateViewModel>(result.Model);          
            Assert.False(viewModel.IsFieldInVisible);
        }

        [Fact]
        public async Task Create_WarningMessagesAndFieldVisibility_AreSetCorrectly()
        {
            // Arrange
            string avNumber = "AV001";
            Guid isolateId = Guid.NewGuid();
            string source = "search";
            var isolateInfo = new IsolateInfoDto
            {
                NoOfAliquots = 1,
                ValidToIssue = false,
                IsMixedIsolate = true
            };
            _mockIsolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(avNumber, isolateId).Returns(isolateInfo);
            SetupLookupServices();

            // Make CheckFieldsVisibility return false
            _mockSubmissionService.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(new SubmissionDto { SubmissionId = Guid.NewGuid() });
            _mockIsolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns(new IsolateDto { IsolateSampleId = Guid.NewGuid() });
            _mockSampleService.GetSamplesBySubmissionIdAsync(Arg.Any<Guid>()).Returns(new List<SampleDto> { new SampleDto { SampleId = Guid.NewGuid(), SampleTypeName = "FTA Cards" } });

            // Act
            var result = await _controller.Create(avNumber, isolateId, source) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var viewModel = Assert.IsType<IsolateDispatchCreateViewModel>(result.Model);
            Assert.False(viewModel.IsFieldInVisible);
            Assert.Contains(viewModel.WarningMessages, m => m.Contains("insufficient aliquots"));
            Assert.Contains(viewModel.WarningMessages, m => m.Contains("not valid for issue"));
            Assert.Contains(viewModel.WarningMessages, m => m.Contains("mixed"));
        }

        [Fact]
        public async Task Create_Post_Unauthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dispatchModel = new IsolateDispatchCreateViewModel
            {
                Avnumber = "AV123",
                DispatchIsolateId = Guid.NewGuid(),
                NoOfAliquotsToBeDispatched = 1,
                NoOfAliquots = 2,
                ValidToIssue = true,
                DispatchedDate = DateTime.Now.AddDays(-1),
                Source = "search"
            };
            // Remove IsolateManager role
            AuthorisationUtil.AppRoles = new List<string> { AppRoleConstant.Administrator };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Create(dispatchModel));
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.Administrator),
                    new Claim(ClaimTypes.Role, AppRoleConstant.IsolateManager),
                    new Claim(ClaimTypes.Name, "TestUser")
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
