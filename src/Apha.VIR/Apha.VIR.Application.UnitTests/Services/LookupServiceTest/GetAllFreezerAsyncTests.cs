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
    public class GetAllFreezerAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllFreezerAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task Test_GetAllFreezerAsync_ReturnsExpectedResult()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var freezers = new List<LookupItem>
            {
                new LookupItem { Id = id1, Name = "Freezer 1" },
                new LookupItem { Id = id2, Name = "Freezer 2" }
            };
            var expectedDtos = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = freezers[0].Id, Name = freezers[0].Name },
                new LookupItemDTO { Id = freezers[1].Id, Name = freezers[1].Name }
            };

            _mockLookupRepository.GetAllFreezerAsync().Returns(freezers);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllFreezerAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
            await _mockLookupRepository.Received(1).GetAllFreezerAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == freezers));
        }

        [Fact]
        public async Task Test_GetAllFreezerAsync_EmptyResult()
        {
            // Arrange
            _mockLookupRepository.GetAllFreezerAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllFreezerAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllFreezerAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => !x.Any()));
        }

        [Fact]
        public async Task Test_GetAllFreezerAsync_ThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllFreezerAsync().ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllFreezerAsync());
            await _mockLookupRepository.Received(1).GetAllFreezerAsync();
        }
    }
}
