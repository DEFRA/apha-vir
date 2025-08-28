using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Repositories;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.SampleServiceTest
{
    public class SampleServiceTests
    {
        private readonly ISampleRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly SampleService _sampleService;

        public SampleServiceTests()
        {
            _mockRepository = Substitute.For<ISampleRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sampleService = new SampleService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetSampleAsync_WithValidParameters_ReturnsSampleDTO()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var sample = new Sample { SampleId = sampleId };
            var sampleDto = new SampleDTO { SampleId = sampleId };

            _mockRepository.GetSampleAsync(avNumber, sampleId).Returns(sample);
            _mockMapper.Map<SampleDTO>(sample).Returns(sampleDto);

            // Act
            var result = await _sampleService.GetSampleAsync(avNumber, sampleId);

            // Assert
            await _mockRepository.Received(1).GetSampleAsync(avNumber, sampleId);
            _mockMapper.Received(1).Map<SampleDTO>(sample);
            Assert.Equal(sampleDto, result);
        }

        [Fact]
        public async Task GetSampleAsync_WithNonExistentSample_ReturnsNull()
        {
            // Arrange
            var avNumber = "AV456";
            var sampleId = Guid.NewGuid();

            _mockRepository.GetSampleAsync(avNumber, sampleId).Returns((Sample?)null);
            _mockMapper.Map<SampleDTO>(null).Returns((SampleDTO?)null);

            // Act
            var result = await _sampleService.GetSampleAsync(avNumber, sampleId);

            // Assert
            await _mockRepository.Received(1).GetSampleAsync(avNumber, sampleId);
            _mockMapper.Received(1).Map<SampleDTO>(null);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSampleAsync_WithNullParameters_ThrowsArgumentNullException()
        {
            // Arrange
            string? avNumber = null;
            Guid? sampleId = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sampleService.GetSampleAsync(avNumber, sampleId));
        }

        [Fact]
        public async Task AddSample_ValidInput_SuccessfullyAddsSample()
        {
            // Arrange
            var sampleDto = new SampleDTO();
            var sample = new Sample();
            var avNumber = "AV123";
            var userName = "TestUser";

            _mockMapper.Map<Sample>(sampleDto).Returns(sample);

            // Act
            await _sampleService.AddSample(sampleDto, avNumber, userName);

            // Assert
            await _mockRepository.Received(1).AddSampleAsync(sample, avNumber, userName);
        }

        [Fact]
        public async Task AddSample_NullInput_ThrowsArgumentNullException()
        {
            // Arrange
            SampleDTO? sampleDto = null; // Fix: Mark sampleDto as nullable
            var avNumber = "AV123";
            var userName = "TestUser";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sampleService.AddSample(sampleDto!, avNumber, userName)); // Fix: Use null-forgiving operator
        }

        [Fact]
        public async Task AddSample_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var sampleDto = new SampleDTO();
            var sample = new Sample();
            var avNumber = "AV123";
            var userName = "TestUser";

            _mockMapper.Map<Sample>(sampleDto).Returns(sample);
            _mockRepository.AddSampleAsync(sample, avNumber, userName)
            .Returns(Task.FromException(new Exception("Repository error"))); // Fix: Replace ThrowsAsync with Returns(Task.FromException(...))

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _sampleService.AddSample(sampleDto, avNumber, userName));
        }

        [Fact]
        public async Task UpdateSample_SuccessfulUpdate_CallsRepositoryAndReturns()
        {
            // Arrange
            var sampleDto = new SampleDTO { SampleId = Guid.NewGuid() };
            var sample = new Sample();
            var userName = "testUser";

            _mockMapper.Map<Sample>(sampleDto).Returns(sample);

            // Act
            await _sampleService.UpdateSample(sampleDto, userName);

            // Assert
            await _mockRepository.Received(1).UpdateSampleAsync(sample, userName);
        }

        [Fact]
        public async Task UpdateSample_NullSampleDto_ThrowsArgumentNullException()
        {
            // Arrange
            SampleDTO? sampleDto = null;
            var userName = "testUser";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sampleService.UpdateSample(sampleDto!, userName));
        }

        [Fact]
        public async Task UpdateSample_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var sampleDto = new SampleDTO { SampleId = Guid.NewGuid() };
            var sample = new Sample();
            var userName = "testUser";

            _mockMapper.Map<Sample>(sampleDto).Returns(sample);
            _mockRepository.UpdateSampleAsync(sample, userName).ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sampleService.UpdateSample(sampleDto, userName));
        }
    }
}
