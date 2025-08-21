using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISubmissionService
    {
        Task<SubmissionDTO> GetSubmissionDetailsByAVNumberAsync(string AVNumber);
        Task AddSubmissionAsync(SubmissionDTO submission, string user);
        Task UpdateSubmissionAsync(SubmissionDTO submission, string user);
    }
}
