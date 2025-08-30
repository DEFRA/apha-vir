using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolatesControllerTest
{
    public class IsolatesControllerEditTests
    {
        private readonly IIsolatesService _mockIsolatesService;
        private readonly ILookupService _mockLookupService;
        private readonly IIsolateViabilityService _mockIsolateViabilityService;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IMapper _mockMapper;
        private readonly IsolatesController _controller;

        public IsolatesControllerEditTests()
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
        }

        [Fact]
        public async Task Edit_ValidInput_ReturnsViewResult()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var isolate = new IsolateDTO { IsolateId = isolateId, IsolateSampleId = sampleId };
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
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var isolate = new IsolateDTO { IsolateId = isolateId, IsolateSampleId = sampleId };
            var isolateModel = new IsolateAddEditViewModel { IsolateId = isolateId, IsolateSampleId = sampleId };
            _controller.ModelState.AddModelError("error", "some error");
            _mockIsolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns(isolate);      
            _mockMapper.Map<IsolateAddEditViewModel>(isolate).Returns(isolateModel);

            // Act
            var result = await _controller.Edit(avNumber, sampleId, isolateId);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
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

            _mockIsolatesService.UpdateIsolateDetailsAsync(Arg.Any<IsolateDTO>()).Returns(Task.CompletedTask);

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

            _mockIsolatesService.UpdateIsolateDetailsAsync(Arg.Any<IsolateDTO>()).Returns(Task.CompletedTask);
            _mockIsolateViabilityService.AddIsolateViabilityAsync(Arg.Any<IsolateViabilityInfoDTO>(), Arg.Any<string>()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(isolateModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            await _mockIsolateViabilityService.Received(1).AddIsolateViabilityAsync(Arg.Any<IsolateViabilityInfoDTO>(), Arg.Any<string>());
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

            _mockIsolatesService.UpdateIsolateDetailsAsync(Arg.Any<IsolateDTO>()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(isolateModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("IsolateCharacteristics", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_WithDefaultAction_RedirectsToSubmissionSamples()
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

            _mockIsolatesService.UpdateIsolateDetailsAsync(Arg.Any<IsolateDTO>()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(isolateModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("SubmissionSamples", redirectResult.ControllerName);
        }
    }
}
