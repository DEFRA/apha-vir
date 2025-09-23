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
    public class IsolatesControllerEditTests
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

        public IsolatesControllerEditTests(AppRolesFixture fixture)
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
        public async Task Edit_ValidInput_ReturnsViewResult()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var isolate = new IsolateDto { IsolateId = isolateId, IsolateSampleId = sampleId };
            var isolateModel = new IsolateAddEditViewModel();

            _mockIsolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns(isolate);
            _mockMapper.Map<IsolateAddEditViewModel>(isolate).Returns(isolateModel);

            // Act
            var result = await _controller.Edit(avNumber, sampleId, isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(isolateModel, viewResult.Model);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsViewResult()
        {
            // Arrange         
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Edit("AV00-01", Guid.NewGuid(), Guid.NewGuid());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);

            // Optionally verify that ModelState was passed back
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey("error"));
        }

        [Fact]
        public async Task Edit_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel
            {
                IsolateId = Guid.NewGuid(),
                AVNumber = "AV123",
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                YearOfIsolation = DateTime.Now.Year
            };

            _mockIsolatesService.UpdateIsolateDetailsAsync(Arg.Any<IsolateDto>()).Returns(Task.CompletedTask);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Edit(isolateModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("SubmissionSamples", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel();
            _controller.ModelState.AddModelError("error", "some error");
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Edit(isolateModel);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.IsType<IsolateAddEditViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Edit_WithViabilityInsert_AddsViability()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel
            {
                IsolateId = Guid.NewGuid(),
                AVNumber = "AV123",
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                YearOfIsolation = DateTime.Now.Year,
                IsViabilityInsert = true,
                Viable = Guid.NewGuid(),
                DateChecked = DateTime.Now,
                CheckedBy = Guid.NewGuid()
            };

            _mockIsolatesService.UpdateIsolateDetailsAsync(Arg.Any<IsolateDto>()).Returns(Task.CompletedTask);
            _mockIsolateViabilityService.AddIsolateViabilityAsync(Arg.Any<IsolateViabilityInfoDto>(), Arg.Any<string>()).Returns(Task.CompletedTask);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Edit(isolateModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            await _mockIsolateViabilityService.Received(1).AddIsolateViabilityAsync(Arg.Any<IsolateViabilityInfoDto>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Edit_WithSaveAndContinueAction_RedirectsToIsolateCharacteristics()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel
            {
                IsolateId = Guid.NewGuid(),
                AVNumber = "AV123",
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                YearOfIsolation = DateTime.Now.Year,
                ActionType = "SaveAndContinue"
            };

            _mockIsolatesService.UpdateIsolateDetailsAsync(Arg.Any<IsolateDto>()).Returns(Task.CompletedTask);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Edit(isolateModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Edit", redirectResult.ActionName);
            Assert.Equal("IsolateCharacteristics", redirectResult.ControllerName);
        }

        [Fact]
#pragma warning disable S4144 // Methods should not have identical implementations
        public async Task Edit_WithDefaultAction_RedirectsToSubmissionSamples()
#pragma warning restore S4144 // Methods should not have identical implementations
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel
            {
                IsolateId = Guid.NewGuid(),
                AVNumber = "AV123",
                Family = Guid.NewGuid(),
                Type = Guid.NewGuid(),
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                YearOfIsolation = DateTime.Now.Year
            };

            _mockIsolatesService.UpdateIsolateDetailsAsync(Arg.Any<IsolateDto>()).Returns(Task.CompletedTask);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Edit(isolateModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("SubmissionSamples", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_Post_Unauthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var isolateModel = new IsolateAddEditViewModel();
            AuthorisationUtil.CanEditItem(Arg.Any<string>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Edit(isolateModel));
        }

        [Theory]
        [InlineData("FTA Cards")]
        [InlineData("RNA")]
        public async Task Edit_Get_SetsIsChkDetection_ForSpecialSampleTypes(string sampleTypeName)
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var isolate = new IsolateDto { IsolateId = isolateId, IsolateSampleId = sampleId };
            var isolateModel = new IsolateAddEditViewModel();
            var submission = new SubmissionDto { DateSubmissionReceived = DateTime.Now, SubmissionId = Guid.NewGuid() };
            var samples = new List<SampleDto> { new SampleDto { SampleId = sampleId, SampleTypeName = sampleTypeName } };

            _mockIsolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns(isolate);
            _mockMapper.Map<IsolateAddEditViewModel>(isolate).Returns(isolateModel);
            _mockIsolateViabilityService.GetLastViabilityByIsolateAsync(isolateId).Returns((IsolateViabilityDto)null!);
            _mockSubmissionService.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleService.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(samples);

            // Act
            var result = await _controller.Edit(avNumber, sampleId, isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateAddEditViewModel>(viewResult.Model);
            Assert.True(model.IsChkDetection);
        }

        [Fact]
        public async Task Edit_Get_SetsPreviousViableFields_WhenViabilityExists()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var isolate = new IsolateDto { IsolateId = isolateId, IsolateSampleId = sampleId };
            var isolateModel = new IsolateAddEditViewModel();
            var viability = new IsolateViabilityDto
            {
                Viable = Guid.NewGuid(),
                DateChecked = DateTime.Now,
                CheckedById = Guid.NewGuid()
            };

            _mockIsolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns(isolate);
            _mockMapper.Map<IsolateAddEditViewModel>(isolate).Returns(isolateModel);
            _mockIsolateViabilityService.GetLastViabilityByIsolateAsync(isolateId).Returns(viability);

            // Act
            var result = await _controller.Edit(avNumber, sampleId, isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateAddEditViewModel>(viewResult.Model);
            Assert.Equal(viability.Viable, model.PreviousViable);
            Assert.Equal(viability.DateChecked, model.PreviousDateChecked);
            Assert.Equal(viability.CheckedById, model.PreviousCheckedBy);
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
