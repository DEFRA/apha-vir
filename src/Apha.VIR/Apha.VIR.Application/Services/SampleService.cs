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
            _sampleRepository = sampleRepository;
            _mapper = mapper;
        }

        public async Task<SampleDTO> GetSampleAsync(string avNumber, Guid? sampleId)
        {
            var sample = await _sampleRepository.GetSampleAsync(avNumber, sampleId);
            return _mapper.Map<SampleDTO>(sample);
        }
    }
}

