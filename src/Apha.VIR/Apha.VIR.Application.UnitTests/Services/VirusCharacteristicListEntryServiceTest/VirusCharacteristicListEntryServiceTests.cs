using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.VirusCharacteristicListEntryServiceTest
{
    public class VirusCharacteristicListEntryServiceTests
    {
        private readonly IVirusCharacteristicListEntryRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly VirusCharacteristicListEntryService _service;

        public VirusCharacteristicListEntryServiceTests()
        {
            _mockRepository = Substitute.For<IVirusCharacteristicListEntryRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _service = new VirusCharacteristicListEntryService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetEntriesByCharacteristicIdAsync_ReturnsMappedDtos()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            var lastModified1 = new byte[] { 1, 2, 3 };
            var lastModified2 = new byte[] { 4, 5, 6 };
            var virusCharacteristicId1 = Guid.NewGuid();
            var virusCharacteristicId2 = Guid.NewGuid();

            var entities = new List<VirusCharacteristicListEntry>
    {
        new VirusCharacteristicListEntry
        {
            Id = Guid.NewGuid(),
            VirusCharacteristicId = virusCharacteristicId1,
            Name = "Entry1",
            LastModified = lastModified1
        },
        new VirusCharacteristicListEntry
        {
            Id = Guid.NewGuid(),
            VirusCharacteristicId = virusCharacteristicId2,
            Name = "Entry2",
            LastModified = lastModified2
        }
    };
            var dtos = new List<VirusCharacteristicListEntryDTO>
    {
        new VirusCharacteristicListEntryDTO
        {
            Id = entities[0].Id,
            VirusCharacteristicId = virusCharacteristicId1,
            Name = "Entry1",
            LastModified = lastModified1
        },
        new VirusCharacteristicListEntryDTO
        {
            Id = entities[1].Id,
            VirusCharacteristicId = virusCharacteristicId2,
            Name = "Entry2",
            LastModified = lastModified2
        }
    };

            _mockRepository.GetVirusCharacteristicListEntryByVirusCharacteristic(characteristicId).Returns(entities);
            _mockMapper.Map<IEnumerable<VirusCharacteristicListEntryDTO>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetEntriesByCharacteristicIdAsync(characteristicId);

            // Assert
            await _mockRepository.Received(1).GetVirusCharacteristicListEntryByVirusCharacteristic(characteristicId);
            _mockMapper.Received(1).Map<IEnumerable<VirusCharacteristicListEntryDTO>>(entities);
            Assert.Equal(dtos, result);
        }

        [Fact]
        public async Task GetEntriesByCharacteristicIdAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            var entities = new List<VirusCharacteristicListEntry>();
            var dtos = new List<VirusCharacteristicListEntryDTO>();

            _mockRepository.GetVirusCharacteristicListEntryByVirusCharacteristic(characteristicId).Returns(entities);
            _mockMapper.Map<IEnumerable<VirusCharacteristicListEntryDTO>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetEntriesByCharacteristicIdAsync(characteristicId);

            // Assert
            await _mockRepository.Received(1).GetVirusCharacteristicListEntryByVirusCharacteristic(characteristicId);
            _mockMapper.Received(1).Map<IEnumerable<VirusCharacteristicListEntryDTO>>(entities);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEntriesByCharacteristicIdAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var characteristicId = Guid.NewGuid();
            _mockRepository.GetVirusCharacteristicListEntryByVirusCharacteristic(characteristicId)
                .Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetEntriesByCharacteristicIdAsync(characteristicId));
            await _mockRepository.Received(1).GetVirusCharacteristicListEntryByVirusCharacteristic(characteristicId);
        }

        [Fact]
        public async Task GetEntryByIdAsync_EntityExists_ReturnsMappedDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new VirusCharacteristicListEntry { Id = id, Name = "Entry" };
            var dto = new VirusCharacteristicListEntryDTO { Id = id, Name = "Entry" };

            _mockRepository.GetByIdAsync(id).Returns(entity);
            _mockMapper.Map<VirusCharacteristicListEntryDTO>(entity).Returns(dto);

            // Act
            var result = await _service.GetEntryByIdAsync(id);

            // Assert
            await _mockRepository.Received(1).GetByIdAsync(id);
            _mockMapper.Received(1).Map<VirusCharacteristicListEntryDTO>(entity);
            Assert.Equal(dto, result);
        }

        [Fact]
        public async Task GetEntryByIdAsync_EntityNotFound_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns((VirusCharacteristicListEntry?)null);

            // Act
            var result = await _service.GetEntryByIdAsync(id);

            // Assert
            await _mockRepository.Received(1).GetByIdAsync(id);
            _mockMapper.DidNotReceive().Map<VirusCharacteristicListEntryDTO>(Arg.Any<VirusCharacteristicListEntry>());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetEntryByIdAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetEntryByIdAsync(id));
            await _mockRepository.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task AddEntryAsync_MapsAndCallsRepository()
        {
            // Arrange
            var dto = new VirusCharacteristicListEntryDTO { Name = "Entry" };
            var mappedEntity = new VirusCharacteristicListEntry { Name = "Entry" };

            _mockMapper.Map<VirusCharacteristicListEntry>(Arg.Any<VirusCharacteristicListEntryDTO>()).Returns(mappedEntity);

            // Act
            await _service.AddEntryAsync(dto);

            // Assert
            _mockMapper.Received(1).Map<VirusCharacteristicListEntry>(Arg.Is<VirusCharacteristicListEntryDTO>(d => d.Name == "Entry" && d.Id != Guid.Empty));
            await _mockRepository.Received(1).AddEntryAsync(mappedEntity);
        }

        [Fact]
        public async Task AddEntryAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var dto = new VirusCharacteristicListEntryDTO { Name = "Entry" };
            var mappedEntity = new VirusCharacteristicListEntry { Name = "Entry" };

            _mockMapper.Map<VirusCharacteristicListEntry>(Arg.Any<VirusCharacteristicListEntryDTO>()).Returns(mappedEntity);
            _mockRepository.AddEntryAsync(mappedEntity).Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AddEntryAsync(dto));
            await _mockRepository.Received(1).AddEntryAsync(mappedEntity);
        }

        [Fact]
        public async Task UpdateEntryAsync_MapsAndCallsRepository()
        {
            // Arrange
            var dto = new VirusCharacteristicListEntryDTO { Id = Guid.NewGuid(), Name = "Entry" };
            var mappedEntity = new VirusCharacteristicListEntry { Id = dto.Id, Name = "Entry" };

            _mockMapper.Map<VirusCharacteristicListEntry>(dto).Returns(mappedEntity);

            // Act
            await _service.UpdateEntryAsync(dto);

            // Assert
            _mockMapper.Received(1).Map<VirusCharacteristicListEntry>(dto);
            await _mockRepository.Received(1).UpdateEntryAsync(mappedEntity);
        }

        [Fact]
        public async Task UpdateEntryAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var dto = new VirusCharacteristicListEntryDTO { Id = Guid.NewGuid(), Name = "Entry" };
            var mappedEntity = new VirusCharacteristicListEntry { Id = dto.Id, Name = "Entry" };

            _mockMapper.Map<VirusCharacteristicListEntry>(dto).Returns(mappedEntity);
            _mockRepository.UpdateEntryAsync(mappedEntity).Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateEntryAsync(dto));
            await _mockRepository.Received(1).UpdateEntryAsync(mappedEntity);
        }

        [Fact]
        public async Task DeleteEntryAsync_CallsRepository()
        {
            // Arrange
            var id = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3 };

            // Act
            await _service.DeleteEntryAsync(id, lastModified);

            // Assert
            await _mockRepository.Received(1).DeleteEntryAsync(id, lastModified);
        }

        [Fact]
        public async Task DeleteEntryAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3 };
            _mockRepository.DeleteEntryAsync(id, lastModified).Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.DeleteEntryAsync(id, lastModified));
            await _mockRepository.Received(1).DeleteEntryAsync(id, lastModified);
        }
    }
}
