using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.IsolatesServiceTest
{
    public class IsolatesServiceTests
    {
        private readonly IIsolateRepository _mockIsolateRepository;
        private readonly ISubmissionRepository _mockSubmissionRepository;
        private readonly ISampleRepository _mockSampleRepository;
        private readonly ICharacteristicRepository _mockCharacteristicRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly IsolatesService _isolatesService;

        public IsolatesServiceTests()
        {
            _mockIsolateRepository = Substitute.For<IIsolateRepository>();
            _mockSubmissionRepository = Substitute.For<ISubmissionRepository>();
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockCharacteristicRepository = Substitute.For<ICharacteristicRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _isolatesService = new IsolatesService(_mockIsolateRepository,
                _mockSubmissionRepository,
                _mockSampleRepository,
                _mockCharacteristicRepository,
                _mockLookupRepository,
                _mockMapper);
        }

        [Fact]
        public async Task GetIsolateFullDetailsAsync_SuccessfulRetrieval_ReturnsIsolateFullDetailDto()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var isolateFullDetail = new IsolateFullDetail
            {
                IsolateDetails = new IsolateInfo { IsolateId = isolateId },
                IsolateViabilityDetails = new List<IsolateViabilityInfo>(),
                IsolateDispatchDetails = new List<IsolateDispatchInfo>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfo>()
            };
            var expectedDto = new IsolateFullDetailDto
            {
                IsolateDetails = new IsolateInfoDto { IsolateId = isolateId },
                IsolateViabilityDetails = new List<IsolateViabilityInfoDto>(),
                IsolateDispatchDetails = new List<IsolateDispatchInfoDto>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfoDto>()
            };

            _mockIsolateRepository.GetIsolateFullDetailsByIdAsync(isolateId).Returns(isolateFullDetail);
            _mockMapper.Map<IsolateFullDetailDto>(isolateFullDetail).Returns(expectedDto);

            // Act
            var result = await _isolatesService.GetIsolateFullDetailsAsync(isolateId);

            // Assert
            Assert.Equal(expectedDto, result);
            await _mockIsolateRepository.Received(1).GetIsolateFullDetailsByIdAsync(isolateId);
            _mockMapper.Received(1).Map<IsolateFullDetailDto>(isolateFullDetail);
        }

        [Fact]
        public async Task GetIsolateFullDetailsAsync_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var expectedException = new Exception("Repository error");
            _mockIsolateRepository.GetIsolateFullDetailsByIdAsync(isolateId).Throws(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _isolatesService.GetIsolateFullDetailsAsync(isolateId));
            await _mockIsolateRepository.Received(1).GetIsolateFullDetailsByIdAsync(isolateId);
            _mockMapper.DidNotReceive().Map<IsolateFullDetailDto>(Arg.Any<IsolateFullDetail>());
        }

        [Fact]
        public async Task GetIsolateByIsolateAndAVNumberAsync_WhenFamilyNameIsParamyxoviridae_ShouldAppendTypeNameToNomenclature()
        {
            // Arrange
            var avNumber = "AV001";
            var isolateId = Guid.NewGuid();
            var isolate = new Isolate { FamilyName = "Paramyxoviridae", Nomenclature = "Test", TypeName = "Type1" };
            _mockIsolateRepository.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns(isolate);
            _mockMapper.Map<IsolateDto>(Arg.Any<Isolate>()).Returns(new IsolateDto { Nomenclature = isolate.Nomenclature + " (" + isolate.TypeName + ")" });

            // Act
            var result = await _isolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId);

            // Assert
            Assert.Equal("Test (Type1)", result.Nomenclature);
            await _mockIsolateRepository.Received(1).GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId);
        }

        [Fact]
        public async Task GetIsolateByIsolateAndAVNumberAsync_WhenFamilyNameIsNotParamyxoviridaeAndIsolateNomenclatureIsNotEmpty_ShouldNotModifyNomenclature()
        {
            // Arrange
            var avNumber = "AV002";
            var isolateId = Guid.NewGuid();
            var isolate = new Isolate { FamilyName = "OtherFamily", Nomenclature = "Test", IsolateNomenclature = "IsolateNomenclature" };
            _mockIsolateRepository.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns(isolate);
            _mockMapper.Map<IsolateDto>(Arg.Any<Isolate>()).Returns(new IsolateDto { Nomenclature = isolate.Nomenclature });

            // Act
            var result = await _isolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId);

            // Assert
            Assert.Equal("Test", result.Nomenclature);
            await _mockIsolateRepository.Received(1).GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId);
        }

        [Fact]
        public async Task GetIsolateByIsolateAndAVNumberAsync_WhenFamilyNameIsNotParamyxoviridaeAndIsolateNomenclatureIsEmpty_ShouldAppendCharacteristicNomenclature()
        {
            // Arrange
            var avNumber = "AV003";
            var isolateId = Guid.NewGuid();
            var isolate = new Isolate { FamilyName = "OtherFamily", Nomenclature = "Test", IsolateNomenclature = "" };
            var characteristics = new List<IsolateCharacteristicInfo> { new IsolateCharacteristicInfo { CharacteristicName = "Char1" } };
            _mockIsolateRepository.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns(isolate);
            _mockCharacteristicRepository.GetIsolateCharacteristicInfoAsync(isolateId).Returns(characteristics);
            _mockMapper.Map<IsolateDto>(Arg.Any<Isolate>()).Returns(new IsolateDto { Nomenclature = isolate.Nomenclature + " Char1" });

            // Act
            var result = await _isolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId);

            // Assert
            Assert.Equal("Test Char1", result.Nomenclature);
            await _mockIsolateRepository.Received(1).GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId);
            await _mockCharacteristicRepository.Received(1).GetIsolateCharacteristicInfoAsync(isolateId);
        }

        [Fact]
        public async Task GetIsolateByIsolateAndAVNumberAsync_WithInvalidInput_ShouldThrowException()
        {
            // Arrange
            var avNumber = "InvalidAV";
            var isolateId = Guid.NewGuid();
            _mockIsolateRepository.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId).Returns((Isolate)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _isolatesService.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId));
            await _mockIsolateRepository.Received(1).GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId);
        }

        [Fact]
        public async Task UpdateIsolateCharacteristicsAsync_SuccessfulUpdate_ReturnsCompletedTask()
        {
            // Arrange
            var IsolateCharacteristicInfoDto = new IsolateCharacteristicDto();
            var isolateCharacteristicInfo = new IsolateCharacteristicInfo();
            var user = "TestUser";

            _mockMapper.Map<IsolateCharacteristicInfo>(IsolateCharacteristicInfoDto).Returns(isolateCharacteristicInfo);
            _mockCharacteristicRepository.UpdateIsolateCharacteristicsAsync(isolateCharacteristicInfo, user).Returns(Task.CompletedTask);

            // Act
            await _isolatesService.UpdateIsolateCharacteristicsAsync(IsolateCharacteristicInfoDto, user);

            // Assert
            await _mockCharacteristicRepository.Received(1).UpdateIsolateCharacteristicsAsync(isolateCharacteristicInfo, user);
        }

        [Fact]
        public async Task UpdateIsolateCharacteristicsAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var IsolateCharacteristicInfoDto = new IsolateCharacteristicDto();
            var isolateCharacteristicInfo = new IsolateCharacteristicInfo();
            var user = "TestUser";
            var expectedException = new Exception("Test exception");

            _mockMapper.Map<IsolateCharacteristicInfo>(IsolateCharacteristicInfoDto).Returns(isolateCharacteristicInfo);
            _mockCharacteristicRepository.UpdateIsolateCharacteristicsAsync(isolateCharacteristicInfo, user).Throws(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<Exception>(() => _isolatesService.UpdateIsolateCharacteristicsAsync(IsolateCharacteristicInfoDto, user));
            Assert.Same(expectedException, actualException);
        }
    }
}
