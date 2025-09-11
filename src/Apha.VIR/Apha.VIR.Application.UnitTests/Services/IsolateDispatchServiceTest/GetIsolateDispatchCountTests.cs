using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.IsolateDispatchServiceTest
{
    public class GetIsolateDispatchCountTests
    {
        private readonly IIsolateDispatchRepository _isolateDispatchRepository;
        private readonly IIsolateRepository _isolateRepository;
        private readonly ICharacteristicRepository _characteristicRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IMapper _mapper;
        private readonly IsolateDispatchService _service;
        private readonly IIsolateViabilityRepository _isolateViabilityRepository;

        public GetIsolateDispatchCountTests()
        {
            _isolateDispatchRepository = Substitute.For<IIsolateDispatchRepository>();
            _isolateRepository = Substitute.For<IIsolateRepository>();
            _characteristicRepository = Substitute.For<ICharacteristicRepository>();
            _lookupRepository = Substitute.For<ILookupRepository>();
            _mapper = Substitute.For<IMapper>();
            _isolateViabilityRepository = Substitute.For<IIsolateViabilityRepository>(); // Added missing dependency

            _service = new IsolateDispatchService(
                _isolateDispatchRepository,
                _isolateRepository,
                _characteristicRepository,
                _lookupRepository,
                _mapper,
                _isolateViabilityRepository
            );
        }

        [Fact]
        public async Task GetIsolateDispatchRecordCountAsync_ValidIsolateId_ReturnsCorrectCount()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var expectedCount = 5;
            _isolateDispatchRepository.GetIsolateDispatchRecordCountAsync(isolateId).Returns(expectedCount);

            // Act
            var result = await _service.GetIsolateDispatchRecordCountAsync(isolateId);

            // Assert
            Assert.Equal(expectedCount, result);
            await _isolateDispatchRepository.Received(1).GetIsolateDispatchRecordCountAsync(isolateId);
        }

        [Fact]
        public async Task GetIsolateDispatchRecordCountAsync_EmptyGuid_ReturnsZero()
        {
            // Arrange
            var emptyGuid = Guid.Empty;
            _isolateDispatchRepository.GetIsolateDispatchRecordCountAsync(emptyGuid).Returns(0);

            // Act
            var result = await _service.GetIsolateDispatchRecordCountAsync(emptyGuid);

            // Assert
            Assert.Equal(0, result);
            await _isolateDispatchRepository.Received(1).GetIsolateDispatchRecordCountAsync(emptyGuid);
        }

        [Fact]
        public async Task GetIsolateDispatchRecordCountAsync_RepositoryThrowsException_ThrowsSameException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var expectedException = new Exception("Repository error");
            _isolateDispatchRepository.GetIsolateDispatchRecordCountAsync(isolateId).Throws(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetIsolateDispatchRecordCountAsync(isolateId));
            await _isolateDispatchRepository.Received(1).GetIsolateDispatchRecordCountAsync(isolateId);
        }
    }
}
