using System.Text;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateViabilityControllerTests
{
    public class IsolateViabilityControllerTest
    {
        private readonly IIsolateViabilityService _isolateViabilityService;
        private readonly IMapper _mapper;
        private readonly IsolateViabilityController _controller;
        private readonly ILookupService _lookupService;
        public IsolateViabilityControllerTest()
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
            //Assert.Equal("ViabilityHistory", result.ViewName);
            //var viewModel = Assert.IsType<IsolateViabilityHistoryViewModel>(result.Model);
            //Assert.Null(viewModel.Nomenclature);
            //Assert.Empty(viewModel.ViabilityHistoryList);
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

            _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolate).Returns(Task.FromResult<IEnumerable<IsolateViabilityInfoDTO>>(null));

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

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(serviceResult).Returns((IEnumerable<IsolateViabilityModel>)null);

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

        [Fact]
        public async Task Delete_ValidInput_ReturnsRedirectToActionResult()
        {
            // Arrange
            var isolateViabilityId = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            _isolateViabilityService.DeleteIsolateViabilityAsync(Arg.Any<Guid>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(isolateViabilityId, lastModified, avNumber, isolateId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(IsolateViabilityController.History), redirectResult.ActionName);
            Assert.Equal(avNumber, redirectResult.RouteValues["AVNumber"]);
            Assert.Equal(isolateId, redirectResult.RouteValues["Isolate"]);

            await _isolateViabilityService.Received(1).DeleteIsolateViabilityAsync(
            Arg.Is<Guid>(g => g == isolateViabilityId),
            Arg.Is<byte[]>(b => b.SequenceEqual(Convert.FromBase64String(lastModified))),
            Arg.Is<string>(s => s == "TestUser")
            );
        }

        [Fact]
        public async Task Delete_ServiceThrowsException_ThrowsException()
        {
            // Arrange
            var isolateViabilityId = Guid.NewGuid();
            var lastModified = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            _isolateViabilityService.DeleteIsolateViabilityAsync(Arg.Any<Guid>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns(Task.FromException(new Exception("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(isolateViabilityId, lastModified, avNumber, isolateId));
        }

        [Fact]
        public async Task Delete_InvalidLastModified_ThrowsFormatException()
        {
            // Arrange
            var isolateViabilityId = Guid.NewGuid();
            var lastModified = "invalid_base64";
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() => _controller.Delete(isolateViabilityId, lastModified, avNumber, isolateId));
        }

        [Fact]
        public async Task Delete_InvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.Delete(Guid.NewGuid(), "validBase64", "AV123", Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Delete_EmptyViabilityId_ShouldReturnBadRequest()
        {
            // Arrange
            var result = await _controller.Delete(Guid.Empty, "validBase64", "AV123", Guid.NewGuid());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid ViabilityId ID.", badRequestResult.Value);
        }

        [Fact]
        public async Task Delete_EmptyLastModified_ShouldReturnBadRequest()
        {
            // Arrange
            var result = await _controller.Delete(Guid.NewGuid(), "", "AV123", Guid.NewGuid());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Last Modified cannot be empty.", badRequestResult.Value);
        }
    }
}
