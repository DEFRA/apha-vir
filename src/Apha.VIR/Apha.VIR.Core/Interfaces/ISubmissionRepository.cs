using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface ISubmissionRepository
{
    Task<Submission> GetSubmissionDetailsByAVNumberAsync(string AVNumber);
    Task AddSubmissionAsync(Submission submission, string user);
    Task UpdateSubmissionAsync(Submission submission, string user);
}
