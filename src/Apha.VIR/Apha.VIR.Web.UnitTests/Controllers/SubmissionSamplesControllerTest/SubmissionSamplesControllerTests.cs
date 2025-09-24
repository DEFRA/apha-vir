using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SubmissionSamplesControllerTest
{
    public class SubmissionSamplesControllerTests
    {
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IIsolatesService _mockIsolatesService;
        private readonly IIsolateDispatchService _mockIsolatesDispatchService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mockMapper;
        private readonly SubmissionSamplesController _controller;

        public SubmissionSamplesControllerTests()
        {
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _mockIsolatesService = Substitute.For<IIsolatesService>();
            _mockIsolatesDispatchService = Substitute.For<IIsolateDispatchService>();
            _cacheService = Substitute.For<ICacheService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new SubmissionSamplesController(_mockSubmissionService, 
                _mockSampleService, 
                _mockIsolatesService,
                _mockIsolatesDispatchService, 
                _cacheService,
                _mockMapper);
        }

        [Fact]
        public async Task Index_AVNumberExistsInVIR_ReturnsViewWithViewModel()
        {
            // Arrange
            string avNumber = "AV123";
            var submission = new SubmissionDto { SubmissionId = Guid.NewGuid(), Avnumber = avNumber };
            var samples = new List<SampleDto>();
            var isolates = new List<IsolateInfoDto>();
            var sampleModels = new List<SubmissionSamplesModel>();
            var isolateModels = new List<SubmissionIsolatesModel>();

            _mockSubmissionService.AVNumberExistsInVirAsync(avNumber).Returns(true);
            _mockSubmissionService.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleService.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(samples);
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolates);
            _mockMapper.Map<List<SubmissionSamplesModel>>(samples).Returns(sampleModels);
            _mockMapper.Map<List<SubmissionIsolatesModel>>(isolates).Returns(isolateModels);

            // Act
            var result = await _controller.Index(avNumber);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SubmissionSamplesViewModel>(viewResult.Model);
            Assert.Equal(avNumber, model.AVNumber);
        }

        [Fact]
        public async Task Index_AVNumberDoesNotExistInVIR_RedirectsToHome()
        {
            // Arrange
            string avNumber = "AV000000-00";
            _mockSubmissionService.AVNumberExistsInVirAsync(avNumber).Returns(false);

            // Act
            var result = await _controller.Index(avNumber);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Index_NoSamplesOrIsolates_ReturnsViewModelWithLetterRequired()
        {
            // Arrange
            string avNumber = "AV789";
            var submission = new SubmissionDto { SubmissionId = Guid.NewGuid(), Avnumber = avNumber };
            var samples = new List<SampleDto>();
            var isolates = new List<IsolateInfoDto>();

            _mockSubmissionService.AVNumberExistsInVirAsync(avNumber).Returns(true);
            _mockSubmissionService.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleService.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(samples);
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolates);
            _mockMapper.Map<List<SubmissionSamplesModel>>(samples).Returns(new List<SubmissionSamplesModel>());
            _mockMapper.Map<List<SubmissionIsolatesModel>>(isolates).Returns(new List<SubmissionIsolatesModel>());

            // Act
            var result = await _controller.Index(avNumber);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SubmissionSamplesViewModel>(viewResult.Model);
            Assert.True(model.IsLetterRequired);
        }

        [Fact]
        public async Task Index_WithIsolatesAndDetections_ReturnsViewModelWithCorrectHeader()
        {
            // Arrange
            string avNumber = "AV101";
            var submission = new SubmissionDto { SubmissionId = Guid.NewGuid(), Avnumber = avNumber };
            var samples = new List<SampleDto>
            {
                new SampleDto { SampleTypeName = "FTA Cards" },
                new SampleDto { SampleTypeName = "Other" }
            };
            var isolates = new List<IsolateInfoDto> { new IsolateInfoDto() };

            _mockSubmissionService.AVNumberExistsInVirAsync(avNumber).Returns(true);
            _mockSubmissionService.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleService.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(samples);
            _mockIsolatesService.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolates);
            _mockMapper.Map<List<SubmissionSamplesModel>>(samples).Returns(samples.ConvertAll(s => new SubmissionSamplesModel { SampleTypeName = s.SampleTypeName }));
            _mockMapper.Map<List<SubmissionIsolatesModel>>(isolates).Returns(new List<SubmissionIsolatesModel>());

            // Act
            var result = await _controller.Index(avNumber);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SubmissionSamplesViewModel>(viewResult.Model);
            Assert.Equal("Isolates / Detections for this submission", model.IsolatesGridHeader);
        }
    }
}
