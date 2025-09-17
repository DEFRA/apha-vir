using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.IsolateViabilityServiceTest
{
    public class AddIsolateViabilityAsyncTests
    {
        protected IIsolateViabilityService _isolateViabilityService;
        protected IIsolateViabilityRepository _mockIsolateViabilityRepository;
        protected IIsolateRepository _mockIsolateRepository;
        protected ICharacteristicRepository _mockCharacteristicRepository;
        protected ILookupRepository _mockLookupRepository;
        protected IMapper _mockMapper;

        public AddIsolateViabilityAsyncTests()
        {
            _mockIsolateViabilityRepository = Substitute.For<IIsolateViabilityRepository>();
            _mockIsolateRepository = Substitute.For<IIsolateRepository>();
            _mockCharacteristicRepository = Substitute.For<ICharacteristicRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();

            _isolateViabilityService = new IsolateViabilityService(
            _mockIsolateViabilityRepository,
            _mockIsolateRepository,
            _mockCharacteristicRepository,
            _mockLookupRepository,
            _mockMapper);
        }

        [Fact]
        public async Task AddIsolateViabilityAsync_ValidInput_ShouldCallRepository()
        {
            // Arrange
            var dto = new IsolateViabilityInfoDto { IsolateViabilityIsolateId = Guid.NewGuid(), DateChecked = DateTime.Now };
            var userId = "testUser";
            var entity = new IsolateViability();
            _mockMapper.Map<IsolateViability>(dto).Returns(entity);

            // Act
            await _isolateViabilityService.AddIsolateViabilityAsync(dto, userId);

            // Assert
            await _mockIsolateViabilityRepository.Received(1).AddIsolateViabilityAsync(entity, userId);
        }

        [Fact]
        public async Task AddIsolateViabilityAsync_NullInput_ShouldThrowArgumentNullException()
        {
            // Arrange
            IsolateViabilityInfoDto? dto = null;
            var userId = "testUser";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _isolateViabilityService.AddIsolateViabilityAsync(dto!, userId));
        }

        [Fact]
        public async Task AddIsolateViabilityAsync_EmptyUserId_ShouldThrowArgumentException()
        {
            // Arrange
            var dto = new IsolateViabilityInfoDto { IsolateViabilityIsolateId = Guid.NewGuid(), DateChecked = DateTime.Now };
            var userId = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _isolateViabilityService.AddIsolateViabilityAsync(dto, userId));
        }

        [Fact]
        public async Task AddIsolateViabilityAsync_ValidInput_ShouldMapCorrectly()
        {
            // Arrange
            var dto = new IsolateViabilityInfoDto { IsolateViabilityIsolateId = Guid.NewGuid(), DateChecked = DateTime.Now };
            var userId = "testUser";
            var entity = new IsolateViability();
            _mockMapper.Map<IsolateViability>(dto).Returns(entity);

            // Act
            await _isolateViabilityService.AddIsolateViabilityAsync(dto, userId);

            // Assert
            _mockMapper.Received(1).Map<IsolateViability>(dto);
        }
    }
}
