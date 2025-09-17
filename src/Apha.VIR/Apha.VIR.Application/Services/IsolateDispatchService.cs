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

        public async Task<IsolateInfoDto> GetIsolateInfoByAVNumberAndIsolateIdAsync(string AVNumber, Guid IsolateId)
        {
            var isolationList = await _iIsolateRepository.GetIsolateInfoByAVNumberAsync(AVNumber);
            var isolateInfo = isolationList.FirstOrDefault(x => x.IsolateId == IsolateId);
            return _mapper.Map<IsolateInfoDto>(isolateInfo);
        }

        public async Task<IEnumerable<IsolateDispatchInfoDto>> GetDispatchesHistoryAsync(string AVNumber, Guid IsolateId)
        {
            string nomenclature;

            var isolationList = await _iIsolateRepository.GetIsolateInfoByAVNumberAsync(AVNumber);

            var matchIsolate = isolationList.Where(x => x.IsolateId == IsolateId).ToList();

            if ((matchIsolate.Count == 0))
            {
                return _mapper.Map<IEnumerable<IsolateDispatchInfoDto>>(Enumerable.Empty<IsolateDispatchInfoDto>());
            }

            var matchIsolateId = matchIsolate.First()?.IsolateId ?? Guid.Empty;

            var dispatchHistList = await _isolateDispatchRepository.GetDispatchesHistoryAsync(matchIsolateId);

            var characteristicList = await _iCharacteristicRepository.GetIsolateCharacteristicInfoAsync(matchIsolateId);

            var charNomenclature = ServiceHelper.GetCharacteristicNomenclature(characteristicList.ToList());

            nomenclature = GetFullNomenclature(matchIsolate[0].Nomenclature, matchIsolate[0].IsolateNomenclature, matchIsolate[0].FamilyName, matchIsolate[0].TypeName, charNomenclature);

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

            return _mapper.Map<IEnumerable<IsolateDispatchInfoDto>>(dispatchHistList);
        }

        public async Task<IsolateFullDetailDto> GetDispatcheConfirmationAsync(Guid IsolateId)
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

            return _mapper.Map<IsolateFullDetailDto>(isolateFullDetail);
        }

        public async Task AddDispatchAsync(IsolateDispatchInfoDto DispatchInfo, string User)
        {
            var dispatchData = _mapper.Map<IsolateDispatchInfo>(DispatchInfo);
            await _isolateDispatchRepository.AddDispatchAsync(dispatchData, User);
        }

        public async Task UpdateDispatchAsync(IsolateDispatchInfoDto DispatchInfoDto, string User)
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

        public async Task<IsolateDispatchInfoDto> GetDispatchForIsolateAsync(string AVNumber, Guid DispatchId, Guid DispatchIsolateId)
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
                return _mapper.Map<IsolateDispatchInfoDto>(null);

            var matchIsolate = isolationList.Where(x => x.IsolateId == DispatchIsolateId).ToList();

            if (!matchIsolate.Any())
                return _mapper.Map<IsolateDispatchInfoDto>(null);

            var matchIsolateId = matchIsolate.First().IsolateId;

            var dispatchHistList = await _isolateDispatchRepository.GetDispatchesHistoryAsync(matchIsolateId);

            if (!(dispatchHistList?.Any() ?? false))
                return _mapper.Map<IsolateDispatchInfoDto>(null);

            var staffs = await _lookupRepository.GetAllStaffAsync();
            var workgroups = await _lookupRepository.GetAllWorkGroupsAsync();

            var dispatch = dispatchHistList.FirstOrDefault(d => d.DispatchId == DispatchId);
            if (dispatch == null)
                return _mapper.Map<IsolateDispatchInfoDto>(null);

            if (dispatch.RecipientId.HasValue)
            {
                dispatch.Recipient = workgroups?.FirstOrDefault(wg => wg.Id == dispatch.RecipientId.Value)?.Name;
            }

            if (dispatch.DispatchedById.HasValue)
            {
                dispatch.DispatchedByName = staffs?.FirstOrDefault(s => s.Id == dispatch.DispatchedById)?.Name;
            }

            IEnumerable<LookupItemDto> lookup = _mapper.Map<IEnumerable<LookupItemDto>>(await _lookupRepository.GetAllViabilityAsync());
            if (dispatch.ViabilityId.HasValue)
            {
                dispatch.ViabilityName = lookup.FirstOrDefault(x => x.Id == dispatch.ViabilityId)?.Name;
            }

            var lastViability = await GetLastViabilityByIsolateAsync(matchIsolateId);
            dispatch.ViabilityId = lastViability?.Viable;
            dispatch.IsolateNoOfAliquots = matchIsolate.First().NoOfAliquots;

            return _mapper.Map<IsolateDispatchInfoDto>(dispatch);
        }        

        public async Task<IsolateViabilityDto?> GetLastViabilityByIsolateAsync(Guid IsolateId)
        {
            if (IsolateId == Guid.Empty)
                throw new ArgumentException("ViabilityId cannot be empty.", nameof(IsolateId));

            var viabilityList = await _isolateViabilityRepository.GetViabilityByIsolateIdAsync(IsolateId);

            var lastViability = viabilityList
                .OrderByDescending(v => v.DateChecked)
                .FirstOrDefault();

            return lastViability == null ? null : _mapper.Map<IsolateViabilityDto>(lastViability);
        }

        public async Task<int> GetIsolateDispatchRecordCountAsync(Guid isolateId)
        {
            return await _isolateDispatchRepository.GetIsolateDispatchRecordCountAsync(isolateId);            
        }

        private static string GetFullNomenclature(string? nomenclature, string? isolateNomenclature, string? familyName, string? typeName, string charNomenclature)
        {
            if (familyName == "Paramyxoviridae")
                return (string.IsNullOrEmpty(nomenclature) ? "" : nomenclature) + " (" + (string.IsNullOrEmpty(typeName) ? "" : typeName) + ")";
            else
            {
                if (string.IsNullOrEmpty(isolateNomenclature))
                    return (string.IsNullOrEmpty(nomenclature) ? "" : nomenclature) + " " + charNomenclature;
                else
                    return (string.IsNullOrEmpty(nomenclature) ? "" : nomenclature);
            }
        }
    }
}
