using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface ISubmissionRepository
{
    Task<bool> AVNumberExistsInVirAsync(string avNumber);
    Task<Submission> GetSubmissionDetailsByAVNumberAsync(string avNumber);
    Task AddSubmissionAsync(Submission submission, string user);
    Task UpdateSubmissionAsync(Submission submission, string user);
    Task<IEnumerable<string>> GetLatestSubmissionsAsync();
}
