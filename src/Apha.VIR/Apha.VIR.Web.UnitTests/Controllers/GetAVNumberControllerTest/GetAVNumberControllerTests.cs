using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.GetAVNumberControllerTest
{
    public class GetAVNumberControllerTests
    {
        private readonly ISubmissionService _mockSubmissionService;
        private readonly GetAVNumberController _mockController;

        public GetAVNumberControllerTests()
        {
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockController = new GetAVNumberController(_mockSubmissionService);
        }

        [Fact]
        public async Task Index_Get_ReturnsViewWithModel()
        {
            // Arrange
            var avNumbers = new List<string> { "AV12345", "AV67890" };
            _mockSubmissionService.GetLatestSubmissionsAsync().Returns(avNumbers);

            // Act
            var result = await _mockController.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<GetAVNumberViewModel>(result.Model);
            var model = result.Model as GetAVNumberViewModel;
            Assert.Equal(avNumbers, model!.LastAVNumbers);
        }

        [Fact]
        public async Task Index_Post_ValidAVNumber_RedirectsToSubmissionSamples()
        {
            // Arrange
            var avNumber = "AV000000-01";
            _mockSubmissionService.AVNumberExistsInVirAsync(avNumber).Returns(true);

            // Act
            var result = await _mockController.Index(avNumber) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("SubmissionSamples", result.ControllerName);
            Assert.Equal(avNumber, result.RouteValues!["AVNumber"]);
        }

        [Fact]
        public async Task Index_Post_InvalidAVNumberFormat_ReturnsViewWithError()
        {
            // Arrange
            var invalidAVNumber = "InvalidAV";

            // Act
            var result = await _mockController.Index(invalidAVNumber) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(_mockController.ModelState.IsValid);
            Assert.True(_mockController.ModelState.ContainsKey("AVNumber"));
            Assert.Equal("Please check the format of this number.", _mockController.ModelState["AVNumber"]?.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task Index_Post_AVNumberNotExistingInVIR_RedirectsToSubmissionEdit()
        {
            // Arrange
            var avNumber = "AV000000-00";
            _mockSubmissionService.AVNumberExistsInVirAsync(avNumber).Returns(false);

            // Act
            var result = await _mockController.Index(avNumber) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Edit", result.ActionName);
            Assert.Equal("Submission", result.ControllerName);
            Assert.Equal(avNumber, result.RouteValues!["AVNumber"]);
        }

        [Fact]
        public async Task Index_Post_ValidAVNumberButNotExistingInVIR_RedirectsToSubmissionEdit()
        {
            // Arrange
            var avNumber = "AV000000-00";
            _mockSubmissionService.AVNumberExistsInVirAsync(Arg.Any<string>()).Returns(false);

            // Act
            var result = await _mockController.Index(avNumber) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Edit", result.ActionName);
            Assert.Equal("Submission", result.ControllerName);
            Assert.Equal(AVNumberUtil.AVNumberFormatted(avNumber), result.RouteValues!["AVNumber"]);
        }
    }
}
