using System.Text.Json;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SubmissionSamplesControllerTest
{
    public class SampleDeleteTests
    {
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IIsolatesService _mockIsolatesService;
        private readonly IIsolateDispatchService _mockIsolatesDispatchService;
        private readonly IMapper _mockMapper;
        private readonly SubmissionSamplesController _controller;

        public SampleDeleteTests()
        {
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _mockIsolatesService = Substitute.For<IIsolatesService>();
            _mockIsolatesDispatchService = Substitute.For<IIsolateDispatchService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new SubmissionSamplesController(_mockSubmissionService, _mockSampleService, _mockIsolatesService, _mockIsolatesDispatchService, _mockMapper);
        }

        [Fact]
        public async Task SampleDelete_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.SampleDelete("AV123", Guid.NewGuid(), Array.Empty<byte>());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SampleDelete_IsolateExistsForSample_ReturnsJsonWithFailure()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();
            var sampleId = Guid.NewGuid();
            var isolates = new List<IsolateInfoDTO> { new IsolateInfoDTO { IsolateId = isolateId, IsolateSampleId = sampleId } };
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolates);

            // Act
            var result = await _controller.SampleDelete(avNumber, sampleId, Array.Empty<byte>());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.False(success);
            Assert.Equal("You cannot delete this Sample as it still has Isolates recorded against it.", message);
        }

        [Fact]
        public async Task SampleDelete_SuccessfulDeletion_ReturnsJsonWithSuccess()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var lastModified = Array.Empty<byte>();
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfoDTO>());
            _mockSampleService.DeleteSampleAsync(sampleId, "testUser", lastModified).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SampleDelete(avNumber, sampleId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.True(success);
            Assert.Equal("Sample deleted successfully.", message);
            await _mockSampleService.Received(1).DeleteSampleAsync(sampleId, "testUser", lastModified);
        }

        [Fact]
        public async Task SampleDelete_SampleDoesNotExist_ReturnsJsonWithSuccess()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var lastModified = Array.Empty<byte>();
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfoDTO>());
            _mockSampleService.DeleteSampleAsync(sampleId, "testUser", lastModified).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SampleDelete(avNumber, sampleId, lastModified);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.True(success);
            Assert.Equal("Sample deleted successfully.", message);
            await _mockSampleService.Received(1).DeleteSampleAsync(sampleId, "testUser", lastModified);
        }
    }
}
