using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Application.Validation;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.IsolateDispatchServiceTest
{
    public class IsolateDispatchServiceTests
    {
        private readonly IIsolateDispatchRepository _isolateDispatchRepository;
        private readonly IIsolateRepository _isolateRepository;
        private readonly ICharacteristicRepository _characteristicRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IMapper _mapper;
        private readonly IsolateDispatchService _service;
        private readonly IIsolateViabilityRepository _isolateViabilityRepository;

        public IsolateDispatchServiceTests()
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
        public async Task GetDispatchesHistoryAsync_ValidInputs_ReturnsDispatches()
        {
            // Arrange
            var avNumber = "AV001";
            var isolateId = Guid.NewGuid();
            var isolateInfo = new List<IsolateInfo> { new IsolateInfo { IsolateId = isolateId, Nomenclature = "Test Nomenclature" } };
            var dispatchHistory = new List<IsolateDispatchInfo> { new IsolateDispatchInfo { DispatchId = Guid.NewGuid() } };
            var characteristicInfo = new List<IsolateCharacteristicInfo>();
            var staffList = new List<LookupItem>();
            var workgroupList = new List<LookupItem>();

            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolateInfo);
            _isolateDispatchRepository.GetDispatchesHistoryAsync(isolateId).Returns(dispatchHistory);
            _characteristicRepository.GetIsolateCharacteristicInfoAsync(isolateId).Returns(characteristicInfo);
            _lookupRepository.GetAllStaffAsync().Returns(staffList);
            _lookupRepository.GetAllWorkGroupsAsync().Returns(workgroupList);
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

        [Fact]
        public async Task UpdateDispatchAsync_SuccessfulUpdate_CallsRepositoryMethod()
        {
            // Arrange
            var dto = new IsolateDispatchInfoDTO { DispatchId = Guid.NewGuid() };
            var user = "TestUser";
            var entity = new IsolateDispatchInfo { DispatchId = dto.DispatchId };
            _mapper.Map<IsolateDispatchInfo>(dto).Returns(entity);

            // Act
            await _service.UpdateDispatchAsync(dto, user);

            // Assert
            await _isolateDispatchRepository.Received(1).UpdateDispatchAsync(Arg.Is<IsolateDispatchInfo>(i => i.DispatchId == dto.DispatchId), user);
        }

        [Fact]
        public async Task UpdateDispatchAsync_NullDispatchInfoDto_ThrowsArgumentNullException()
        {
            // Arrange
            IsolateDispatchInfoDTO? dto = null;
            var user = "TestUser";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateDispatchAsync(dto!, user));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public async Task UpdateDispatchAsync_EmptyUser_ThrowsArgumentException(string user)
        {
            // Arrange
            var dto = new IsolateDispatchInfoDTO { DispatchId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateDispatchAsync(dto, user));
        }

        [Fact]
        public async Task UpdateDispatchAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new IsolateDispatchInfoDTO { DispatchId = Guid.NewGuid() };
            var user = "TestUser";
            var entity = new IsolateDispatchInfo { DispatchId = dto.DispatchId };
            _mapper.Map<IsolateDispatchInfo>(dto).Returns(entity);
            _isolateDispatchRepository.UpdateDispatchAsync(Arg.Any<IsolateDispatchInfo>(), Arg.Any<string>())
            .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdateDispatchAsync(dto, user));
        }

        [Fact]
        public async Task GetDispatchForIsolateAsync_WithValidInputs_ReturnsIsolateDispatchInfoDTO()
        {
            // Arrange
            string avNumber = "AV123";
            Guid dispatchId = Guid.NewGuid();
            Guid dispatchIsolateId = Guid.NewGuid();

            var isolateInfo = new IsolateInfo { IsolateId = dispatchIsolateId };
            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfo> { isolateInfo });

            var dispatchInfo = new IsolateDispatchInfo { DispatchId = dispatchId, RecipientId = Guid.NewGuid(), DispatchedById = Guid.NewGuid() };
            _isolateDispatchRepository.GetDispatchesHistoryAsync(dispatchIsolateId).Returns(new List<IsolateDispatchInfo> { dispatchInfo });

            _lookupRepository.GetAllStaffAsync().Returns(new List<LookupItem> { new LookupItem { Id = dispatchInfo.DispatchedById.Value, Name = "John Doe" } });
            _lookupRepository.GetAllWorkGroupsAsync().Returns(new List<LookupItem> { new LookupItem { Id = dispatchInfo.RecipientId.Value, Name = "Test Workgroup" } });

            _lookupRepository.GetAllViabilityAsync().Returns(new List<LookupItem> { new LookupItem { Id = Guid.NewGuid(), Name = "Viable" } });

            var expectedDto = new IsolateDispatchInfoDTO { DispatchId = dispatchId };
            _mapper.Map<IsolateDispatchInfoDTO>(Arg.Any<IsolateDispatchInfo>()).Returns(expectedDto);

            // Act
            var result = await _service.GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.DispatchId, result.DispatchId);
        }

        [Fact]
        public async Task GetDispatchForIsolateAsync_WithNoMatchingIsolate_ReturnsNull()
        {
            // Arrange
            string avNumber = "AV123";
            Guid dispatchId = Guid.NewGuid();
            Guid dispatchIsolateId = Guid.NewGuid();

            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfo>());

            // Act
            var result = await _service.GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDispatchForIsolateAsync_WithNoMatchingDispatch_ReturnsNull()
        {
            // Arrange
            string avNumber = "AV123";
            Guid dispatchId = Guid.NewGuid();
            Guid dispatchIsolateId = Guid.NewGuid();

            var isolateInfo = new IsolateInfo { IsolateId = dispatchIsolateId };
            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfo> { isolateInfo });

            _isolateDispatchRepository.GetDispatchesHistoryAsync(dispatchIsolateId).Returns(new List<IsolateDispatchInfo>());

            // Act
            var result = await _service.GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDispatchForIsolateAsync_WithNullOrEmptyAVNumber_ThrowsArgumentException()
        {
            // Arrange
            Guid dispatchId = Guid.NewGuid();
            Guid dispatchIsolateId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetDispatchForIsolateAsync(null!, dispatchId, dispatchIsolateId));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetDispatchForIsolateAsync(string.Empty, dispatchId, dispatchIsolateId));
        }

        [Fact]
        public async Task GetDispatchForIsolateAsync_WithEmptyDispatchId_ThrowsArgumentException()
        {
            // Arrange
            string avNumber = "AV123";
            Guid dispatchId = Guid.Empty;
            Guid dispatchIsolateId = Guid.NewGuid();

            var isolateInfo = new IsolateInfo { IsolateId = dispatchIsolateId };
            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(new List<IsolateInfo> { isolateInfo });

            _isolateDispatchRepository.GetDispatchesHistoryAsync(dispatchIsolateId).Returns(new List<IsolateDispatchInfo>());

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId));
            Assert.Equal("DispatchId cannot be empty. (Parameter 'DispatchId')", ex.Message);
        }

        [Fact]
        public async Task GetDispatchForIsolateAsync_WithEmptyDispatchIsolateId_ReturnsNull()
        {
            // Arrange
            string avNumber = "AV123";
            Guid dispatchId = Guid.NewGuid();
            Guid dispatchIsolateId = Guid.Empty;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId));
            Assert.Equal("DispatchIsolateId cannot be empty. (Parameter 'DispatchIsolateId')", ex.Message);
        }

        [Fact]
        public async Task GetIsolateInfoByAVNumberAndIsolateIdAsync_SuccessfulRetrieval_ReturnsCorrectDTO()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();
            var isolateInfo = new IsolateInfo { IsolateId = isolateId, AvNumber = avNumber };
            var isolateInfoDTO = new IsolateInfoDTO { IsolateId = isolateId, Avnumber = avNumber };

            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber)
            .Returns(new List<IsolateInfo> { isolateInfo });
            _mapper.Map<IsolateInfoDTO>(isolateInfo).Returns(isolateInfoDTO);

            // Act
            var result = await _service.GetIsolateInfoByAVNumberAndIsolateIdAsync(avNumber, isolateId);

            // Assert
            Assert.Equal(isolateInfoDTO, result);
            await _isolateRepository.Received(1).GetIsolateInfoByAVNumberAsync(avNumber);
            _mapper.Received(1).Map<IsolateInfoDTO>(isolateInfo);
        }

        [Fact]
        public async Task GetIsolateInfoByAVNumberAndIsolateIdAsync_EmptyResult_ReturnsNull()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber)
            .Returns(new List<IsolateInfo>());

            // Act
            var result = await _service.GetIsolateInfoByAVNumberAndIsolateIdAsync(avNumber, isolateId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetIsolateInfoByAVNumberAndIsolateIdAsync_RepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber)
            .Returns(new List<IsolateInfo>());

            // Act
            var result = await _service.GetIsolateInfoByAVNumberAndIsolateIdAsync(avNumber, isolateId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetIsolateInfoByAVNumberAndIsolateIdAsync_InvalidIsolateId_ReturnsNull()
        {
            // Arrange
            var avNumber = "AV123";
            var invalidIsolateId = Guid.NewGuid();
            var isolateInfo = new IsolateInfo { IsolateId = Guid.NewGuid(), AvNumber = avNumber };

            _isolateRepository.GetIsolateInfoByAVNumberAsync(avNumber)
            .Returns(new List<IsolateInfo> { isolateInfo });

            // Act
            var result = await _service.GetIsolateInfoByAVNumberAndIsolateIdAsync(avNumber, invalidIsolateId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddDispatchAsync_SuccessfulAddition_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var dispatchInfoDto = new IsolateDispatchInfoDTO();
            var dispatchInfo = new IsolateDispatchInfo();
            var user = "testUser";

            _mapper.Map<IsolateDispatchInfo>(dispatchInfoDto).Returns(dispatchInfo);

            // Act
            await _service.AddDispatchAsync(dispatchInfoDto, user);

            // Assert
            await _isolateDispatchRepository.Received(1).AddDispatchAsync(dispatchInfo, user);
        }

        [Fact]
        public async Task AddDispatchAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dispatchInfoDto = new IsolateDispatchInfoDTO();
            var dispatchInfo = new IsolateDispatchInfo();
            var user = "testUser";
            var expectedException = new Exception("Repository error");

            _mapper.Map<IsolateDispatchInfo>(dispatchInfoDto).Returns(dispatchInfo);
            _isolateDispatchRepository.AddDispatchAsync(dispatchInfo, user).Throws(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<Exception>(() =>
            _service.AddDispatchAsync(dispatchInfoDto, user));
            Assert.Equal(expectedException.Message, actualException.Message);
        }

        [Fact]
        public async Task GetLastViabilityByIsolateAsync_WithValidIsolateId_ReturnsLastViability()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var viabilityList = new List<IsolateViability>
{
new IsolateViability { DateChecked = DateTime.Now.AddDays(-2) },
new IsolateViability { DateChecked = DateTime.Now },
new IsolateViability { DateChecked = DateTime.Now.AddDays(-1) }
};

            _isolateViabilityRepository.GetViabilityByIsolateIdAsync(isolateId)
            .Returns(viabilityList);

            var expectedViability = viabilityList.OrderByDescending(v => v.DateChecked).First();
            var expectedDto = new IsolateViabilityDTO();

            _mapper.Map<IsolateViabilityDTO>(expectedViability).Returns(expectedDto);

            // Act
            var result = await _service.GetLastViabilityByIsolateAsync(isolateId);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedDto, result);
            await _isolateViabilityRepository.Received(1).GetViabilityByIsolateIdAsync(isolateId);
            _mapper.Received(1).Map<IsolateViabilityDTO>(expectedViability);
        }

        [Fact]
        public async Task GetLastViabilityByIsolateAsync_WithValidIsolateIdNoRecords_ReturnsNull()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            _isolateViabilityRepository.GetViabilityByIsolateIdAsync(isolateId)
            .Returns(new List<IsolateViability>());

            // Act
            var result = await _service.GetLastViabilityByIsolateAsync(isolateId);

            // Assert
            Assert.Null(result);
            await _isolateViabilityRepository.Received(1).GetViabilityByIsolateIdAsync(isolateId);
            _mapper.DidNotReceive().Map<IsolateViabilityDTO>(Arg.Any<IsolateViability>());
        }

        [Fact]
        public async Task GetLastViabilityByIsolateAsync_WithEmptyGuid_ThrowsArgumentException()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetLastViabilityByIsolateAsync(emptyGuid));

            await _isolateViabilityRepository.DidNotReceive().GetViabilityByIsolateIdAsync(Arg.Any<Guid>());
        }
    }
}
