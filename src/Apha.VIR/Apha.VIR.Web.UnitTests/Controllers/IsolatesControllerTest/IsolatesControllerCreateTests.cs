using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolatesControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class IsolatesControllerCreateTests
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

        public IsolatesControllerCreateTests(AppRolesFixture fixture)
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
        public async Task Test_Create_ValidInput_ReturnsViewWithModel()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            _mockSubmissionService.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(new SubmissionDto { DateSubmissionReceived = DateTime.Now });
            _mockSampleService.GetSamplesBySubmissionIdAsync(Arg.Any<Guid>()).Returns(new List<SampleDto> { new SampleDto { SampleId = sampleId, SampleTypeName = "Normal" } });

            // Act
            var result = await _controller.Create(avNumber, sampleId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<IsolateAddEditViewModel>(result.Model);
            var model = result.Model as IsolateAddEditViewModel;
            Assert.Equal(avNumber, model!.AVNumber);
            Assert.Equal(sampleId, model.IsolateSampleId);
        }

        [Fact]
        public async Task Test_Create_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Create("AV123", Guid.NewGuid());

            // Assert          
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);

            // Optionally verify that ModelState was passed back
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey("error"));
        }

        [Fact]
        public async Task Test_Create_SubmissionNotFound_ReturnsViewWithDefaultModel()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            _mockSubmissionService.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns((SubmissionDto)null!);

            // Act
            var result = await _controller.Create(avNumber, sampleId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<IsolateAddEditViewModel>(result.Model);
            var model = result.Model as IsolateAddEditViewModel;
            Assert.Equal(avNumber, model!.AVNumber);
            Assert.Equal(sampleId, model.IsolateSampleId);
            Assert.Null(model.YearOfIsolation);
        }

        [Fact]
        public async Task Create_SuccessfulCreation_RedirectsToIsolateCharacteristics()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel
            {
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                YearOfIsolation = DateTime.Now.Year,
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                ActionType = "SaveAndContinue",
                AVNumber = "AV123",
                IsolateId = Guid.NewGuid()
            };

            var IsolateDto = new IsolateDto
            {
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                YearOfIsolation = DateTime.Now.Year,
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                IsolateId = Guid.NewGuid()
            };

            _mockIsolatesService.AddIsolateDetailsAsync(Arg.Any<IsolateDto>()).Returns(IsolateDto.IsolateId);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(isolateModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Edit", result.ActionName);
            Assert.Equal("IsolateCharacteristics", result.ControllerName);
            Assert.Equal(isolateModel.AVNumber, result!.RouteValues!["AVNumber"]);
            Assert.Equal(isolateModel.IsolateId, result!.RouteValues["IsolateId"]);
        }

        [Fact]
        public async Task Create_ValidationFailure_ReturnsViewWithModel()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel();
            _controller.ModelState.AddModelError("Family", "Family is required");
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Create(isolateModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(isolateModel, result.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_WithViabilityInsert_CallsAddIsolateViability()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel
            {
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                YearOfIsolation = DateTime.Now.Year,
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                IsViabilityInsert = true,
                Viable = Guid.NewGuid(),
                DateChecked = DateTime.Now,
                CheckedBy = Guid.NewGuid(),
                ActionType = "SaveAndContinue",
                AVNumber = "AV123",
                IsolateId = Guid.NewGuid()
            };

            _mockIsolatesService.AddIsolateDetailsAsync(Arg.Any<IsolateDto>()).Returns((Guid)isolateModel.IsolateId);
            SetupMockUserAndRoles();
            // Act
            await _controller.Create(isolateModel);

            // Assert
            await _mockIsolateViabilityService.Received(1).AddIsolateViabilityAsync(Arg.Any<IsolateViabilityInfoDto>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Create_WithActionTypeSaveAndReturn_RedirectsToSubmissionSamples()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel
            {
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                YearOfIsolation = DateTime.Now.Year,
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                ActionType = "SaveAndReturn",
                AVNumber = "AV123",
                IsolateId = Guid.NewGuid()
            };

            _mockIsolatesService.AddIsolateDetailsAsync(Arg.Any<IsolateDto>()).Returns((Guid)isolateModel.IsolateId);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(isolateModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("SubmissionSamples", result.ControllerName);
            Assert.Equal(isolateModel.AVNumber, result!.RouteValues!["AVNumber"]);
        }

        [Fact]
        public async Task Create_WithInvalidYearOfIsolation_ReturnsViewWithError()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel
            {
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                YearOfIsolation = DateTime.Now.Year + 1,
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid()
            };
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(isolateModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage.Contains("Year of Isolation cannot be in the future")));
        }

        [Fact]
        public async Task Create_WithMissingRequiredFields_ReturnsViewWithErrors()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel();
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(isolateModel) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage.Contains("Virus Family")));
            Assert.Contains(_controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage.Contains("Virus Type")));
            Assert.Contains(_controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage.Contains("Freezer")));
            Assert.Contains(_controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage.Contains("Freezer Tray")));
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
