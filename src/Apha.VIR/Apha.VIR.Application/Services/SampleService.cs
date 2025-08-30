using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class SampleService : ISampleService
    {
        private readonly ISampleRepository _sampleRepository;
        private readonly IMapper _mapper;

        public SampleService(ISampleRepository sampleRepository,
            IMapper mapper)
        {
            _sampleRepository = sampleRepository ?? throw new ArgumentNullException(nameof(sampleRepository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<SampleDTO>> GetSamplesBySubmissionIdAsync(Guid submissionId)
        {
            var samples = await _sampleRepository.GetSamplesBySubmissionIdAsync(submissionId);
            return _mapper.Map<IEnumerable<SampleDTO>>(samples);
        }
    }
}
