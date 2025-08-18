using System.Text;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Validation;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class IsolateDispatchService : IIsolateDispatchService
    {
        private readonly IIsolateDispatchRepository _isolateDispatchRepository;
        private readonly IIsolateRepository _iIsolateRepository;
        private readonly ICharacteristicRepository _iCharacteristicRepository;
        private readonly IMapper _mapper;
        private readonly ILookupRepository _lookupRepository;
        private readonly IIsolateViabilityRepository _isolateViabilityRepository;


        public IsolateDispatchService(IIsolateDispatchRepository isolateDispatchRepository,
            IIsolateRepository iIsolateRepository,
            ICharacteristicRepository iCharacteristicRepository,
            ILookupRepository lookupRepository,
            IMapper mapper,
            IIsolateViabilityRepository isolateViabilityRepository)
        {
            _isolateDispatchRepository = isolateDispatchRepository ?? throw new ArgumentNullException(nameof(isolateDispatchRepository));
            _iIsolateRepository = iIsolateRepository ?? throw new ArgumentNullException(nameof(iIsolateRepository));
            _iCharacteristicRepository = iCharacteristicRepository ?? throw new ArgumentNullException(nameof(iCharacteristicRepository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _mapper = mapper;
            _isolateViabilityRepository = isolateViabilityRepository;
        }

        public async Task<IsolateInfoDTO> GetIsolateInfoByAVNumberAndIsolateIdAsync(string AVNumber, Guid IsolateId)
        {
            var isolationList = await _iIsolateRepository.GetIsolateInfoByAVNumberAsync(AVNumber);
            var isolateInfo = isolationList.FirstOrDefault(x => x.IsolateId == IsolateId);
            return _mapper.Map<IsolateInfoDTO>(isolateInfo);
        }

        public async Task<IEnumerable<IsolateDispatchInfoDTO>> GetDispatchesHistoryAsync(string AVNumber, Guid IsolateId)
        {
            string nomenclature;

            var isolationList = await _iIsolateRepository.GetIsolateInfoByAVNumberAsync(AVNumber);

            var matchIsolate = isolationList.Where(x => x.IsolateId == IsolateId).ToList();

            if ((matchIsolate.Count == 0))
            {
                return _mapper.Map<IEnumerable<IsolateDispatchInfoDTO>>(Enumerable.Empty<IsolateDispatchInfoDTO>());
            }

            var matchIsolateId = matchIsolate.First()?.IsolateId ?? Guid.Empty;

            var dispatchHistList = await _isolateDispatchRepository.GetDispatchesHistoryAsync(matchIsolateId);

            var characteristicList = await _iCharacteristicRepository.GetIsolateCharacteristicInfoAsync(matchIsolateId);

            var charNomenclature = GetCharacteristicNomenclature(characteristicList.ToList());

            if (string.IsNullOrEmpty(charNomenclature))
            {
                nomenclature = matchIsolate.First().Nomenclature ?? string.Empty;
            }
            else
            {
                nomenclature = charNomenclature!;
            }

            foreach (var item in dispatchHistList)
            {
                item.Nomenclature = nomenclature;
                item.IsolateNoOfAliquots = matchIsolate[0].NoOfAliquots;
            }

            var staffs = await _lookupRepository.GetAllStaffAsync();
            var workgroups = await _lookupRepository.GetAllWorkGroupsAsync();

            foreach (var dispatch in dispatchHistList)
            {
                if (dispatch.RecipientId.HasValue)
                {
                    dispatch.Recipient = workgroups?.FirstOrDefault(wg => wg.Id == dispatch.RecipientId.Value)?.Name;
                }
            }

            foreach (var dispatch in dispatchHistList)
            {
                if (dispatch.DispatchedById.HasValue)
                {
                    dispatch.DispatchedByName = staffs?.FirstOrDefault(s => s.Id == dispatch.DispatchedById)?.Name;
                }
            }

            return _mapper.Map<IEnumerable<IsolateDispatchInfoDTO>>(dispatchHistList);
        }

        public async Task<IsolateFullDetailDTO> GetDispatcheConfirmationAsync(Guid IsolateId)
        {
            var isolateFullDetail = await _iIsolateRepository.GetIsolateFullDetailsByIdAsync(IsolateId);

            if (isolateFullDetail.IsolateDetails == null)
            {
                var error = new BusinessValidationError(
                    message: "Problem with reading IsolateDetails.",
                    code: "ERR_ISOLATE");

                var errorResponse = new BusinessValidationErrorException([error]);

                throw errorResponse;
            }

            return _mapper.Map<IsolateFullDetailDTO>(isolateFullDetail);
        }

        public async Task AddDispatchAsync(IsolateDispatchInfoDTO DispatchInfo, string User)
        {
            var dispatchData = _mapper.Map<IsolateDispatchInfo>(DispatchInfo);
            await _isolateDispatchRepository.AddDispatchAsync(dispatchData, User);
        }

        public async Task UpdateDispatchAsync(IsolateDispatchInfoDTO DispatchInfoDto, string User)
        {
            if (DispatchInfoDto == null)
                throw new ArgumentNullException(nameof(DispatchInfoDto), "DispatchInfoDto cannot be null.");
            if (string.IsNullOrWhiteSpace(User))
                throw new ArgumentException("User cannot be empty.", nameof(User));

            IsolateDispatchInfo dispatchInfo = _mapper.Map<IsolateDispatchInfo>(DispatchInfoDto);
            await _isolateDispatchRepository.UpdateDispatchAsync(dispatchInfo, User);
        }

        public async Task DeleteDispatchAsync(Guid DispatchId, byte[] LastModified, string User)
        {
            if (DispatchId == Guid.Empty)
                throw new ArgumentException("DispatchId cannot be empty.", nameof(DispatchId));

            if (LastModified == Array.Empty<byte>())
                throw new ArgumentException("LastModified cannot be empty.", nameof(LastModified));

            if (string.IsNullOrWhiteSpace(User))
                throw new ArgumentException("User cannot be empty.", nameof(User));

            await _isolateDispatchRepository.DeleteDispatchAsync(DispatchId, LastModified, User);
        }

        public async Task<IsolateDispatchInfoDTO> GetDispatchForIsolateAsync(string AVNumber, Guid DispatchId, Guid DispatchIsolateId)
        {
            // Defensive checks for empty GUIDs and null/empty AVNumber
            if (DispatchId == Guid.Empty)
                throw new ArgumentException("DispatchId cannot be empty.", nameof(DispatchId));
            if (DispatchIsolateId == Guid.Empty)
                throw new ArgumentException("DispatchIsolateId cannot be empty.", nameof(DispatchIsolateId));
            if (string.IsNullOrWhiteSpace(AVNumber))
                throw new ArgumentException("AVNumber cannot be empty.", nameof(AVNumber));

            var isolationList = await _iIsolateRepository.GetIsolateInfoByAVNumberAsync(AVNumber);

            if (!(isolationList?.Any() ?? false))
                return _mapper.Map<IsolateDispatchInfoDTO>(null);

            var matchIsolate = isolationList.Where(x => x.IsolateId == DispatchIsolateId).ToList();

            if (!matchIsolate.Any())
                return _mapper.Map<IsolateDispatchInfoDTO>(null);

            var matchIsolateId = matchIsolate.First().IsolateId;

            var dispatchHistList = await _isolateDispatchRepository.GetDispatchesHistoryAsync(matchIsolateId);

            if (!(dispatchHistList?.Any() ?? false))
                return _mapper.Map<IsolateDispatchInfoDTO>(null);

            var staffs = await _lookupRepository.GetAllStaffAsync();
            var workgroups = await _lookupRepository.GetAllWorkGroupsAsync();

            var dispatch = dispatchHistList.FirstOrDefault(d => d.DispatchId == DispatchId);
            if (dispatch == null)
                return _mapper.Map<IsolateDispatchInfoDTO>(null);

            if (dispatch.RecipientId.HasValue)
            {
                dispatch.Recipient = workgroups?.FirstOrDefault(wg => wg.Id == dispatch.RecipientId.Value)?.Name;
            }

            if (dispatch.DispatchedById.HasValue)
            {
                dispatch.DispatchedByName = staffs?.FirstOrDefault(s => s.Id == dispatch.DispatchedById)?.Name;
            }

            IEnumerable<LookupItemDTO> lookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupRepository.GetAllViabilityAsync());
            if (dispatch.ViabilityId.HasValue)
            {
                dispatch.ViabilityName = lookup.FirstOrDefault(x => x.Id == dispatch.ViabilityId)?.Name;
            }

            var lastViability = await GetLastViabilityByIsolateAsync(matchIsolateId);
            dispatch.ViabilityId = lastViability?.Viable;

            return _mapper.Map<IsolateDispatchInfoDTO>(dispatch);
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

        public async Task<IsolateViabilityDTO?> GetLastViabilityByIsolateAsync(Guid IsolateId)
        {
            if (IsolateId == Guid.Empty)
                throw new ArgumentException("ViabilityId cannot be empty.", nameof(IsolateId));

            var viabilityList = await _isolateViabilityRepository.GetViabilityByIsolateIdAsync(IsolateId);

            var lastViability = viabilityList
                .OrderByDescending(v => v.DateChecked)
                .FirstOrDefault();

            return lastViability == null ? null : _mapper.Map<IsolateViabilityDTO>(lastViability);
        }
    }
}
