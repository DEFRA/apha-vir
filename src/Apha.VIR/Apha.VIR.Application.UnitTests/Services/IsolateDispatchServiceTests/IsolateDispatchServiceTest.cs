using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Application.Validation;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.IsolateDispatchServiceTests
{
    public class IsolateDispatchServiceTest : IDisposable
    {
        private readonly IIsolateDispatchRepository _isolateDispatchRepository;
        private readonly IIsolateRepository _isolateRepository;
        private readonly ICharacteristicRepository _characteristicRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IWorkgroupRepository _workgroupRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IMapper _mapper;
        private readonly IsolateDispatchService _service;

        public IsolateDispatchServiceTest()
        {
            _isolateDispatchRepository = Substitute.For<IIsolateDispatchRepository>();
            _isolateRepository = Substitute.For<IIsolateRepository>();
            _characteristicRepository = Substitute.For<ICharacteristicRepository>();
            _staffRepository = Substitute.For<IStaffRepository>();
            _workgroupRepository = Substitute.For<IWorkgroupRepository>();
            _lookupRepository = Substitute.For<ILookupRepository>();
            _mapper = Substitute.For<IMapper>();

            _service = new IsolateDispatchService(
                _isolateDispatchRepository,
                _isolateRepository,
                _characteristicRepository,
                _staffRepository,
                _workgroupRepository,
                _lookupRepository,
                _mapper
            );
        }


        [Fact]
        public async Task GetDispatchesHistoryAsync_ValidInputs_ReturnsDispatches()
        {
            // Arrange
            var avNumber = "AV001";
            var isolateId = Guid.NewGuid();
            var isolateInfo = new List<IsolateInfo> { new IsolateInfo { IsolateId = isolateId, Nomenclature = "Test Nomenclature" } };
            var dispatchHistory = new List<IsolateDispatchInfo> { new IsolateDispatchInfo { DispatchId = Guid.NewGuid() } };
            var characteristicInfo = new List<IsolateCharacteristicInfo>();
            var staffList = new List<Staff>();
            var workgroupList = new List<Workgroup>();

            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolateInfo);
            _isolateDispatchRepository.GetDispatchesHistoryAsync(isolateId).Returns(dispatchHistory);
            _characteristicRepository.GetIsolateCharacteristicInfoAsync(isolateId).Returns(characteristicInfo);
            _staffRepository.GetStaffListAsync().Returns(staffList);
            _workgroupRepository.GetWorkgroupfListAsync().Returns(workgroupList);
            _mapper.Map<IEnumerable<IsolateDispatchInfoDTO>>(Arg.Any<IEnumerable<IsolateDispatchInfo>>()).Returns(x => x.Arg<IEnumerable<IsolateDispatchInfo>>().Select(d => new IsolateDispatchInfoDTO { DispatchId = d.DispatchId ?? Guid.Empty })); // Fix for CS0266 and CS8629

            // Act
            var result = await _service.GetDispatchesHistoryAsync(avNumber, isolateId);

            // Assert
            Assert.NotEmpty(result);
            Assert.Single(result);
            Assert.Equal(dispatchHistory[0].DispatchId, result.First().DispatchId);
        }

        [Fact]
        public async Task GetDispatchesHistoryAsync_ValidInputsNoDispatches_ReturnsEmptyList()
        {
            // Arrange
            var avNumber = "AV001";
            var isolateId = Guid.NewGuid();
            var isolateInfo = new List<IsolateInfo> { new IsolateInfo { IsolateId = isolateId } };
            var dispatchHistory = new List<IsolateDispatchInfo>();

            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolateInfo);
            _isolateDispatchRepository.GetDispatchesHistoryAsync(isolateId).Returns(dispatchHistory);
            _mapper.Map<IEnumerable<IsolateDispatchInfoDTO>>(Arg.Any<IEnumerable<IsolateDispatchInfo>>()).Returns(new List<IsolateDispatchInfoDTO>());

            // Act
            var result = await _service.GetDispatchesHistoryAsync(avNumber, isolateId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDispatchesHistoryAsync_InvalidAVNumber_ReturnsEmptyList()
        {
            // Arrange
            var avNumber = "InvalidAV";
            var isolateId = Guid.NewGuid();
            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfo>());
            _mapper.Map<IEnumerable<IsolateDispatchInfoDTO>>(Arg.Any<IEnumerable<IsolateDispatchInfoDTO>>()).Returns(new List<IsolateDispatchInfoDTO>());

            // Act
            var result = await _service.GetDispatchesHistoryAsync(avNumber, isolateId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDispatchesHistoryAsync_InvalidIsolateId_ReturnsEmptyList()
        {
            // Arrange
            var avNumber = "AV001";
            var invalidIsolateId = Guid.NewGuid();
            var isolateInfo = new List<IsolateInfo> { new IsolateInfo { IsolateId = Guid.NewGuid() } };
            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolateInfo);
            _isolateDispatchRepository.GetDispatchesHistoryAsync(invalidIsolateId).Returns(new List<IsolateDispatchInfo>());
            _mapper.Map<IEnumerable<IsolateDispatchInfoDTO>>(Arg.Any<IEnumerable<IsolateDispatchInfo>>()).Returns(new List<IsolateDispatchInfoDTO>());

            // Act
            var result = await _service.GetDispatchesHistoryAsync(avNumber, invalidIsolateId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDispatchesHistoryAsync_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var avNumber = "AV001";
            var isolateId = Guid.NewGuid();
            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetDispatchesHistoryAsync(avNumber, isolateId));
        }

        [Fact]
        public async Task DeleteDispatchAsync_WithValidInput_ShouldCallRepositoryMethod()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3, 4 };
            var user = "TestUser";

            // Act
            await _service.DeleteDispatchAsync(dispatchId, lastModified, user);

            // Assert
            await _isolateDispatchRepository.Received(1).DeleteDispatchAsync(dispatchId, lastModified, user);
        }

        [Fact]
        public async Task DeleteDispatchAsync_WithEmptyDispatchId_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyDispatchId = Guid.Empty;
            var lastModified = new byte[] { 1, 2, 3, 4 };
            var user = "TestUser";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.DeleteDispatchAsync(emptyDispatchId, lastModified, user));
        }

        [Fact]
        public async Task DeleteDispatchAsync_WithNullLastModified_ShouldThrowArgumentNullException()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            byte[]? lastModified = Array.Empty<byte>(); ;
            var user = "TestUser";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteDispatchAsync(dispatchId, lastModified!, user));
        }

        [Fact]
        public async Task DeleteDispatchAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3, 4 };
            var user = "TestUser";

            _isolateDispatchRepository.DeleteDispatchAsync(Arg.Any<Guid>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _service.DeleteDispatchAsync(dispatchId, lastModified, user));
        }

        [Fact]
        public async Task GetDispatcheConfirmationAsync_SuccessfulRetrieval_ReturnsIsolateFullDetailDTO()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var isolateFullDetail = new IsolateFullDetail
            {
                IsolateDetails = new IsolateInfo { NoOfAliquots = 5 },
                IsolateDispatchDetails = new List<IsolateDispatchInfo>(),
                IsolateViabilityDetails = new List<IsolateViabilityInfo>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfo>()
            };
            var expectedDto = new IsolateFullDetailDTO
            {
                IsolateDetails = new IsolateInfoDTO { NoOfAliquots = 5 },
                IsolateDispatchDetails = new List<IsolateDispatchInfoDTO>(),
                IsolateViabilityDetails = new List<IsolateViabilityInfoDTO>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfoDTO>()

            };


            _isolateRepository.GetIsolateFullDetailsByIdAsync(isolateId).Returns(isolateFullDetail);
            _mapper.Map<IsolateFullDetailDTO>(isolateFullDetail).Returns(expectedDto);

            // Act
            var result = await _service.GetDispatcheConfirmationAsync(isolateId);

            // Assert
            Assert.Equal(expectedDto, result);
            await _isolateRepository.Received(1).GetIsolateFullDetailsByIdAsync(isolateId);
            _mapper.Received(1).Map<IsolateFullDetailDTO>(isolateFullDetail);
        }

        [Fact]
        public async Task GetDispatcheConfirmationAsync_NullIsolateDetails_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();

            var isolateFullDetail = new IsolateFullDetail
            {
                IsolateDetails = null!,
                IsolateDispatchDetails = new List<IsolateDispatchInfo>(),
                IsolateViabilityDetails = new List<IsolateViabilityInfo>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfo>()
            };

            _isolateRepository.GetIsolateFullDetailsByIdAsync(isolateId).Returns(isolateFullDetail);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() =>
            _service.GetDispatcheConfirmationAsync(isolateId));
        }

        [Fact]
        public async Task GetDispatcheConfirmationAsync_ExceptionThrown_PropagatesException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            _isolateRepository.GetIsolateFullDetailsByIdAsync(isolateId).Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            _service.GetDispatcheConfirmationAsync(isolateId));
        }

        private string InvokeGetCharacteristicNomenclature(IList<IsolateCharacteristicInfo> characteristicList)
        {
            var methodInfo = typeof(IsolateDispatchService).GetMethod("GetCharacteristicNomenclature", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (methodInfo == null)
                throw new InvalidOperationException("GetCharacteristicNomenclature method not found.");
            var result = methodInfo.Invoke(_service, new object[] { characteristicList });
            return result as string ?? string.Empty;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }
}
