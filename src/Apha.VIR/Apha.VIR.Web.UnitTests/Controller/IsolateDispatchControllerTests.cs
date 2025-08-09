using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Controller
{
    public class IsolateDispatchControllerTests
    {
        private readonly IIsolateDispatchService _mockIsolateDispatchService;
        private readonly IMapper _mockMapper;
        private readonly IsolateDispatchController _controller;

        public IsolateDispatchControllerTests()
        {
            _mockIsolateDispatchService = Substitute.For<IIsolateDispatchService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new IsolateDispatchController(_mockIsolateDispatchService, _mockMapper);
        }

        [Fact]
        public async Task History_ReturnsViewModelWithData_WhenServiceReturnsValidData()
        {
            // Arrange
            var avNumber = "AV123456-01";
            var isolateId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var isolateDispatchInfoDTOs = new List<IsolateDispatchInfoDTO>
            {
                new IsolateDispatchInfoDTO { Nomenclature = "Test Nomenclature" }
            };
            var isolateDispatchHistories = new List<IsolateDispatchHistory>
            {
                new IsolateDispatchHistory
                {
                    DispatchIsolateId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    IsolateId = isolateId,
                    DispatchId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    NoOfAliquots = 5,
                    PassageNumber = 2,
                    Recipient = "recipient@test.com",
                    RecipientName = "Test Recipient",
                    RecipientAddress = "123 Test Street, Test City",
                    ReasonForDispatch = "Research",
                    DispatchedDate = new DateTime(2024, 1, 1),
                    DispatchedByName = "Test Dispatcher",
                    Avnumber = avNumber,
                    Nomenclature = "Test Nomenclature",
                    LastModified = new byte[] { 1, 2, 3, 4 }
                }
            };

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult((IEnumerable<IsolateDispatchInfoDTO>)isolateDispatchInfoDTOs));
            _mockMapper.Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDTO>>())
                .Returns(isolateDispatchHistories);

            // Act
            var result = await _controller.History(avNumber, isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchHistoryViewModel>(viewResult.Model);
            Assert.Equal("Test Nomenclature", model.Nomenclature);
            Assert.Equal(isolateDispatchHistories, model.DispatchHistoryRecords);
        }

        [Fact]
        public async Task History_ReturnsNullViewModel_WhenServiceReturnsEmptyList()
        {
            // Arrange
            var avNumber = "AV000000-01";
            var isolateId = Guid.Parse("2066E698-B746-493A-AB14-B30800CB75A8");

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult((IEnumerable<IsolateDispatchInfoDTO>)new List<IsolateDispatchInfoDTO>()));
            _mockMapper.Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDTO>>())
                .Returns(new List<IsolateDispatchHistory>());

            // Act
            var result = await _controller.History(avNumber, isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public async Task History_HandlesException_WhenServiceThrowsException()
        {
            // Arrange
            var avNumber = "AV000000-01";
            var isolateId = Guid.Parse("2066E698-B746-493A-AB14-B30800CB75A8");

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.History(avNumber, isolateId));
        }

        [Fact]
        public async Task History_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var avNumber = "AV000000-01";
            var isolateId = Guid.Parse("2066E698-B746-493A-AB14-B30800CB75A8");

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult((IEnumerable<IsolateDispatchInfoDTO>)new List<IsolateDispatchInfoDTO>()));

            // Act
            await _controller.History(avNumber, isolateId);

            // Assert
            await _mockIsolateDispatchService.Received(1).GetDispatchesHistoryAsync(
                Arg.Is<string>(s => s == avNumber),
                Arg.Is<Guid>(g => g == isolateId)
            );
        }

        [Fact]
        public async Task Confirmation_WithValidIsolateGuid_ReturnsViewResult()
        {
            // Arrange
            var isolateGuid = Guid.NewGuid();
            var dispatchConfirmationDTO = new IsolateFullDetailDTO
            {
                IsolateDetails = new IsolateInfoDTO { NoOfAliquots = 5 },
                IsolateDispatchDetails = new List<IsolateDispatchInfoDTO>(),
                IsolateViabilityDetails = new List<IsolateViabilityInfoDTO>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfoDTO>()

            };

            _mockIsolateDispatchService.GetDispatcheConfirmationAsync(isolateGuid)
            .Returns(Task.FromResult(dispatchConfirmationDTO));

            var mappedDispatchHistory = new List<IsolateDispatchHistory>();
            _mockMapper.Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDTO>>())
            .Returns(mappedDispatchHistory);

            // Act
            var result = await _controller.Confirmation(isolateGuid);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchConfirmatioViewModel>(viewResult.Model);
            Assert.Equal("Isolate dispatch completed successfully.", model.DispatchConfirmationMessage);
            Assert.Equal(5, model.RemainingAliquots);
            Assert.Same(mappedDispatchHistory, model.DispatchHistorys);

            await _mockIsolateDispatchService.Received(1).GetDispatcheConfirmationAsync(isolateGuid);
            _mockMapper.Received(1).Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDTO>>());
        }

        [Fact]
        public async Task Confirmation_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act
            var result = await _controller.Confirmation(emptyGuid);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Isolate ID.", badRequestResult.Value);
        }

        [Fact]
        public async Task Confirmation_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var isolateGuid = Guid.NewGuid();
            _controller.ModelState.AddModelError("Error", "Model error");

            // Act
            var result = await _controller.Confirmation(isolateGuid);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Isolate ID.", badRequestResult.Value);
        }
    }
}
