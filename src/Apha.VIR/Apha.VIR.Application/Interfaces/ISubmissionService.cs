using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISubmissionService
    {
        Task<bool> AVNumberExistsInVirAsync(string avNumber);
        Task<SubmissionDTO> GetSubmissionDetailsByAVNumberAsync(string avNumber);
        Task AddSubmissionAsync(SubmissionDTO submission, string user);
        Task UpdateSubmissionAsync(SubmissionDTO submission, string user);
    }
}
