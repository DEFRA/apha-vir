using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.SampleServiceTest
{
    public class GetSamplesBySubmissionTests
    {
        private readonly ISampleRepository _mockSampleRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly SampleService _mockSampleService;

        public GetSamplesBySubmissionTests()
        {
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockSampleService = new SampleService(_mockSampleRepository, 
                _mockLookupRepository,
                _mockMapper);
        }

        [Fact]
        public async Task GetSamplesBySubmissionIdAsync_ShouldReturnMappedDTOs_WhenSamplesExist()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var samples = new List<Sample> { new Sample(), new Sample() };
            var SampleDtos = new List<SampleDto> { new SampleDto(), new SampleDto() };

            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submissionId).Returns(samples);
            _mockMapper.Map<IEnumerable<SampleDto>>(samples).Returns(SampleDtos);

            // Act
            var result = await _mockSampleService.GetSamplesBySubmissionIdAsync(submissionId);

            // Assert
            Assert.Equal(SampleDtos, result);
            await _mockSampleRepository.Received(1).GetSamplesBySubmissionIdAsync(submissionId);
            _mockMapper.Received(1).Map<IEnumerable<SampleDto>>(samples);
        }

        [Fact]
        public async Task GetSamplesBySubmissionIdAsync_ShouldReturnEmptyCollection_WhenNoSamplesExist()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var emptySamples = new List<Sample>();
            var emptySampleDtos = new List<SampleDto>();

            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submissionId).Returns(emptySamples);
            _mockMapper.Map<IEnumerable<SampleDto>>(emptySamples).Returns(emptySampleDtos);

            // Act
            var result = await _mockSampleService.GetSamplesBySubmissionIdAsync(submissionId);

            // Assert
            Assert.Empty(result);
            await _mockSampleRepository.Received(1).GetSamplesBySubmissionIdAsync(submissionId);
            _mockMapper.Received(1).Map<IEnumerable<SampleDto>>(emptySamples);
        }

        [Fact]
        public async Task GetSamplesBySubmissionIdAsync_ShouldHandleRepositoryException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submissionId)
            .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _mockSampleService.GetSamplesBySubmissionIdAsync(submissionId));
        }

    }
}
