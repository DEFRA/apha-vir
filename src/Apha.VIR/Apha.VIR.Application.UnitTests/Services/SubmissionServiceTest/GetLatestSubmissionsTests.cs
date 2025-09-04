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
        private readonly SubmissionService _submissionService;

        public GetLatestSubmissionsTests()
        {
            _mockSubmissionRepository = Substitute.For<ISubmissionRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _submissionService = new SubmissionService(_mockSubmissionRepository, _mockMapper);
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
