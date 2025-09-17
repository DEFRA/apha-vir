using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISubmissionService
    {
        Task<bool> AVNumberExistsInVirAsync(string avNumber);
        Task<SubmissionDto> GetSubmissionDetailsByAVNumberAsync(string avNumber);
        Task AddSubmissionAsync(SubmissionDto submission, string user);
        Task UpdateSubmissionAsync(SubmissionDto submission, string user);
        Task DeleteSubmissionAsync(Guid submissionId, string userId, byte[] lastModified);
        Task<string> SubmissionLetter(string AVNumber, string user);
        Task<IEnumerable<string>> GetLatestSubmissionsAsync();
    }
}
