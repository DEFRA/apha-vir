using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.SubmissionServiceTest
{
    public class GetLatestSubmissionsTests
    {
        private readonly ISubmissionRepository _mockSubmissionRepository;
        private readonly IMapper _mockMapper;
        private readonly ISampleRepository _mockSampleRepository;
        private readonly IIsolateRepository _mockIsolatesRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly SubmissionService _submissionService;

        public GetLatestSubmissionsTests()
        {
            _mockSubmissionRepository = Substitute.For<ISubmissionRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockIsolatesRepository = Substitute.For<IIsolateRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _submissionService = new SubmissionService(_mockSubmissionRepository,
                _mockSampleRepository, 
                _mockIsolatesRepository,
                _mockLookupRepository,
                _mockMapper);
        }

        [Fact]
        public async Task GetLatestSubmissionsAsync_SuccessfulRetrieval_ReturnsListOfAVNumbers()
        {
            // Arrange
            var expectedAVNumbers = new List<string> { "AV001", "AV002", "AV003" };
            _mockSubmissionRepository.GetLatestSubmissionsAsync().Returns(expectedAVNumbers);

            // Act
            var result = await _submissionService.GetLatestSubmissionsAsync();

            // Assert
            Assert.Equal(expectedAVNumbers, result);
        }

        [Fact]
        public async Task GetLatestSubmissionsAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _mockSubmissionRepository.GetLatestSubmissionsAsync().Returns(new List<string>());

            // Act
            var result = await _submissionService.GetLatestSubmissionsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLatestSubmissionsAsync_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            _mockSubmissionRepository.GetLatestSubmissionsAsync().ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _submissionService.GetLatestSubmissionsAsync());
        }
    }
}
