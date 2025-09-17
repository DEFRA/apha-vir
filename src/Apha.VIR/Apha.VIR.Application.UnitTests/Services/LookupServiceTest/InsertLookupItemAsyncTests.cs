using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class InsertLookupItemAsyncTests
    {
        private readonly ILookupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _lookupService;

        public InsertLookupItemAsyncTests()
        {
            _mockRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _lookupService = new LookupService(_mockRepository, _mockMapper);
        }
        [Fact]
        public async Task InsertLookupItemAsync_SuccessfulInsert_WhenValidInput()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var dto = new LookupItemDto();
            var entity = new LookupItem();

            _mockMapper.Map<LookupItem>(dto).Returns(entity);

            // Act & Assert
            var exception = await Record.ExceptionAsync(() =>
                _lookupService.InsertLookupItemAsync(lookupId, dto));

            Assert.Null(exception);
            _mockMapper.Received(1).Map<LookupItem>(dto);
            await _mockRepository.Received(1).InsertLookupItemAsync(lookupId, entity);
        }

        [Fact]
        public async Task InsertLookupItemAsync_ExceptionIsThrown_WhenRepositoryThrowsException()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var dto = new LookupItemDto();
            var entity = new LookupItem();

            _mockMapper.Map<LookupItem>(dto).Returns(entity);
            _mockRepository
                .InsertLookupItemAsync(lookupId, entity)
                .Returns<Task>(_ => throw new InvalidOperationException("Insert failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _lookupService.InsertLookupItemAsync(lookupId, dto));

            Assert.Equal("Insert failed", exception.Message);
            _mockMapper.Received(1).Map<LookupItem>(dto);
            await _mockRepository.Received(1).InsertLookupItemAsync(lookupId, entity);
        }
    }
}
