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
        private readonly IStaffRepository _staffRepository;
        private readonly IWorkgroupRepository _IWorkgroupRepository;
        private readonly IMapper _mapper;

        public IsolateDispatchService(IIsolateDispatchRepository isolateDispatchRepository,
            IIsolateRepository iIsolateRepository,
            ICharacteristicRepository iCharacteristicRepository,
            IStaffRepository staffRepository,
            IWorkgroupRepository workgroupRepository,
            ILookupRepository lookupRepository,
            IMapper mapper)
        {
            _isolateDispatchRepository = isolateDispatchRepository ?? throw new ArgumentNullException(nameof(isolateDispatchRepository));
            _iIsolateRepository = iIsolateRepository ?? throw new ArgumentNullException(nameof(iIsolateRepository));
            _iCharacteristicRepository = iCharacteristicRepository ?? throw new ArgumentNullException(nameof(iCharacteristicRepository));
            _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
            _IWorkgroupRepository = workgroupRepository ?? throw new ArgumentNullException(nameof(workgroupRepository));
            _mapper = mapper;
        }

        public async Task DeleteDispatchAsync(Guid DispatchId, byte[] LastModified, string User)
        {
            if (DispatchId == Guid.Empty)
                throw new ArgumentException("DispatchId cannot be empty.", nameof(DispatchId));

            if (LastModified == Array.Empty<byte>() )
                throw new ArgumentException( "LastModified cannot be empty.", nameof(LastModified));
            
            if (string.IsNullOrWhiteSpace(User))
                throw new ArgumentException("User cannot be empty.", nameof(User));

            await _isolateDispatchRepository.DeleteDispatchAsync(DispatchId, LastModified, User);
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

            var matchIsolateId = matchIsolate.First().IsolateId;

            var dispatchHistList = await _isolateDispatchRepository.GetDispatchesHistoryAsync(matchIsolateId);

            var characteristicList = await _iCharacteristicRepository.GetIsolateCharacteristicInfoAsync(matchIsolateId);

            var charNomenclature = GetCharacteristicNomenclature(characteristicList.ToList());

            if (string.IsNullOrEmpty(charNomenclature))
            {
                nomenclature = matchIsolate.First().Nomenclature != null
                    ? matchIsolate.First().Nomenclature!
                    : string.Empty;
            }
            else
            {
                nomenclature = charNomenclature!;
            }

            foreach (var item in dispatchHistList)
            {
                item.Nomenclature = nomenclature;
            }

            var staffs = await _staffRepository.GetStaffListAsync();

            var workgroups = await _IWorkgroupRepository.GetWorkgroupfListAsync();

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

            if ( isolateFullDetail.IsolateDetails == null)
            {
                var error = new BusinessValidationError(
                    message: "Problem with reading IsolateDetails.",
                    code: "ERR_ISOLATE" );

                var errorResponse = new BusinessValidationErrorException([error]);

                throw errorResponse;
            }

            return _mapper.Map<IsolateFullDetailDTO>(isolateFullDetail);

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

    
    }
}
