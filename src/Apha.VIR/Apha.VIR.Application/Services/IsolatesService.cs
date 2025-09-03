using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class IsolatesService : IIsolatesService
    {
        private readonly IIsolateRepository _iIsolateRepository;
        private readonly ISubmissionRepository _submissionRepository;
        private readonly ISampleRepository _sampleRepository;
        private readonly ICharacteristicRepository _characteristicRepository;
        private readonly IMapper _mapper;

        public IsolatesService(IIsolateRepository iIsolateRepository,
            ISubmissionRepository submissionRepository,
            ISampleRepository sampleRepository,
            ICharacteristicRepository characteristicRepository,
            IMapper mapper)
        {
            _iIsolateRepository = iIsolateRepository ?? throw new ArgumentNullException(nameof(iIsolateRepository));
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _sampleRepository = sampleRepository ?? throw new ArgumentNullException(nameof(sampleRepository));
            _characteristicRepository = characteristicRepository ?? throw new ArgumentNullException(nameof(characteristicRepository));
            _mapper = mapper;
        }

        public async Task<IsolateFullDetailDTO> GetIsolateFullDetailsAsync(Guid IsolateId)
        {
            var isolateFullDetail = await _iIsolateRepository.GetIsolateFullDetailsByIdAsync(IsolateId);
            return _mapper.Map<IsolateFullDetailDTO>(isolateFullDetail);
        }

        public async Task<IsolateDTO> GetIsolateByIsolateAndAVNumberAsync(string avNumber, Guid isolateId)
        {
            var isolate = await _iIsolateRepository.GetIsolateByIsolateAndAVNumberAsync(avNumber, isolateId);

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
            return await _iIsolateRepository.AddIsolateDetailsAsync(isolateData);
        }

        public async Task UpdateIsolateDetailsAsync(IsolateDTO isolate)
        {
            var isolateData = _mapper.Map<Isolate>(isolate);
            await _iIsolateRepository.UpdateIsolateDetailsAsync(isolateData);
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

        public async Task<IEnumerable<IsolateCharacteristicInfoDTO>> GetIsolateCharacteristicInfoAsync(Guid IsolateId)
        {
            var characteristicList = await _characteristicRepository.GetIsolateCharacteristicInfoAsync(IsolateId);
            return _mapper.Map<IEnumerable<IsolateCharacteristicInfoDTO>>(characteristicList);
        }

        public Task UpdateIsolateCharacteristicsAsync(IsolateCharacteristicInfoDTO item, string User)
        {
            var data = _mapper.Map<IsolateCharacteristicInfo>(item);
            return _characteristicRepository.UpdateIsolateCharacteristicsAsync(data, User);
        }
    }
}
