using System.Xml;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class IsolatesService : IIsolatesService
    {
        private readonly IIsolateRepository _isolateRepository;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly ISampleRepository _sampleRepository;
        private readonly ICharacteristicRepository _characteristicRepository;
        private readonly IMapper _mapper;

        public IsolatesService(IIsolateRepository isolateRepository,
            ISubmissionRepository submissionRepository,
            ISampleRepository sampleRepository,
            ICharacteristicRepository characteristicRepository,
            IMapper mapper)
        {
            _isolateRepository = isolateRepository ?? throw new ArgumentNullException(nameof(isolateRepository));
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _sampleRepository = sampleRepository ?? throw new ArgumentNullException(nameof(sampleRepository));
            _characteristicRepository = characteristicRepository ?? throw new ArgumentNullException(nameof(characteristicRepository));
            _mapper = mapper;
        }

        public async Task<IsolateFullDetailDTO> GetIsolateFullDetailsAsync(Guid IsolateId)
        {
            var isolateFullDetail = await _isolateRepository.GetIsolateFullDetailsByIdAsync(IsolateId);
            return _mapper.Map<IsolateFullDetailDTO>(isolateFullDetail);
        }

        public async Task<IsolateDTO> GetIsolateByIsolateAndAVNumberAsync(string avNumber, Guid isolateId)
        {
            var isolate = await _isolateRepository.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId);

            if (isolate.FamilyName == "Paramyxoviridae")
            {
                isolate.Nomenclature = isolate.Nomenclature + " (" + isolate.TypeName + ")";
            }
            else
            {
                if (String.IsNullOrEmpty(isolate.IsolateNomenclature))
                {
                    var characteristicList = await _characteristicRepository.GetIsolateCharacteristicInfoAsync(isolateId);
                    var charNomenclature = ServiceHelper.GetCharacteristicNomenclature(characteristicList.ToList());
                    isolate.Nomenclature = isolate.Nomenclature + " " + charNomenclature;
                }
            }

            return _mapper.Map<IsolateDTO>(isolate);
        }

        public async Task<Guid> AddIsolateDetailsAsync(IsolateDTO isolate)
        {
            var isolateData = _mapper.Map<Isolate>(isolate);
            return await _isolateRepository.AddIsolateDetailsAsync(isolateData);
        }

        public async Task UpdateIsolateDetailsAsync(IsolateDTO isolate)
        {
            var isolateData = _mapper.Map<Isolate>(isolate);
            await _isolateRepository.UpdateIsolateDetailsAsync(isolateData);
        }

        public async Task DeleteIsolateAsync(Guid isolateId, string userId, byte[] lastModified)
        {
            await _isolateRepository.DeleteIsolateAsync(isolateId, userId, lastModified);
        }

        public async Task<string> GenerateNomenclature(string avNumber, Guid sampleId, string virusType, string yearOfIsolation)
        {
            var nomenclature = new System.Text.StringBuilder();
            var submission = await _submissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber);

            var samples = await _sampleRepository.GetSamplesBySubmissionIdAsync(submission.SubmissionId);
            var sample = samples.FirstOrDefault(s => s.SampleId == sampleId);

            nomenclature.Append(!string.IsNullOrEmpty(virusType) ? virusType : "[Virus Type]");
            nomenclature.Append('/');
            nomenclature.Append(string.IsNullOrEmpty(sample?.HostBreedName) ? sample?.HostSpeciesName : sample.HostBreedName);
            nomenclature.Append('/');
            nomenclature.Append(submission.CountryOfOriginName);
            nomenclature.Append('/');
            nomenclature.Append(sample?.SenderReferenceNumber);
            nomenclature.Append('/');
            nomenclature.Append(!string.IsNullOrEmpty(yearOfIsolation) ? yearOfIsolation : "[Year of Isolation]");

            return nomenclature.ToString();
        }

        public async Task<IEnumerable<IsolateCharacteristicDTO>> GetIsolateCharacteristicInfoAsync(Guid IsolateId)
        {
            var characteristicList = await _characteristicRepository.GetIsolateCharacteristicInfoAsync(IsolateId);
            return _mapper.Map<IEnumerable<IsolateCharacteristicDTO>>(characteristicList);
        }

        public async Task UpdateIsolateCharacteristicsAsync(IsolateCharacteristicDTO item, string User)
        {
            var data = _mapper.Map<IsolateCharacteristicInfo>(item);
            await _characteristicRepository.UpdateIsolateCharacteristicsAsync(data, User);
        }

        public async Task<IEnumerable<IsolateInfoDTO>> GetIsolateInfoByAVNumberAsync(string AVNumber)
        {
            var isolates = await _isolateRepository.GetIsolateInfoByAVNumberAsync(AVNumber);            
            var isolatesDto = _mapper.Map<IEnumerable<IsolateInfoDTO>>(isolates);
            foreach (var isolate in isolatesDto)
            {
                var isolatecharList = await _characteristicRepository.GetIsolateCharacteristicInfoAsync(isolate.IsolateId);
                var isolateCharNomenclature = ServiceHelper.GetCharacteristicNomenclature(isolatecharList.ToList());
                isolate.Characteristics = string.IsNullOrEmpty(isolateCharNomenclature) ? "" : "(" + isolateCharNomenclature + ")";
            }
            return isolatesDto;
        }

        public async Task<bool> UniqueNomenclatureAsync(Guid isolateId, string? familyName, Guid type)
        {
            var isolates = await _isolateRepository.GetIsolateForNomenclatureAsync(isolateId);
            if(isolates != null && isolates.Any())
            {
                return await UniquenessCheckingForIsolates(isolateId, isolates, familyName, type);
            }
            else
            {
                return true;
            }                
        }

        private async Task<bool> UniquenessCheckingForIsolates(Guid isolateId, IEnumerable<IsolateNomenclature> isolates, string? familyName, Guid type)
        {
            bool unique = true;
            foreach (var nmIsolate in isolates)
            {
                //HACK:Temporary hardcoded method to handle uniqueness checking for viruses in the Paramyxoviridae family
                if (familyName == "Paramyxoviridae")
                {
                    if (type == nmIsolate.VirusType)
                    {
                        unique = false;
                    }                    
                }
                else
                {
                    var nmIsolatecharList = await _characteristicRepository.GetIsolateCharacteristicInfoAsync(nmIsolate.IsolateId);
                    var nmIsolateCharNomenclature = ServiceHelper.GetCharacteristicNomenclature(nmIsolatecharList.ToList());

                    var isolatecharList = await _characteristicRepository.GetIsolateCharacteristicInfoAsync(isolateId);
                    var isolateCharNomenclature = ServiceHelper.GetCharacteristicNomenclature(isolatecharList.ToList());
                    if (isolateCharNomenclature == nmIsolateCharNomenclature)
                    {
                        unique = false;
                    }
                }
            }
            return unique;
        }
    }
}
