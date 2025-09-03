using Apha.VIR.Core.Entities;
using Apha.VIR.DataAccess.Data;
using Apha.VIR.DataAccess.Repositories;
using Moq;

namespace Apha.VIR.DataAccess.UnitTests.Repository
{
    public class SystemInfoRepositoryTests
    {
        private readonly Mock<VIRDbContext> _mockContext;
        private readonly SystemInfoRepository _repository;
        private readonly Mock<SystemInfoRepository> _mockRepository;

        public SystemInfoRepositoryTests()
        {
            _mockContext = new Mock<VIRDbContext>();
            _mockRepository = new Mock<SystemInfoRepository>(_mockContext.Object);
            _repository = _mockRepository.Object;
        }

        [Fact]
        public async Task GetLatestSysInfoAsync_ReturnsSystemInfo_WhenDataExists()
        {
            // Arrange
            var expectedSystemInfo = new SystemInfo
            {
                Id = Guid.NewGuid(),
                SystemName = "VIRLocal",
                DatabaseVersion = "SQL 2022",
                ReleaseDate = DateTime.Now,
                Environment = "Unit Test",
                Live = false,
                ReleaseNotes = "Unit Test release"
            };
            _mockRepository.Setup(r => r.ExecuteStoredProcedureAsync())
            .ReturnsAsync(new List<SystemInfo> { expectedSystemInfo });

            // Act
            var result = await _repository.GetLatestSysInfoAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSystemInfo.Id, result.Id);
            Assert.Equal(expectedSystemInfo.SystemName, result.SystemName);
            _mockRepository.Verify(r => r.ExecuteStoredProcedureAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLatestSysInfoAsync_ThrowsInvalidOperationException_WhenNoDataExists()
        {
            // Arrange
            _mockRepository.Setup(r => r.ExecuteStoredProcedureAsync())
            .ReturnsAsync(new List<SystemInfo>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.GetLatestSysInfoAsync());
            _mockRepository.Verify(r => r.ExecuteStoredProcedureAsync(), Times.Once);
        }
    }
}