using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Repositories;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.IsolatesServiceTest
{
    public class UniqueNomenclatureTests
    {
        private readonly IIsolateRepository _mockIsolateRepository;
        private readonly ISubmissionRepository _mockSubmissionRepository;
        private readonly ISampleRepository _mockSampleRepository;
        private readonly ICharacteristicRepository _mockCharacteristicRepository;
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly IsolatesService _mockIsolatesService;

        public UniqueNomenclatureTests()
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
        public async Task UniqueNomenclatureAsync_WhenIsolatesAreNullOrEmpty_ReturnsTrue()
        {
            // Arrange
            _mockIsolateRepository.GetIsolateForNomenclatureAsync(Arg.Any<Guid>()).Returns(Task.FromResult<IEnumerable<IsolateNomenclature>>(null!));

            // Act
            var result = await _mockIsolatesService.UniqueNomenclatureAsync(Guid.NewGuid(), "TestFamily", Guid.NewGuid());

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UniqueNomenclatureAsync_WhenFamilyNameIsParamyxoviridaeAndTypeMatches_ReturnsFalse()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var type = Guid.NewGuid();
            var isolates = new List<IsolateNomenclature>
            {
                new IsolateNomenclature { IsolateId = isolateId, VirusType = type }
            };
            _mockIsolateRepository.GetIsolateForNomenclatureAsync(Arg.Any<Guid>()).Returns(Task.FromResult(isolates.AsEnumerable()));

            // Act
            var result = await _mockIsolatesService.UniqueNomenclatureAsync(isolateId, "Paramyxoviridae", type);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UniqueNomenclatureAsync_WhenFamilyNameIsParamyxoviridaeAndTypeDoesNotMatch_ReturnsTrue()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var type = Guid.NewGuid();
            var isolates = new List<IsolateNomenclature>
            {
                new IsolateNomenclature { IsolateId = isolateId, VirusType = Guid.NewGuid() }
            };
            _mockIsolateRepository.GetIsolateForNomenclatureAsync(Arg.Any<Guid>()).Returns(Task.FromResult(isolates.AsEnumerable()));

            // Act
            var result = await _mockIsolatesService.UniqueNomenclatureAsync(isolateId, "Paramyxoviridae", type);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UniqueNomenclatureAsync_WhenFamilyNameIsNotParamyxoviridaeAndCharacteristicNomenclaturesMatch_ReturnsFalse()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var isolates = new List<IsolateNomenclature>
            {
                new IsolateNomenclature { IsolateId = Guid.NewGuid() }
            };
            _mockIsolateRepository.GetIsolateForNomenclatureAsync(Arg.Any<Guid>()).Returns(Task.FromResult(isolates.AsEnumerable()));
            _mockCharacteristicRepository.GetIsolateCharacteristicInfoAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<IsolateCharacteristicInfo>().AsEnumerable()));

            // Act
            var result = await _mockIsolatesService.UniqueNomenclatureAsync(isolateId, "OtherFamily", Guid.NewGuid());

            // Assert
            Assert.False(result);
        }        
    }
}
