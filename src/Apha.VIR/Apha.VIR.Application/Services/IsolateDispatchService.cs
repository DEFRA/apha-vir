using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
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
        private readonly ILookupRepository _lookupRepository;
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
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _mapper = mapper;
        }

        public async Task DeleteDispatchAsync(Guid DispatchId, byte[] LastModified, string User)
        {
            if (DispatchId == Guid.Empty)
                throw new ArgumentException("DispatchId cannot be empty.", nameof(DispatchId));
            if (LastModified == null)
                throw new ArgumentNullException("LastModified cannot be null.", nameof(LastModified));
            if (string.IsNullOrWhiteSpace(User))
                throw new ArgumentException("User cannot be empty.", nameof(User));

            await _isolateDispatchRepository.DeleteDispatchAsync(DispatchId, LastModified, User);
        }

        //public async Task<IEnumerable<DispatchDTO>> GetDispatchAsync(string AVNumber, Guid DispatchId, Guid DispatchIsolateId)
        //{
        //    var isolationList = await _iIsolateRepository.GetIsolateInfoByAVNumberAsync(AVNumber);

        //    var matchIsolate = isolationList.Where(x => x.IsolateId == DispatchIsolateId).ToList();

        //    if ((matchIsolate == null) || (matchIsolate.Count == 0))
        //    {
        //        return Enumerable.Empty<DispatchDTO>();
        //    }

        //    var matchIsolateId = matchIsolate.FirstOrDefault()?.IsolateId;

        //    var dispatchHistList = await _isolateDispatchRepository.GetDispatchAsync(matchIsolateId.Value);

        //    var staffs = await _staffRepository.GetStaffListAsync();
        //    var workgroups = await _IWorkgroupRepository.GetWorkgroupfListAsync();
        //    var viablities = await _lookupRepository.GetAllViabilityAsync();

        //    var dispatchDtos = MapDispatchesToDispatchDTO(dispatchHistList, staffs, workgroups);

        //    return dispatchDtos;
        //}

        //private IEnumerable<DispatchDTO> MapDispatchesToDispatchDTO(IEnumerable<IsolateDispatch> dispatchHistList, IEnumerable<Staff> staffs, IEnumerable<Workgroup> workgroups)
        //{
        //    var dispatchDtos = new List<DispatchDTO>();

        //    foreach (var dispatch in dispatchHistList)
        //    {
        //        var dto = new DispatchDTO
        //        {
        //            DispatchId = dispatch.DispatchId ?? Guid.Empty,
        //            DispatchIsolateId = dispatch.DispatchIsolateId ?? Guid.Empty,
        //            AVNumber = dispatch.AVNumber ?? string.Empty,
        //            Nomenclature = dispatch.Nomenclature ?? string.Empty,
        //            ValidToIssue = dispatch.ValidToIssue,
        //            ViabilityId = dispatch.ViabilityId,
        //            NoOfAliquots = dispatch.NoOfAliquots,
        //            NoOfAliquotsToBeDispatched = dispatch.NoOfAliquotsToBeDispatched,
        //            PassageNumber = dispatch.PassageNumber ?? 0,
        //            Recipient = dispatch.RecipientId,
        //            RecipientList = workgroups?.Select(wg => new SelectListItem
        //            {
        //                Value = wg.Id.ToString(),
        //                Text = wg.Name
        //            }).ToList() ?? new List<SelectListItem>(),
        //            RecipientName = dispatch.RecipientName,
        //            RecipientAddress = dispatch.RecipientAddress,
        //            ReasonForDispatch = dispatch.ReasonForDispatch,
        //            DispatchedDate = (DateTime)dispatch.DispatchedDate,
        //            DispatchedBy = dispatch.DispatchedById,
        //            LastModified = dispatch.LastModified ?? Array.Empty<byte>()
        //        };

        //        dispatchDtos.Add(dto);
        //    }

        //    return dispatchDtos;
        //}

            
             

        public async Task<IEnumerable<IsolateDispatchInfoDTO>> GetDispatchesHistoryAsync(string AVNumber, Guid IsolateId)
        {
            var isolationList = await _iIsolateRepository.GetIsolateInfoByAVNumberAsync(AVNumber);

            var matchIsolate = isolationList.Where(x => x.IsolateId == IsolateId).ToList();

            if ((matchIsolate == null) || (matchIsolate.Count == 0))
            {
                //return Enumerable.Empty<IsolateDispatchInfoDTO>();
                return _mapper.Map<IEnumerable<IsolateDispatchInfoDTO>>(Enumerable.Empty<IsolateDispatchInfoDTO>());
            }

            var matchIsolateId = matchIsolate?.FirstOrDefault()?.IsolateId;


            var dispatchHistList = await _isolateDispatchRepository.GetDispatchesHistoryAsync(matchIsolateId.Value);

            var characteristicList = await _iCharacteristicRepository.GetIsolateCharacteristicInfoAsync(matchIsolateId.Value);

            var charNomenclature = GetCharacteristicNomenclature(characteristicList.ToList());
            ;
            var Nomenclature = string.IsNullOrEmpty(charNomenclature) ? matchIsolate?.FirstOrDefault()?.Nomenclature : charNomenclature;

            foreach (var dh in dispatchHistList)
            {
                dh.Nomenclature = Nomenclature;
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


        private string GetCharacteristicNomenclature(IList<IsolateCharacteristicInfo> characteristicList)
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
