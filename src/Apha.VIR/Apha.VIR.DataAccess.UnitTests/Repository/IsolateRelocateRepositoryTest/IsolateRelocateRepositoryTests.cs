using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Moq;


namespace Apha.VIR.DataAccess.UnitTests.Repository.IsolateRelocateRepositoryTest
{
    public class IsolateRelocateRepositoryTests
    {
        private readonly Mock<VIRDbContext> _mockContext;
        private readonly Mock<IsolateRelocateRepository> _mockRepository;

        public IsolateRelocateRepositoryTests()
        {
            _mockContext = new Mock<VIRDbContext>();
            _mockRepository = new Mock<IsolateRelocateRepository>(_mockContext.Object);
        }

        [Fact]
        public async Task GetIsolatesByCriteria_ReturnsEmptyList()
        {
            // Arrange
            string min = "001";
            string max = "100";
            Guid freezer = Guid.NewGuid();
            Guid tray = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.GetIsolatesByCriteria(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()))
                .ReturnsAsync(Enumerable.Empty<IsolateRelocate>());

            // Act
            var result = await _mockRepository.Object.GetIsolatesByCriteria(min, max, freezer, tray);

            // Assert
            Assert.Empty(result);
            _mockRepository.Verify(repo => repo.GetIsolatesByCriteria(min, max, freezer, tray), Times.Once);
        }

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_IsolateType_ExecutesWithCorrectParameters()
        {
            // Arrange
            var isolateRelocate = new IsolateRelocate
            {
                UpdateType = "Isolate",
                UserID = "testUser",
                IsolateId = Guid.NewGuid(),
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid(),
                Well = "A1",
                LastModified = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 } // Mock timestamp
            };

            _mockRepository.Setup(repo => repo.UpdateIsolateFreezeAndTrayAsync(It.IsAny<IsolateRelocate>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockRepository.Object.UpdateIsolateFreezeAndTrayAsync(isolateRelocate);

            // Assert
            _mockRepository.Verify(repo => repo.UpdateIsolateFreezeAndTrayAsync(It.Is<IsolateRelocate>(
                ir => ir.UpdateType == "Isolate" &&
                      ir.UserID == "testUser" &&
                      ir.IsolateId == isolateRelocate.IsolateId &&
                      ir.Freezer == isolateRelocate.Freezer &&
                      ir.Tray == isolateRelocate.Tray &&
                      ir.Well == "A1" &&
                      ir.LastModified != null &&
                      ir.LastModified.SequenceEqual(isolateRelocate.LastModified))), Times.Once);
        }


        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_TrayType_ExecutesWithCorrectParameters()
        {
            // Arrange
            var isolateRelocate = new IsolateRelocate
            {
                UpdateType = "Tray",
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid()
            };

            _mockRepository.Setup(repo => repo.UpdateIsolateFreezeAndTrayAsync(It.IsAny<IsolateRelocate>()))
                .Returns(Task.CompletedTask);

            // Act
            await _mockRepository.Object.UpdateIsolateFreezeAndTrayAsync(isolateRelocate);

            // Assert
            _mockRepository.Verify(repo => repo.UpdateIsolateFreezeAndTrayAsync(It.Is<IsolateRelocate>(
                ir => ir.UpdateType == "Tray" &&
                      ir.Freezer == isolateRelocate.Freezer &&
                      ir.Tray == isolateRelocate.Tray)), Times.Once);
        }
        [Fact]
        public async Task GetIsolatesByCriteria_WithNullParameters_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetIsolatesByCriteria(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()))
                .ReturnsAsync(Enumerable.Empty<IsolateRelocate>());

            // Act
            var result = await _mockRepository.Object.GetIsolatesByCriteria(null, null, null, null);

            // Assert
            Assert.Empty(result);
            _mockRepository.Verify(repo => repo.GetIsolatesByCriteria(null, null, null, null), Times.Once);
        }

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_WithInvalidUpdateType_ThrowsArgumentException()
        {
            // Arrange
            var isolateRelocate = new IsolateRelocate
            {
                UpdateType = "InvalidType",
                Freezer = Guid.NewGuid(),
                Tray = Guid.NewGuid()
            };

            _mockRepository.Setup(repo => repo.UpdateIsolateFreezeAndTrayAsync(It.IsAny<IsolateRelocate>()))
                .ThrowsAsync(new ArgumentException("Invalid UpdateType"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _mockRepository.Object.UpdateIsolateFreezeAndTrayAsync(isolateRelocate));
        }

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_IsolateTypeWithMissingRequiredFields_ThrowsArgumentException()
        {
            // Arrange
            var isolateRelocate = new IsolateRelocate
            {
                UpdateType = "Isolate",
                // Missing UserID, IsolateId, Freezer, Tray
            };

            _mockRepository.Setup(repo => repo.UpdateIsolateFreezeAndTrayAsync(It.IsAny<IsolateRelocate>()))
                .ThrowsAsync(new ArgumentException("Missing required fields for Isolate update"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _mockRepository.Object.UpdateIsolateFreezeAndTrayAsync(isolateRelocate));
        }

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_TrayTypeWithMissingRequiredFields_ThrowsArgumentException()
        {
            // Arrange
            var isolateRelocate = new IsolateRelocate
            {
                UpdateType = "Tray",
                // Missing Freezer or Tray
            };

            _mockRepository.Setup(repo => repo.UpdateIsolateFreezeAndTrayAsync(It.IsAny<IsolateRelocate>()))
                .ThrowsAsync(new ArgumentException("Missing required fields for Tray update"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _mockRepository.Object.UpdateIsolateFreezeAndTrayAsync(isolateRelocate));
        }

       
    }
}
