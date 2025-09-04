using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly ISubmissionRepository _submissionRepository;
        private readonly IMapper _mapper;

        public SubmissionService(ISubmissionRepository submissionRepository,
            IMapper mapper)
        {
            _submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
            _mapper = mapper;
        }

        public async Task<bool> AVNumberExistsInVirAsync(string avNumber)
        {
            return await _submissionRepository.AVNumberExistsInVirAsync(avNumber);
        }

        public async Task<SubmissionDTO> GetSubmissionDetailsByAVNumberAsync(string avNumber)
        {
            var submission = await _submissionRepository.GetSubmissionDetailsByAVNumberAsync(avNumber);
            return _mapper.Map<SubmissionDTO>(submission);
        }

        public async Task AddSubmissionAsync(SubmissionDTO submission, string user)
        {
            var submissionData = _mapper.Map<Submission>(submission);
            await _submissionRepository.AddSubmissionAsync(submissionData, user);
        }

        public async Task UpdateSubmissionAsync(SubmissionDTO submission, string user)
        {
            var submissionData = _mapper.Map<Submission>(submission);
            await _submissionRepository.UpdateSubmissionAsync(submissionData, user);
        }

        public async Task<IEnumerable<string>> GetLatestSubmissionsAsync()
        {
            var avNumbers = await _submissionRepository.GetLatestSubmissionsAsync();
            return avNumbers;
        }
    }
}
