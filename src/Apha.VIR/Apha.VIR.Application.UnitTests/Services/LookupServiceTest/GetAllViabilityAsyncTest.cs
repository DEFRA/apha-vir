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

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllViabilityAsyncTest
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllViabilityAsyncTest()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task Test_GetAllViabilityAsync_ReturnsExpectedResult()
        {
            // Arrange
            var viabilityEntities = new List<LookupItem> { new LookupItem(), new LookupItem() };
            var expectedDtos = new List<LookupItemDto> { new LookupItemDto(), new LookupItemDto() };

            _mockLookupRepository.GetAllViabilityAsync().Returns(viabilityEntities);
            _mockMapper.Map<IEnumerable<LookupItemDto>>(viabilityEntities).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllViabilityAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
        }

        [Fact]
        public async Task Test_GetAllViabilityAsync_CallsRepositoryMethod()
        {
            // Arrange
            _mockLookupRepository.GetAllViabilityAsync().Returns(new List<LookupItem>());

            // Act
            await _mockLookupService.GetAllViabilityAsync();

            // Assert
            await _mockLookupRepository.Received(1).GetAllViabilityAsync();
        }

        [Fact]
        public async Task Test_GetAllViabilityAsync_HandlesEmptyResult()
        {
            // Arrange
            _mockLookupRepository.GetAllViabilityAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDto>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDto>());

            // Act
            var result = await _mockLookupService.GetAllViabilityAsync();

            // Assert
            Assert.Empty(result);
        }

    }
}
