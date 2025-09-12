using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISubmissionService
    {
        Task<bool> AVNumberExistsInVirAsync(string avNumber);
        Task<SubmissionDTO> GetSubmissionDetailsByAVNumberAsync(string avNumber);
        Task AddSubmissionAsync(SubmissionDTO submission, string user);
        Task UpdateSubmissionAsync(SubmissionDTO submission, string user);
        Task DeleteSubmissionAsync(Guid submissionId, string userId, byte[] lastModified);
        Task<string> SubmissionLetter(string AVNumber, string user);
        Task<IEnumerable<string>> GetLatestSubmissionsAsync();
    }
}
