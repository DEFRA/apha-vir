using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Controllers
{
    public class IsolatesControllerTests
    {
        private readonly IIsolatesService _mockIsolatesService;
        private readonly IMapper _mockMapper;
        private readonly IsolatesController _controller;

        public IsolatesControllerTests()
        {
            _mockIsolatesService = Substitute.For<IIsolatesService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new IsolatesController(_mockIsolatesService, _mockMapper);
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
    }
}
