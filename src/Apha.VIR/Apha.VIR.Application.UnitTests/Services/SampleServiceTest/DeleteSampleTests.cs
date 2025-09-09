using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.SampleServiceTest
{
    public class DeleteSampleTests
    {
        private readonly ISampleRepository _mockSampleRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly SampleService _sampleService;

        public DeleteSampleTests()
        {
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sampleService = new SampleService(_mockSampleRepository, _mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task DeleteSampleAsync_ValidInput_CallsRepositoryMethod()
        {
            // Arrange
            var sampleId = Guid.NewGuid();
            var userId = "testUser";
            var lastModified = new byte[] { 1, 2, 3 };

            // Act
            await _sampleService.DeleteSampleAsync(sampleId, userId, lastModified);

            // Assert
            await _mockSampleRepository.Received(1).DeleteSampleAsync(sampleId, userId, lastModified);
        }       

        [Fact]
        public async Task DeleteSampleAsync_NullUserId_ThrowsArgumentNullException()
        {
            // Arrange
            var sampleId = Guid.NewGuid();
            string? userId = null;
            var lastModified = new byte[] { 1, 2, 3 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sampleService.DeleteSampleAsync(sampleId, userId!, lastModified));
        }

        [Fact]
        public async Task DeleteSampleAsync_NullLastModified_ThrowsArgumentNullException()
        {
            // Arrange
            var sampleId = Guid.NewGuid();
            var userId = "testUser";
            byte[]? lastModified = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _sampleService.DeleteSampleAsync(sampleId, userId, lastModified!));
        }

        [Fact]
        public async Task DeleteSampleAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var sampleId = Guid.NewGuid();
            var userId = "testUser";
            var lastModified = new byte[] { 1, 2, 3 };
            _mockSampleRepository.DeleteSampleAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<byte[]>())
            .Returns(Task.FromException(new Exception("Repository error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _sampleService.DeleteSampleAsync(sampleId, userId, lastModified));
        }
    }
}
