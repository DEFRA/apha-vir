using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Repositories;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.SubmissionServiceTest
{
    public class DeleteSubmissionTests
    {
        private readonly ISubmissionRepository _mockSubmissionRepository;
        private readonly IMapper _mockMapper;
        private readonly ISampleRepository _mockSampleRepository;
        private readonly IIsolateRepository _mockIsolatesRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly SubmissionService _submissionService;

        public DeleteSubmissionTests()
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
        public async Task DeleteSubmissionAsync_ValidInput_CallsRepositoryMethod()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var userId = "testUser";
            var lastModified = new byte[] { 1, 2, 3 };

            // Act
            await _submissionService.DeleteSubmissionAsync(submissionId, userId, lastModified);

            // Assert
            await _mockSubmissionRepository.Received(1).DeleteSubmissionAsync(submissionId, userId, lastModified);
        }

        [Fact]
        public async Task DeleteSubmissionAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            var userId = "testUser";
            var lastModified = new byte[] { 1, 2, 3 };
            _mockSubmissionRepository.DeleteSubmissionAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<byte[]>())
            .Returns(Task.FromException(new Exception("Repository error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _submissionService.DeleteSubmissionAsync(submissionId, userId, lastModified));
        } 
    }
}
