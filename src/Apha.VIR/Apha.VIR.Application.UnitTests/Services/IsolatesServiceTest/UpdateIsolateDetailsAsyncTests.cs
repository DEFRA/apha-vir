using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.IsolatesServiceTest
{
    public class UpdateIsolateDetailsAsyncTests
    {
        private readonly IIsolateRepository _mockIsolateRepository;
        private readonly ISubmissionRepository _mockSubmissionRepository;
        private readonly ISampleRepository _mockSampleRepository;
        private readonly ICharacteristicRepository _mockCharacteristicRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly IsolatesService _mockIsolatesService;

        public UpdateIsolateDetailsAsyncTests()
        {
            _mockIsolateRepository = Substitute.For<IIsolateRepository>();
            _mockSubmissionRepository = Substitute.For<ISubmissionRepository>();
            _mockSampleRepository = Substitute.For<ISampleRepository>();
            _mockCharacteristicRepository = Substitute.For<ICharacteristicRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockIsolatesService = new IsolatesService(_mockIsolateRepository, 
                _mockSubmissionRepository, 
                _mockSampleRepository, 
                _mockCharacteristicRepository,
                _mockLookupRepository,
                _mockMapper);
        }

        [Fact]
        public async Task Test_UpdateIsolateDetailsAsync_Success()
        {
            // Arrange
            var IsolateDto = new IsolateDto { IsolateId = Guid.NewGuid() };
            var isolateEntity = new Isolate { IsolateId = IsolateDto.IsolateId };

            _mockMapper.Map<Isolate>(IsolateDto).Returns(isolateEntity);

            // Act
            await _mockIsolatesService.UpdateIsolateDetailsAsync(IsolateDto);

            // Assert
            await _mockIsolateRepository.Received(1).UpdateIsolateDetailsAsync(Arg.Is<Isolate>(i => i.IsolateId == IsolateDto.IsolateId));
        }

        [Fact]
        public async Task Test_UpdateIsolateDetailsAsync_MappingFailure()
        {
            // Arrange
            var IsolateDto = new IsolateDto { IsolateId = Guid.NewGuid() };
            _mockMapper.Map<Isolate>(IsolateDto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _mockIsolatesService.UpdateIsolateDetailsAsync(IsolateDto));
        }

        [Fact]
        public async Task GenerateNomenclature_ValidInput_ReturnsCorrectNomenclature()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var virusType = "H5N1";
            var yearOfIsolation = "2023";

            var submission = new Submission { SubmissionId = Guid.NewGuid(), CountryOfOriginName = "UK" };
            var sample = new Sample { SampleId = sampleId, SenderReferenceNumber = "SRN001" };

            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(new List<Sample> { sample });

            // Act
            var result = await _mockIsolatesService.GenerateNomenclature(avNumber, sampleId, virusType, yearOfIsolation);

            // Assert
            Assert.Equal("H5N1//UK/SRN001/2023", result);
        }

        [Fact]
        public async Task GenerateNomenclature_MissingVirusType_ReturnsNomenclatureWithPlaceholder()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            string? virusType = null;
            var yearOfIsolation = "2023";

            var submission = new Submission { SubmissionId = Guid.NewGuid(), CountryOfOriginName = "UK" };
            var sample = new Sample { SampleId = sampleId, SenderReferenceNumber = "SRN001" };

            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(new List<Sample> { sample });

            // Act
            var result = await _mockIsolatesService.GenerateNomenclature(avNumber, sampleId, virusType!, yearOfIsolation);

            // Assert
            Assert.Equal("[Virus Type]//UK/SRN001/2023", result);
        }

        [Fact]
        public async Task GenerateNomenclature_MissingYearOfIsolation_ReturnsNomenclatureWithPlaceholder()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var virusType = "H5N1";
            string? yearOfIsolation = null;

            var submission = new Submission { SubmissionId = Guid.NewGuid(), CountryOfOriginName = "UK" };
            var sample = new Sample { SampleId = sampleId, SenderReferenceNumber = "SRN001" };

            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(new List<Sample> { sample });

            // Act
            var result = await _mockIsolatesService.GenerateNomenclature(avNumber, sampleId, virusType, yearOfIsolation!);

            // Assert
            Assert.Equal("H5N1//UK/SRN001/[Year of Isolation]", result);
        }

        [Fact]
        public async Task GenerateNomenclature_NullSampleData_ReturnsPartialNomenclature()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var virusType = "H5N1";
            var yearOfIsolation = "2023";

            var submission = new Submission { SubmissionId = Guid.NewGuid(), CountryOfOriginName = "UK" };

            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns(submission);
            _mockSampleRepository.GetSamplesBySubmissionIdAsync(submission.SubmissionId).Returns(new List<Sample>());

            // Act
            var result = await _mockIsolatesService.GenerateNomenclature(avNumber, sampleId, virusType, yearOfIsolation);

            // Assert
            Assert.Equal("H5N1//UK//2023", result);
        }

        [Fact]
        public async Task GenerateNomenclature_NullSubmissionData_ThrowsNullReferenceException()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var virusType = "H5N1";
            var yearOfIsolation = "2023";

            _mockSubmissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber).Returns((Submission)null!);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() =>
            _mockIsolatesService.GenerateNomenclature(avNumber, sampleId, virusType, yearOfIsolation));
        }
    }
}
