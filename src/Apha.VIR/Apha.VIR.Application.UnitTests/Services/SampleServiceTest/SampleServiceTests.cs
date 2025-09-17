using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.SampleServiceTest
{
    public class SampleServiceTests
    {
        private readonly ISampleRepository _mockSampleRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly SampleService _sampleService;

        public SampleServiceTests()
        {
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sampleService = new SampleService(_mockSampleRepository, _mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetSampleAsync_WithValidParameters_ReturnsSampleDto()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var sample = new Sample { SampleId = sampleId };
            var SampleDto = new SampleDto { SampleId = sampleId };

            _mockSampleRepository.GetSampleAsync(avNumber, sampleId).Returns(sample);
            _mockMapper.Map<SampleDto>(sample).Returns(SampleDto);

            // Act
            var result = await _sampleService.GetSampleAsync(avNumber, sampleId);

            // Assert
            await _mockSampleRepository.Received(1).GetSampleAsync(avNumber, sampleId);
            _mockMapper.Received(1).Map<SampleDto>(sample);
            Assert.Equal(SampleDto, result);
        }

        [Fact]
        public async Task GetSampleAsync_WithNonExistentSample_ReturnsNull()
        {
            // Arrange
            var avNumber = "AV456";
            var sampleId = Guid.NewGuid();

            _mockSampleRepository.GetSampleAsync(avNumber, sampleId).Returns((Sample?)null);
            _mockMapper.Map<SampleDto>(null).Returns((SampleDto?)null);

            // Act
            var result = await _sampleService.GetSampleAsync(avNumber, sampleId);

            // Assert
            await _mockSampleRepository.Received(1).GetSampleAsync(avNumber, sampleId);
            _mockMapper.Received(1).Map<SampleDto>(null);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSampleAsync_WithNullParameters_ThrowsArgumentNullException()
        {
            // Arrange
            string? avNumber = null;
            Guid? sampleId = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sampleService.GetSampleAsync(avNumber!, sampleId));
        }

        [Fact]
        public async Task AddSample_ValidInput_SuccessfullyAddsSample()
        {
            // Arrange
            var SampleDto = new SampleDto();
            var sample = new Sample();
            var avNumber = "AV123";
            var userName = "TestUser";

            _mockMapper.Map<Sample>(SampleDto).Returns(sample);

            // Act
            await _sampleService.AddSample(SampleDto, avNumber, userName);

            // Assert
            await _mockSampleRepository.Received(1).AddSampleAsync(sample, avNumber, userName);
        }

        [Fact]
        public async Task AddSample_NullInput_ThrowsArgumentNullException()
        {
            // Arrange
            SampleDto? SampleDto = null; // Fix: Mark SampleDto as nullable
            var avNumber = "AV123";
            var userName = "TestUser";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sampleService.AddSample(SampleDto!, avNumber, userName)); // Fix: Use null-forgiving operator
        }

        [Fact]
        public async Task AddSample_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var SampleDto = new SampleDto();
            var sample = new Sample();
            var avNumber = "AV123";
            var userName = "TestUser";

            _mockMapper.Map<Sample>(SampleDto).Returns(sample);
            _mockSampleRepository.AddSampleAsync(sample, avNumber, userName)
            .Returns(Task.FromException(new Exception("Repository error"))); // Fix: Replace ThrowsAsync with Returns(Task.FromException(...))

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _sampleService.AddSample(SampleDto, avNumber, userName));
        }

        [Fact]
        public async Task UpdateSample_SuccessfulUpdate_CallsRepositoryAndReturns()
        {
            // Arrange
            var SampleDto = new SampleDto { SampleId = Guid.NewGuid() };
            var sample = new Sample();
            var userName = "testUser";

            _mockMapper.Map<Sample>(SampleDto).Returns(sample);

            // Act
            await _sampleService.UpdateSample(SampleDto, userName);

            // Assert
            await _mockSampleRepository.Received(1).UpdateSampleAsync(sample, userName);
        }

        [Fact]
        public async Task UpdateSample_NullSampleDto_ThrowsArgumentNullException()
        {
            // Arrange
            SampleDto? SampleDto = null;
            var userName = "testUser";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sampleService.UpdateSample(SampleDto!, userName));
        }

        [Fact]
        public async Task UpdateSample_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var SampleDto = new SampleDto { SampleId = Guid.NewGuid() };
            var sample = new Sample();
            var userName = "testUser";

            _mockMapper.Map<Sample>(SampleDto).Returns(sample);
            _mockSampleRepository.UpdateSampleAsync(sample, userName).ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sampleService.UpdateSample(SampleDto, userName));
        }
    }
}
