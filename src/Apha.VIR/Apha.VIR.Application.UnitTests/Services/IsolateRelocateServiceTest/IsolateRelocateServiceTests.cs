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

namespace Apha.VIR.Application.UnitTests.Services.IsolateRelocateServiceTest
{
    public class IsolateRelocateServiceTests
    {
        private readonly IIsolateRelocateRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly IsolateRelocateService _service;

        public IsolateRelocateServiceTests()
        {
            _mockRepository = Substitute.For<IIsolateRelocateRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _service = new IsolateRelocateService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetIsolatesByCriteria_ShouldReturnMappedDTOs_WhenRepositoryReturnsData()
        {
            // Arrange
            var min = "001";
            var max = "100";
            var freezer = Guid.NewGuid();
            var tray = Guid.NewGuid();
            var isolates = new List<IsolateRelocate> { new IsolateRelocate(), new IsolateRelocate() };
            var dtos = new List<IsolateRelocateDTO> { new IsolateRelocateDTO(), new IsolateRelocateDTO() };

            _mockRepository.GetIsolatesByCriteria(min, max, freezer, tray).Returns(isolates);
            _mockMapper.Map<IEnumerable<IsolateRelocateDTO>>(isolates).Returns(dtos);

            // Act
            var result = await _service.GetIsolatesByCriteria(min, max, freezer, tray);

            // Assert
            Assert.Equal(dtos, result);
            await _mockRepository.Received(1).GetIsolatesByCriteria(min, max, freezer, tray);
            _mockMapper.Received(1).Map<IEnumerable<IsolateRelocateDTO>>(isolates);
        }

        [Fact]
        public async Task GetIsolatesByCriteria_ShouldReturnEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>())
            .Returns(new List<IsolateRelocate>());
            _mockMapper.Map<IEnumerable<IsolateRelocateDTO>>(Arg.Any<IEnumerable<IsolateRelocate>>())
            .Returns(new List<IsolateRelocateDTO>());

            // Act
            var result = await _service.GetIsolatesByCriteria("001", "100", null, null);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetIsolatesByCriteria_ShouldHandleNullParameters()
        {
            // Arrange
            var isolates = new List<IsolateRelocate> { new IsolateRelocate() };
            var dtos = new List<IsolateRelocateDTO> { new IsolateRelocateDTO() };

            _mockRepository.GetIsolatesByCriteria(null, null, null, null).Returns(isolates);
            _mockMapper.Map<IEnumerable<IsolateRelocateDTO>>(isolates).Returns(dtos);

            // Act
            var result = await _service.GetIsolatesByCriteria(null, null, null, null);

            // Assert
            Assert.Single(result);
            await _mockRepository.Received(1).GetIsolatesByCriteria(null, null, null, null);
        }

        [Fact]
        public async Task GetIsolatesByCriteria_ShouldThrowException_WhenRepositoryThrowsException()
        {
            // Arrange
            _mockRepository.GetIsolatesByCriteria(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<Guid?>())
            .Returns<Task<IEnumerable<IsolateRelocate>>>(x => throw new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetIsolatesByCriteria("001", "100", null, null));
        }

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_SuccessfulUpdate()
        {
            // Arrange
            var inputDto = new IsolateRelocateDTO();
            var mappedEntity = new IsolateRelocate();
            _mockMapper.Map<IsolateRelocate>(inputDto).Returns(mappedEntity);

            // Act
            await _service.UpdateIsolateFreezeAndTrayAsync(inputDto);

            // Assert
            await _mockRepository.Received(1).UpdateIsolateFreezeAndTrayAsync(Arg.Is<IsolateRelocate>(x => x == mappedEntity));
        }        

        [Fact]
        public async Task UpdateIsolateFreezeAndTrayAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new IsolateRelocateDTO();
            var mappedEntity = new IsolateRelocate();
            _mockMapper.Map<IsolateRelocate>(inputDto).Returns(mappedEntity);
            _mockRepository.UpdateIsolateFreezeAndTrayAsync(Arg.Any<IsolateRelocate>()).ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateIsolateFreezeAndTrayAsync(inputDto));
        }
    }
}
