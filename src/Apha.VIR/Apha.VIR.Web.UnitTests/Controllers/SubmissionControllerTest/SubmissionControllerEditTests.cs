using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Controllers.SubmissionControllerTest
{
    public class SubmissionControllerEditTests
    {
        private readonly ILookupService _mockLookupService;
        private readonly ISenderService _mockSenderService;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IIsolatesService _mockIsolatedService;
        private readonly IMapper _mockMapper;
        private readonly SubmissionController _controller;

        public SubmissionControllerEditTests()
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _mockSenderService = Substitute.For<ISenderService>();
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _mockIsolatedService = Substitute.For<IIsolatesService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new SubmissionController(_mockLookupService, _mockSenderService, _mockSubmissionService, _mockMapper);
        }

        [Fact]
        public async Task Edit_AVNumberExistsInVIR_ReturnsViewWithModel()
        {
            // Arrange
            var avNumber = "TEST123";
            _mockSubmissionService.AVNumberExistsInVirAsync(avNumber).Returns(true);
            var submissionDto = new SubmissionDTO();
            _mockSubmissionService.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submissionDto);
            var submissionEditViewModel = new SubmissionEditViewModel();
            _mockMapper.Map<SubmissionEditViewModel>(submissionDto).Returns(submissionEditViewModel);

            var countryList = new List<SelectListItem>();
            var submittingLabList = new List<SelectListItem>();
            var submissionReasonList = new List<SelectListItem>();

            var ddldata = new List<LookupItemDTO>
            {
                new LookupItemDTO{ Id = Guid.NewGuid(), Name = "lookitem_1" },
                new LookupItemDTO{ Id = Guid.NewGuid(), Name = "lookitem_2" }
            };

            _mockLookupService.GetAllCountriesAsync().Returns(ddldata.AsEnumerable());
            _mockLookupService.GetAllSubmittingLabAsync().Returns(ddldata.AsEnumerable());
            _mockLookupService.GetAllSubmissionReasonAsync().Returns(ddldata.AsEnumerable());

            // Act
            var result = await _controller.Edit(avNumber);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubmissionEditViewModel>(viewResult.Model);
            Assert.Equal(submissionEditViewModel, model);
            Assert.NotNull(model.CountryList);
            Assert.NotNull(model.SubmittingLabList);
            Assert.NotNull(model.SubmissionReasonList);
        }

        [Fact]
        public async Task Edit_AVNumberDoesNotExistInVIR_RedirectsToCreate()
        {
            // Arrange
            var avNumber = "TEST123";
            _mockSubmissionService.AVNumberExistsInVirAsync(avNumber).Returns(false);

            // Act
            var result = await _controller.Edit(avNumber);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirectResult.ActionName);
            Assert.Equal(avNumber, redirectResult.RouteValues?["AVNumber"]);
        }

        [Fact]
        public async Task Edit_ExceptionThrown_ThrowsException()
        {
            // Arrange
            var avNumber = "TEST123";
            var expectedException = new Exception("Test exception");
            _mockSubmissionService.AVNumberExistsInVirAsync(avNumber).Throws(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Edit(avNumber));
        }

        [Fact]
        public async Task Edit_ValidSubmission_ShouldUpdateAndRedirect()
        {
            // Arrange
            var submission = new SubmissionEditViewModel();
            var submissionDto = new SubmissionDTO();
            _mockMapper.Map<SubmissionDTO>(submission).Returns(submissionDto);

            // Act
            var result = await _controller.Edit(submission) as RedirectToActionResult;

            // Assert
            await _mockSubmissionService.Received(1).UpdateSubmissionAsync(Arg.Any<SubmissionDTO>(), Arg.Any<string>());
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("SubmissionSamples", result.ControllerName);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ShouldReturnViewWithPopulatedLists()
        {
            // Arrange
            var submission = new SubmissionEditViewModel();
            _controller.ModelState.AddModelError("Error", "Model error");

            var ddldata = new List<LookupItemDTO>
            {
                new LookupItemDTO{ Id = Guid.NewGuid(), Name = "lookitem_1" },
                new LookupItemDTO{ Id = Guid.NewGuid(), Name = "lookitem_2" }
            };

            _mockLookupService.GetAllCountriesAsync().Returns(ddldata.AsEnumerable());
            _mockLookupService.GetAllSubmittingLabAsync().Returns(ddldata.AsEnumerable());
            _mockLookupService.GetAllSubmissionReasonAsync().Returns(ddldata.AsEnumerable());

            // Act
            var result = await _controller.Edit(submission) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<SubmissionEditViewModel>(result.Model);
            Assert.NotNull(model.CountryList);
            Assert.NotNull(model.SubmittingLabList);
            Assert.NotNull(model.SubmissionReasonList);
            Assert.Empty(model.Senders!);
            Assert.Empty(model.Organisations!);
        }

        [Fact]
        public async Task Edit_ExceptionThrown_ShouldHandleException()
        {
            // Arrange
            var submission = new SubmissionEditViewModel();
            _mockMapper.Map<SubmissionDTO>(submission).Returns(new SubmissionDTO());
            _mockSubmissionService.When(x => x.UpdateSubmissionAsync(Arg.Any<SubmissionDTO>(), Arg.Any<string>()))
            .Do(x => { throw new Exception("Test exception"); });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Edit(submission));
        }
    }
}
