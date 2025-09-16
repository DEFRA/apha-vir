using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.VirusCharacteristicServiceTest
{
    public class VirusCharacteristicServiceTests
    {
        private readonly IVirusCharacteristicRepository _mockVirusCharacteristicRepository;
        private readonly IMapper _mockMapper;
        private readonly VirusCharacteristicService _mockVirusCharacteristicService;

        public VirusCharacteristicServiceTests()
        {
            _mockVirusCharacteristicRepository = Substitute.For<IVirusCharacteristicRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockVirusCharacteristicService = new VirusCharacteristicService(_mockVirusCharacteristicRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllVirusCharacteristicsAsync_SuccessfulRetrieval_ReturnsCorrectNumberOfItems()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            // Arrange
            var repositoryResult = new List<VirusCharacteristic>
            {
                new VirusCharacteristic { Id = id1, Name = "Characteristic 1" },
                new VirusCharacteristic { Id = id2, Name = "Characteristic 2" }
            };
            var expectedDtos = new List<VirusCharacteristicDTO>
            {
                new VirusCharacteristicDTO { Id = id1, Name = "Characteristic 1" },
                new VirusCharacteristicDTO { Id = id2, Name = "Characteristic 2" }
            };

            _mockVirusCharacteristicRepository.GetAllVirusCharacteristicsAsync().Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<VirusCharacteristicDTO>>(Arg.Any<IEnumerable<VirusCharacteristic>>()).Returns(expectedDtos);

            // Act
            var result = await _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync();

            // Assert
            await _mockVirusCharacteristicRepository.Received(1).GetAllVirusCharacteristicsAsync();
            _mockMapper.Received(1).Map<IEnumerable<VirusCharacteristicDTO>>(Arg.Any<IEnumerable<VirusCharacteristic>>());
            Assert.Equal(expectedDtos.Count, result.Count());
        }

        [Fact]
        public async Task GetAllVirusCharacteristicsAsync_EmptyResultSet_ReturnsEmptyList()
        {
            // Arrange
            var repositoryResult = new List<VirusCharacteristic>();
            var expectedDtos = new List<VirusCharacteristicDTO>();

            _mockVirusCharacteristicRepository.GetAllVirusCharacteristicsAsync().Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<VirusCharacteristicDTO>>(Arg.Any<IEnumerable<VirusCharacteristic>>()).Returns(expectedDtos);

            // Act
            var result = await _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync();

            // Assert
            await _mockVirusCharacteristicRepository.Received(1).GetAllVirusCharacteristicsAsync();
            _mockMapper.Received(1).Map<IEnumerable<VirusCharacteristicDTO>>(Arg.Any<IEnumerable<VirusCharacteristic>>());
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllVirusCharacteristicsAsync_ExceptionThrown_PropagatesException()
        {
            // Arrange
            _mockVirusCharacteristicRepository.GetAllVirusCharacteristicsAsync().Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync());
            await _mockVirusCharacteristicRepository.Received(1).GetAllVirusCharacteristicsAsync();
        }

        [Fact]
        public async Task GetAllVirusCharacteristicsByVirusTypeAsync_ValidVirusTypeAndIsAbscentTrue_ReturnsMatchingCharacteristics()
        {
            // Arrange
            Guid virusType = Guid.NewGuid();
            var isAbscent = true;
            var characteristics = new List<VirusCharacteristic> { new VirusCharacteristic() };
            var expectedDtos = new List<VirusCharacteristicDTO> { new VirusCharacteristicDTO() };

            _mockVirusCharacteristicRepository.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent).Returns(characteristics);
            _mockMapper.Map<IEnumerable<VirusCharacteristicDTO>>(characteristics).Returns(expectedDtos);

            // Act
            var result = await _mockVirusCharacteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent);

            // Assert
            await _mockVirusCharacteristicRepository.Received(1).GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent);
            _mockMapper.Received(1).Map<IEnumerable<VirusCharacteristicDTO>>(characteristics);
            Assert.Equal(expectedDtos, result);
        }

        [Fact]
        public async Task GetAllVirusCharacteristicsByVirusTypeAsync_ValidVirusTypeAndIsAbscentFalse_ReturnsMatchingCharacteristics()
        {
            // Arrange
            Guid virusType = Guid.NewGuid();
            var isAbscent = false;
            var characteristics = new List<VirusCharacteristic> { new VirusCharacteristic() };
            var expectedDtos = new List<VirusCharacteristicDTO> { new VirusCharacteristicDTO() };

            _mockVirusCharacteristicRepository.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent).Returns(characteristics);
            _mockMapper.Map<IEnumerable<VirusCharacteristicDTO>>(characteristics).Returns(expectedDtos);

            // Act
            var result = await _mockVirusCharacteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent);

            // Assert
            await _mockVirusCharacteristicRepository.Received(1).GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent);
            _mockMapper.Received(1).Map<IEnumerable<VirusCharacteristicDTO>>(characteristics);
            Assert.Equal(expectedDtos, result);
        }

        [Fact]
        public async Task GetAllVirusCharacteristicsByVirusTypeAsync_NullVirusType_ReturnsMatchingCharacteristics()
        {
            // Arrange
            Guid? virusType = null;
            var isAbscent = true;
            var characteristics = new List<VirusCharacteristic> { new VirusCharacteristic() };
            var expectedDtos = new List<VirusCharacteristicDTO> { new VirusCharacteristicDTO() };

            _mockVirusCharacteristicRepository.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent).Returns(characteristics);
            _mockMapper.Map<IEnumerable<VirusCharacteristicDTO>>(characteristics).Returns(expectedDtos);

            // Act
            var result = await _mockVirusCharacteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent);

            // Assert
            await _mockVirusCharacteristicRepository.Received(1).GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent);
            _mockMapper.Received(1).Map<IEnumerable<VirusCharacteristicDTO>>(characteristics);
            Assert.Equal(expectedDtos, result);
        }

        [Fact]
        public async Task GetAllVirusCharacteristicsByVirusTypeAsync_RepositoryReturnsEmptyList_ReturnsEmptyList()
        {
            // Arrange
            Guid virusType = Guid.NewGuid();
            var isAbscent = true;
            var characteristics = new List<VirusCharacteristic>();
            var expectedDtos = new List<VirusCharacteristicDTO>();

            _mockVirusCharacteristicRepository.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent).Returns(characteristics);
            _mockMapper.Map<IEnumerable<VirusCharacteristicDTO>>(characteristics).Returns(expectedDtos);

            // Act
            var result = await _mockVirusCharacteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent);

            // Assert
            await _mockVirusCharacteristicRepository.Received(1).GetAllVirusCharacteristicsByVirusTypeAsync(virusType, isAbscent);
            _mockMapper.Received(1).Map<IEnumerable<VirusCharacteristicDTO>>(characteristics);
            Assert.Empty(result);
        }


        [Fact]
        public async Task AddEntryAsync_ValidDto_ShouldAddNewVirusCharacteristic()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new VirusCharacteristicDTO { Id= id, Name = "Test Virus" };
            var entity = new VirusCharacteristic { Id = id, Name = "Test Virus" };

            _mockMapper.Map<VirusCharacteristic>(dto).Returns(entity);

            // Act
            await _mockVirusCharacteristicService.AddEntryAsync(dto);

            // Assert
            await _mockVirusCharacteristicRepository.Received(1)
                .AddEntryAsync(Arg.Is<VirusCharacteristic>(v => v.Equals(entity)));
            
            Assert.NotEqual(Guid.Empty, dto.Id);
        }

        [Fact]
        public async Task AddEntryAsync_ShouldCallRepositoryWithMappedEntity()
        {
            // Arrange
            var dto = new VirusCharacteristicDTO();
            var entity = new VirusCharacteristic();
            _mockMapper.Map<VirusCharacteristic>(dto).Returns(entity);

            // Act
            await _mockVirusCharacteristicService.AddEntryAsync(dto);

            // Assert
            await _mockVirusCharacteristicRepository.Received(1).AddEntryAsync(Arg.Is<VirusCharacteristic>(v => v == entity));
        }


        [Fact]
        public async Task UpdateEntryAsync_ShouldCallRepositoryWithMappedEntity()
        {
            // Arrange
            var dto = new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Test Virus" };
            var entity = new VirusCharacteristic { Id = dto.Id, Name = dto.Name };
            _mockMapper.Map<VirusCharacteristic>(dto).Returns(entity);

            // Act
            await _mockVirusCharacteristicService.UpdateEntryAsync(dto);

            // Assert
            await _mockVirusCharacteristicRepository.Received(1).UpdateEntryAsync(Arg.Is<VirusCharacteristic>(v => v.Id == entity.Id && v.Name == entity.Name));
        }


        [Fact]
        public async Task UpdateEntryAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var dto = new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Test Virus" };
            _mockMapper.Map<VirusCharacteristic>(dto).Returns(new VirusCharacteristic());
            _mockVirusCharacteristicRepository.UpdateEntryAsync(Arg.Any<VirusCharacteristic>()).Returns(Task.FromException(new Exception("Test exception")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockVirusCharacteristicService.UpdateEntryAsync(dto));
        }


        [Fact]
        public async Task DeleteVirusCharactersticsAsync_Success()
        {
            // Arrange
            var id = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3 };

            // Act
            await _mockVirusCharacteristicService.DeleteVirusCharactersticsAsync(id, lastModified);

            // Assert
            await _mockVirusCharacteristicRepository.Received(1).DeleteVirusCharactersticsAsync(id, lastModified);
        }


        [Fact]
        public async Task Test_DeleteVirusCharactersticsAsync_ThrowsException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3 };
            _mockVirusCharacteristicRepository.DeleteVirusCharactersticsAsync(id, lastModified).Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockVirusCharacteristicService.DeleteVirusCharactersticsAsync(id, lastModified));
        }
    }
}
