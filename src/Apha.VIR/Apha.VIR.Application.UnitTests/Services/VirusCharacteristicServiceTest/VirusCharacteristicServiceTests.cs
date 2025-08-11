using Apha.VIR.Application.DTOs;
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
        public async Task GetAllVirusCharacteristicsByVirusTypeAsync_EmptyStringVirusType_ReturnsMatchingCharacteristics()
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
    }
}
