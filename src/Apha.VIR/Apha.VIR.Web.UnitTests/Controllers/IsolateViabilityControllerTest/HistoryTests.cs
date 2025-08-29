using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateViabilityControllerTest
{
    public class HistoryTests
    {
        private readonly IIsolateViabilityService _isolateViabilityService;
        private readonly IMapper _mapper;
        private readonly IsolateViabilityController _controller;
        private readonly ILookupService _lookupService;
        public HistoryTests()
        {
            _lookupService = Substitute.For<ILookupService>();
            _isolateViabilityService = Substitute.For<IIsolateViabilityService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new IsolateViabilityController(_isolateViabilityService, _lookupService, _mapper);
        }

        [Fact]
        public void History_WithValidInputs_ReturnsCorrectView()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateGuid = Guid.NewGuid();
            var serviceResult = new List<IsolateViabilityInfoDTO> { new IsolateViabilityInfoDTO { Nomenclature = "NULL/Congo Peafowl/Ascension Island/Ref 2/2025" } };
            var mappedResult = new List<IsolateViabilityModel> { new IsolateViabilityModel { Nomenclature = "NULL/Congo Peafowl/Ascension Island/Ref 2/2025" } };

            _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolateGuid).Returns(Task.FromResult((IEnumerable<IsolateViabilityInfoDTO>)serviceResult));
            _mapper.Map<IEnumerable<IsolateViabilityModel>>(serviceResult).Returns(mappedResult);

            // Act
            var result = _controller.History(avNumber, isolateGuid);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateViabilityHistoryViewModel>(viewResult.Model);

            Assert.Equal("ViabilityHistory", viewResult.ViewName);
            Assert.Equal("NULL/Congo Peafowl/Ascension Island/Ref 2/2025", model.Nomenclature);
            Assert.Single(model.ViabilityHistoryList);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void History_WithNullOrEmptyAVNumber_ReturnsNUllViewModel(string avNumber)
        {
            // Arrange
            var isolate = Guid.NewGuid();

            // Act
            var result = _controller.History(avNumber, isolate) as ViewResult;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void History_WithInvalidIsolateGuid_ReturnsNull()
        {
            // Arrange
            var avNumber = "AV001";
            var isolate = Guid.Empty;

            // Act
            var result = _controller.History(avNumber, isolate) as ViewResult;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void History_WhenServiceReturnsNullOrEmpty_ReturnsViewWithEmptyViewModel()
        {
            // Arrange
            var avNumber = "AV001";
            var isolate = Guid.NewGuid();

            _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolate).Returns(Task.FromResult<IEnumerable<IsolateViabilityInfoDTO>>(null!));

            // Act
            var result = _controller.History(avNumber, isolate) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ViabilityHistory", result.ViewName);
            var viewModel = Assert.IsType<IsolateViabilityHistoryViewModel>(result.Model);
            Assert.Null(viewModel.Nomenclature);
            Assert.Empty(viewModel.ViabilityHistoryList);
        }

        [Fact]
        public void History_WhenMapperReturnsNullOrEmpty_ThrowsArgumentExceptionl()
        {
            // Arrange
            var avNumber = "AV001";
            var isolate = Guid.NewGuid();
            var serviceResult = new List<IsolateViabilityInfoDTO> { new IsolateViabilityInfoDTO { Nomenclature = "Test" } };

            _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolate).Returns(Task.FromResult<IEnumerable<IsolateViabilityInfoDTO>>(serviceResult));

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(serviceResult).Returns((IEnumerable<IsolateViabilityModel>)null!);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _controller.History(avNumber, isolate));
        }

        [Fact]
        public void History_WhenServiceThrowsException_ReturnsErrorView()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateGuid = Guid.NewGuid();

            _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolateGuid)
                .Returns(Task.FromException<IEnumerable<IsolateViabilityInfoDTO>>(new Exception("Test exception")));

            // Act
            var exception = Assert.ThrowsAny<AggregateException>(() => _controller.History(avNumber, isolateGuid));

            // Assert
            Assert.Contains("Test exception", exception.InnerException?.Message);
        }
    }
}
