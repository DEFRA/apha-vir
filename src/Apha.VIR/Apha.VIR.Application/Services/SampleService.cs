using System.Net.Http.Headers;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class SampleService : ISampleService
    {
        private readonly ISampleRepository _sampleRepository;
        private readonly IMapper _mapper;

        public SampleService(ISampleRepository sampleRepository, IMapper mapper)
        {
            _sampleRepository = sampleRepository ?? throw new ArgumentNullException(nameof(sampleRepository));
            _mapper = mapper;
        }

        public async Task<SampleDTO> GetSampleAsync(string avNumber, Guid? sampleId)
        {
            ArgumentNullException.ThrowIfNull(avNumber);

            var sample = await _sampleRepository.GetSampleAsync(avNumber, sampleId);
            return _mapper.Map<SampleDTO>(sample);
        }

        public async Task AddSample(SampleDTO sampleDto, string avNumber, string userName)
        {
            ArgumentNullException.ThrowIfNull(sampleDto);

            var sample = _mapper.Map<Sample>(sampleDto);
            await _sampleRepository.AddSampleAsync(sample, avNumber, userName);            
        }

        public async Task UpdateSample(SampleDTO sampleDto, string userName)
        {
            ArgumentNullException.ThrowIfNull(sampleDto); 

            var sample = _mapper.Map<Sample>(sampleDto);
            await _sampleRepository.UpdateSampleAsync(sample, userName);
        }
    }
}

