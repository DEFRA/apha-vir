using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateViabilityControllerTest
{
    public class DeleteTests
    {
        private readonly IIsolateViabilityService _isolateViabilityService;
        private readonly IMapper _mapper;
        private readonly IsolateViabilityController _controller;
        private readonly ILookupService _lookupService;
        public DeleteTests()
        {
            _lookupService = Substitute.For<ILookupService>();
            _isolateViabilityService = Substitute.For<IIsolateViabilityService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new IsolateViabilityController(_isolateViabilityService, _lookupService, _mapper);
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

            Assert.NotNull(redirectResult);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.True(redirectResult.RouteValues.ContainsKey("AVNumber"));
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
