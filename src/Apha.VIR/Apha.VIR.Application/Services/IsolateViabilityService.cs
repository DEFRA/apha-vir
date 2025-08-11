using System.Text;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class IsolateViabilityService : IIsolateViabilityService
    {
        private readonly IIsolateViabilityRepository _isolateViabilityRepository;
        private readonly IIsolateRepository _iIsolateRepository;
        private readonly ICharacteristicRepository _iCharacteristicRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IMapper _mapper;

        public IsolateViabilityService(IIsolateViabilityRepository isolateViabilityRepository,
            IIsolateRepository iIsolateRepository,
            ICharacteristicRepository iCharacteristicRepository,
            ILookupRepository lookupRepository,
            IMapper mapper)
        {
            _isolateViabilityRepository = isolateViabilityRepository ?? throw new ArgumentNullException(nameof(isolateViabilityRepository));
            _iIsolateRepository = iIsolateRepository ?? throw new ArgumentNullException(nameof(iIsolateRepository));
            _iCharacteristicRepository = iCharacteristicRepository ?? throw new ArgumentNullException(nameof(iCharacteristicRepository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<IsolateViabilityInfoDTO>> GetViabilityHistoryAsync(string AVNumber, Guid IsolateId)
        {
            var isolationList = await _iIsolateRepository.GetIsolateInfoByAVNumberAsync(AVNumber);

            var matchIsolate = isolationList.Where(x => x.IsolateId == IsolateId).ToList();

            if (matchIsolate.Count == 0)
            {
                return Enumerable.Empty<IsolateViabilityInfoDTO>();
            }

            var matchIsolateId = matchIsolate[0].IsolateId;


            var result = await _isolateViabilityRepository.GetViabilityHistoryAsync(matchIsolateId);

            var viabilityHistorList = _mapper.Map<IEnumerable<IsolateViabilityInfo>>(result);

            var characteristicList = await _iCharacteristicRepository.GetIsolateCharacteristicInfoAsync(matchIsolateId);

            var charNomenclature = GetCharacteristicNomenclature(characteristicList.ToList());

            var nomenclature = string.IsNullOrEmpty(charNomenclature) ? matchIsolate[0].Nomenclature : charNomenclature;
            var staffs = await _lookupRepository.GetAllStaffAsync();
            var Viabilities = await _lookupRepository.GetAllViabilityAsync();

            GetNomenclature(viabilityHistorList, nomenclature, AVNumber);
            GetCheckedByName(viabilityHistorList, staffs);

            GetViableName(viabilityHistorList, Viabilities);

            return _mapper.Map<IEnumerable<IsolateViabilityInfoDTO>>(viabilityHistorList);
        }

        public async Task DeleteIsolateViabilityAsync(Guid IsolateId, byte[] lastModified, string userid)
        {
            await _isolateViabilityRepository.DeleteIsolateViabilityAsync(IsolateId, lastModified, userid);
        }

        public async Task UpdateIsolateViabilityAsync(IsolateViabilityInfoDTO isolateViability, string userid)
        {
            ArgumentNullException.ThrowIfNull(isolateViability);

            if (string.IsNullOrWhiteSpace(userid)) throw new ArgumentException("User ID cannot be empty.", nameof(userid));

            var result = _mapper.Map<IsolateViability>(isolateViability);

            await _isolateViabilityRepository.UpdateIsolateViabilityAsync(result, userid);
        }

        private static string GetCharacteristicNomenclature(IList<IsolateCharacteristicInfo> characteristicList)
        {
            var characteristicNomenclatureList = new StringBuilder();

            // Build nomenclature string from characteristics
            foreach (IsolateCharacteristicInfo item in characteristicList)
            {
                if ((item.CharacteristicDisplay == true) && (!string.IsNullOrEmpty(item.CharacteristicValue)))
                {
                    characteristicNomenclatureList.Append(item.CharacteristicPrefix + item.CharacteristicValue + " ");
                }
            }

            var characteristicNomenclature = characteristicNomenclatureList.ToString().Trim();

            return characteristicNomenclature;
        }

        private static void GetNomenclature(IEnumerable<IsolateViabilityInfo> viabilityHistorList, string nomenclature, string AVNumber)
        {
            foreach (var vh in viabilityHistorList)
            {
                vh.Nomenclature = nomenclature;
                vh.AVNumber = AVNumber;
            }
        }

        private static void GetCheckedByName(IEnumerable<IsolateViabilityInfo> viabilityHistorList, IEnumerable<LookupItem>? staffs)
        {
            foreach (var viability in viabilityHistorList)
            {
                if (viability.CheckedById != Guid.Empty)
                {
                    viability.CheckedByName = staffs?.FirstOrDefault(s => s.Id == viability.CheckedById)?.Name!;
                }
            }
        }

        private static void GetViableName(IEnumerable<IsolateViabilityInfo> viabilityHistorList, IEnumerable<LookupItem>? viabilities)
        {
            foreach (var viability in viabilityHistorList)
            {
                if (viability.Viable != Guid.Empty)
                {
                    viability.ViableName = viabilities?.FirstOrDefault(v => v.Id == viability.Viable)?.Name!;
                }
            }
        }
    }
}
