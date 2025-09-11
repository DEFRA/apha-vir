using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.IsolatesServiceTest
{
    public class DeleteIsolateTests
    {
        private readonly IIsolateRepository _mockIsolateRepository;
        private readonly ISubmissionRepository _mockSubmissionRepository;
        private readonly ISampleRepository _mockSampleRepository;
        private readonly ICharacteristicRepository _mockCharacteristicRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly IsolatesService _mockIsolatesService;

        public DeleteIsolateTests()
        {
            _mockIsolateRepository = Substitute.For<IIsolateRepository>();
            _mockSubmissionRepository = Substitute.For<ISubmissionRepository>();
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockCharacteristicRepository = Substitute.For<ICharacteristicRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockIsolatesService = new IsolatesService(_mockIsolateRepository, 
                _mockSubmissionRepository, 
                _mockSampleRepository, 
                _mockCharacteristicRepository, 
                _mockLookupRepository, 
                _mockMapper);
        }

        [Fact]
        public async Task DeleteSampleAsync_ValidInput_CallsRepositoryMethod()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var userId = "testUser";
            var lastModified = new byte[] { 1, 2, 3 };

            // Act
            await _mockIsolatesService.DeleteIsolateAsync(isolateId, userId, lastModified);

            // Assert
            await _mockIsolateRepository.Received(1).DeleteIsolateAsync(isolateId, userId, lastModified);
        }

        [Fact]
        public async Task DeleteIsolateAsync_NullUserId_ThrowsArgumentNullException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            string? userId = null;
            var lastModified = new byte[] { 1, 2, 3 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _mockIsolatesService.DeleteIsolateAsync(isolateId, userId!, lastModified));
        }

        [Fact]
        public async Task DeleteIsolateAsync_NullLastModified_ThrowsArgumentNullException()
        {
            // Arrange
            var sampleId = Guid.NewGuid();
            var userId = "testUser";
            byte[]? lastModified = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _mockIsolatesService.DeleteIsolateAsync(sampleId, userId, lastModified!));
        }

        [Fact]
        public async Task DeleteIsolateAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var userId = "testUser";
            var lastModified = new byte[] { 1, 2, 3 };
            _mockIsolateRepository.DeleteIsolateAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<byte[]>())
            .Returns(Task.FromException(new Exception("Repository error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _mockIsolatesService.DeleteIsolateAsync(isolateId, userId, lastModified));
        }
    }
}
