using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.IsolateViabilityServiceTest
{
    public class AbstractIsolateViabilityServiceTest
    {
        protected IIsolateViabilityService _isolateViabilityService;
        protected IIsolateViabilityRepository _mockIsolateViabilityRepository;
        protected IIsolateRepository _mockIsolateRepository;
        protected ICharacteristicRepository _mockCharacteristicRepository;
        protected ILookupRepository _mockLookupRepository;
        protected IMapper _mockMapper;

        public AbstractIsolateViabilityServiceTest()
        {
            _mockIsolateViabilityRepository = Substitute.For<IIsolateViabilityRepository>();
            _mockIsolateRepository = Substitute.For<IIsolateRepository>();
            _mockCharacteristicRepository = Substitute.For<ICharacteristicRepository>();
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();

            _isolateViabilityService = new IsolateViabilityService(
            _mockIsolateViabilityRepository,
            _mockIsolateRepository,
            _mockCharacteristicRepository,
            _mockLookupRepository,
            _mockMapper);
        }

        public void GetViabilityHistoryAsyncSuccessfulRetrievalArrange(string avNumber, Guid isolateId)
        {
            var isolateList = new List<IsolateInfo> { new IsolateInfo { IsolateId = isolateId, Nomenclature = "Test Nomenclature" } };
            var viabilityHistory = new List<IsolateViability> { new IsolateViability { IsolateViabilityIsolateId = isolateId } };
            var characteristicList = new List<IsolateCharacteristicInfo>();
            var staffList = new List<LookupItem> { new LookupItem { Id = Guid.NewGuid(), Name = "John Doe" } };
            var viabilityList = new List<LookupItem> { new LookupItem { Id = Guid.NewGuid(), Name = "Viable" } };

            _mockIsolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolateList);
            _mockIsolateViabilityRepository.GetViabilityHistoryAsync(isolateId).Returns(viabilityHistory);
            _mockCharacteristicRepository.GetIsolateCharacteristicInfoAsync(isolateId).Returns(characteristicList);
            _mockLookupRepository.GetAllStaffAsync().Returns(staffList);
            _mockLookupRepository.GetAllViabilityAsync().Returns(viabilityList);

            _mockMapper.Map<IEnumerable<IsolateViabilityInfo>>(Arg.Any<IEnumerable<IsolateViability>>())
               .Returns(x => x.Arg<IEnumerable<IsolateViability>>().Select(i => new IsolateViabilityInfo()));

            _mockMapper.Map<IEnumerable<IsolateViabilityInfoDto>>(Arg.Any<IEnumerable<IsolateViabilityInfo>>())
                .Returns(x => x.Arg<IEnumerable<IsolateViabilityInfo>>().Select(i => new IsolateViabilityInfoDto()));

            _isolateViabilityService = new IsolateViabilityService(
            _mockIsolateViabilityRepository,
            _mockIsolateRepository,
            _mockCharacteristicRepository,
            _mockLookupRepository,
            _mockMapper);

        }

        public void GetViabilityHistoryAsyncSuccessfulRetrievalEnrichArrange(string avNumber, Guid isolateId)
        {
            var checkedById = Guid.NewGuid();
            var viableId = Guid.NewGuid();

            var isolateList = new List<IsolateInfo> { new IsolateInfo { IsolateId = isolateId, Nomenclature = "Test Nomenclature" } };
            var viabilityHistory = new List<IsolateViability>
                                    {
                                    new IsolateViability
                                        {
                                        IsolateViabilityIsolateId = isolateId,
                                        CheckedById = checkedById,
                                        Viable = viableId
                                    }
                                    };

            var characteristicList = new List<IsolateCharacteristicInfo>
                        { new IsolateCharacteristicInfo { CharacteristicDisplay = true, CharacteristicValue="Test",CharacteristicPrefix="P" }};

            var staffList = new List<LookupItem> { new LookupItem { Id = checkedById, Name = "John Doe" } };
            var viabilityList = new List<LookupItem> { new LookupItem { Id = viableId, Name = "Viable" } };

            _mockIsolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolateList);
            _mockIsolateViabilityRepository.GetViabilityHistoryAsync(isolateId).Returns(viabilityHistory);

            _mockCharacteristicRepository.GetIsolateCharacteristicInfoAsync(isolateId).Returns(characteristicList);
            _mockLookupRepository.GetAllStaffAsync().Returns(staffList);
            _mockLookupRepository.GetAllViabilityAsync().Returns(viabilityList);

            _mockMapper.Map<IEnumerable<IsolateViabilityInfo>>(Arg.Any<IEnumerable<IsolateViability>>())
              .Returns(x => x.Arg<IEnumerable<IsolateViability>>().Select(i => new IsolateViabilityInfo()));

            _mockMapper.Map<IEnumerable<IsolateViabilityInfoDto>>(Arg.Any<IEnumerable<IsolateViabilityInfo>>()).Returns(x => x.Arg<IEnumerable<IsolateViabilityInfo>>().Select(i => new IsolateViabilityInfoDto
            {
                Nomenclature = "PTest",
                CheckedByName = "John Doe",
                ViableName = "Viable"
            }));

            _isolateViabilityService = new IsolateViabilityService(
            _mockIsolateViabilityRepository,
            _mockIsolateRepository,
            _mockCharacteristicRepository,
            _mockLookupRepository,
            _mockMapper);
        }
    }
}
