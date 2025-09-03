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
        private readonly IMapper _mockMapper;
        private readonly SampleService _mockSampleService;

        public GetSamplesBySubmissionTests()
        {
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockSampleService = new SampleService(_mockSampleRepository, _mockMapper);
        }

        [Fact]
        public async Task GetSamplesBySubmissionIdAsync_ShouldReturnMappedDTOs_WhenSamplesExist()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var samples = new List<Sample> { new Sample(), new Sample() };
            var sampleDTOs = new List<SampleDTO> { new SampleDTO(), new SampleDTO() };

            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submissionId).Returns(samples);
            _mockMapper.Map<IEnumerable<SampleDTO>>(samples).Returns(sampleDTOs);

            // Act
            var result = await _mockSampleService.GetSamplesBySubmissionIdAsync(submissionId);

            // Assert
            Assert.Equal(sampleDTOs, result);
            await _mockSampleRepository.Received(1).GetSamplesBySubmissionIdAsync(submissionId);
            _mockMapper.Received(1).Map<IEnumerable<SampleDTO>>(samples);
        }

        [Fact]
        public async Task GetSamplesBySubmissionIdAsync_ShouldReturnEmptyCollection_WhenNoSamplesExist()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var emptySamples = new List<Sample>();
            var emptySampleDTOs = new List<SampleDTO>();

            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submissionId).Returns(emptySamples);
            _mockMapper.Map<IEnumerable<SampleDTO>>(emptySamples).Returns(emptySampleDTOs);

            // Act
            var result = await _mockSampleService.GetSamplesBySubmissionIdAsync(submissionId);

            // Assert
            Assert.Empty(result);
            await _mockSampleRepository.Received(1).GetSamplesBySubmissionIdAsync(submissionId);
            _mockMapper.Received(1).Map<IEnumerable<SampleDTO>>(emptySamples);
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
