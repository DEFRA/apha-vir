using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.VirusCharacteristicAssociationServiceTest
{
    public class VirusCharacteristicAssociationServiceTests
    {
        private readonly IVirusCharacteristicAssociationRepository _mockRepo;
        private readonly VirusCharacteristicAssociationService _service;

        public VirusCharacteristicAssociationServiceTests()
        {
            _mockRepo = Substitute.For<IVirusCharacteristicAssociationRepository>();
            _service = new VirusCharacteristicAssociationService(_mockRepo);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            IVirusCharacteristicAssociationRepository? nullRepository = null;
            Assert.Throws<ArgumentNullException>(() => new VirusCharacteristicAssociationService(nullRepository!));
        }

        [Fact]
        public async Task AssignCharacteristicToTypeAsync_ValidArguments_CallsRepositoryMethod()
        {
            // Arrange
            var virusTypeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();

            // Act
            await _service.AssignCharacteristicToTypeAsync(virusTypeId, characteristicId);

            // Assert
            await _mockRepo.Received(1).AssignCharacteristicToTypeAsync(virusTypeId, characteristicId);
        }

        [Fact]
        public async Task AssignCharacteristicToTypeAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var virusTypeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();
            _mockRepo.AssignCharacteristicToTypeAsync(virusTypeId, characteristicId)
                .Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AssignCharacteristicToTypeAsync(virusTypeId, characteristicId));
            await _mockRepo.Received(1).AssignCharacteristicToTypeAsync(virusTypeId, characteristicId);
        }

        [Fact]
        public async Task RemoveCharacteristicFromTypeAsync_ValidArguments_CallsRepositoryMethod()
        {
            // Arrange
            var virusTypeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();

            // Act
            await _service.RemoveCharacteristicFromTypeAsync(virusTypeId, characteristicId);

            // Assert
            await _mockRepo.Received(1).RemoveCharacteristicFromTypeAsync(virusTypeId, characteristicId);
        }

        [Fact]
        public async Task RemoveCharacteristicFromTypeAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var virusTypeId = Guid.NewGuid();
            var characteristicId = Guid.NewGuid();
            _mockRepo.RemoveCharacteristicFromTypeAsync(virusTypeId, characteristicId)
                .Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.RemoveCharacteristicFromTypeAsync(virusTypeId, characteristicId));
            await _mockRepo.Received(1).RemoveCharacteristicFromTypeAsync(virusTypeId, characteristicId);
        }
    }
}
