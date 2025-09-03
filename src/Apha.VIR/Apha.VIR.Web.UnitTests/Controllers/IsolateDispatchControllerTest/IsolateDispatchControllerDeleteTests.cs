using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateDispatchControllerTest
{
    public class IsolateDispatchControllerDeleteTests
    {
        private readonly IIsolateDispatchService _mockIsolateDispatchService;
        private readonly ILookupService _mockLookupService;
        private readonly IIsolatesService _mockIsolatesService;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IMapper _mockMapper;
        private readonly IsolateDispatchController _controller;

        public IsolateDispatchControllerDeleteTests()
        {
            _mockIsolateDispatchService = Substitute.For<IIsolateDispatchService>();
            _mockLookupService = Substitute.For<ILookupService>();
            _mockIsolatesService = Substitute.For<IIsolatesService>();
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _mockMapper = Substitute.For<IMapper>();

            _controller = new IsolateDispatchController(_mockIsolateDispatchService,
                _mockLookupService,
                _mockIsolatesService,
                _mockSubmissionService,
                _mockSampleService,
                _mockMapper);
        }

        [Fact]
        public async Task Delete_ValidInput_ReturnsRedirectToActionResult()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var isolateId = Guid.NewGuid();
            const string avNumber = "AV123";

            // Act
            var result = await _controller.Delete(dispatchId, lastModified, isolateId, avNumber);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("History", redirectResult.ActionName);
            Assert.Equal(avNumber, redirectResult!.RouteValues?["AVNumber"]);
            Assert.Equal(isolateId, redirectResult!.RouteValues?["IsolateId"]);

            await _mockIsolateDispatchService.Received(1).DeleteDispatchAsync(
            dispatchId,
            Arg.Is<byte[]>(b => Convert.ToBase64String(b) == lastModified),
            "Test User"
            );
        }

        [Fact]
        public async Task Delete_EmptyDispatchId_ReturnsBadRequest()
        {
            // Arrange
            var dispatchId = Guid.Empty;
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var isolateId = Guid.NewGuid();
            const string avNumber = "AV123";

            // Act
            var result = await _controller.Delete(dispatchId, lastModified, isolateId, avNumber);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Dispatch ID.", badRequestResult.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Delete_InvalidLastModified_ReturnsBadRequest(string lastModified)
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            const string avNumber = "AV123";

            // Act
            var result = await _controller.Delete(dispatchId, lastModified, isolateId, avNumber);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Last Modified cannot be empty.", badRequestResult.Value);
        }

        [Fact]
        public async Task Delete_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var isolateId = Guid.NewGuid();
            const string avNumber = "AV123";

            _controller.ModelState.AddModelError("Error", "Sample error");

            // Act
            var result = await _controller.Delete(dispatchId, lastModified, isolateId, avNumber);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Delete_ValidInput_CallsDeleteDispatchAsyncWithCorrectParameters()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            var isolateId = Guid.NewGuid();
            const string avNumber = "AV123";

            // Act
            await _controller.Delete(dispatchId, lastModified, isolateId, avNumber);

            // Assert
            await _mockIsolateDispatchService.Received(1).DeleteDispatchAsync(
            dispatchId,
            Arg.Is<byte[]>(b => Convert.ToBase64String(b) == lastModified),
            "Test User"
            );
        }

        [Theory]
        [InlineData("search")]
        [InlineData("SEARCH")]
        [InlineData("SeArCh")]
        public void CancelAction_WhenSourceIsSearch_ReturnsRedirectToSearchRepository(string source)
        {
            // Act
            var result = _controller.CancelAction(source);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Search", redirectResult.ActionName);
            Assert.Equal("SearchRepository", redirectResult.ControllerName);
        }

        [Theory]
        [InlineData("summary")]
        [InlineData("SUMMARY")]
        [InlineData("SuMmArY")]
        public void CancelAction_WhenSourceIsSummary_ReturnsRedirectToSummaryIndex(string source)
        {
            // Act
            var result = _controller.CancelAction(source);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Summary", redirectResult.ControllerName);
        }

        [Theory]
        [InlineData("other")]
        [InlineData("")]
        [InlineData(null)]
        public void CancelAction_WhenSourceIsOther_ReturnsRedirectToIsolateDispatchCreate(string source)
        {
            // Act
            var result = _controller.CancelAction(source);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirectResult.ActionName);
            Assert.Equal("IsolateDispatch", redirectResult.ControllerName);
        }
    }
}
