using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllTraysAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllTraysAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task Test_GetAllTraysAsync_ReturnsExpectedResult()
        {
            // Arrange
            var mockTrays = new List<LookupItem>
            {
                new LookupItem { Id = Guid.NewGuid(), Name = "Tray 1" },
                new LookupItem { Id = Guid.NewGuid(), Name = "Tray 2" }
            };

            var expectedDTOs = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Tray 1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Tray 2" }
            };

            _mockLookupRepository.GetAllTraysAsync().Returns(mockTrays);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDTOs);

            // Act
            var result = await _mockLookupService.GetAllTraysAsync();

            // Assert
            Assert.Equal(expectedDTOs, result);
            await _mockLookupRepository.Received(1).GetAllTraysAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

        [Fact]
        public async Task Test_GetAllTraysAsync_ThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllTraysAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllTraysAsync());
            await _mockLookupRepository.Received(1).GetAllTraysAsync();
        }

        [Fact]
        public async Task Test_GetAllTraysByParentAsync_ReturnsExpectedResult()
        {
            // Arrange
            var freezerId = Guid.NewGuid();
            var expectedTrays = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Tray 1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Tray 2" }
            };

            _mockLookupRepository.GetAllTraysByParentAsync(freezerId).Returns(Task.FromResult(expectedTrays as IEnumerable<LookupItem>)!);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItemDTO>>()).Returns(expectedTrays);

            // Act
            var result = await _mockLookupService.GetAllTraysByParentAsync(freezerId);

            // Assert
            Assert.Equal(expectedTrays, result);
            await _mockLookupRepository.Received(1).GetAllTraysByParentAsync(freezerId);
        }

        [Fact]
        public async Task Test_GetAllTraysByParentAsync_WithNullFreezer()
        {
            // Arrange
            var expectedTrays = new List<LookupItemDTO>();
            _mockLookupRepository.GetAllTraysByParentAsync(null).Returns(Task.FromResult(expectedTrays as IEnumerable<LookupItem>)!);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItemDTO>>()).Returns(expectedTrays);

            // Act
            var result = await _mockLookupService.GetAllTraysByParentAsync(null);

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllTraysByParentAsync(null);
        }       
    }
}
