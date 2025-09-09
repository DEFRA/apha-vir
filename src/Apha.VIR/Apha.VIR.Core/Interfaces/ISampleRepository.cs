using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface ISampleRepository
{
    Task<IEnumerable<Sample>> GetSamplesBySubmissionIdAsync(Guid submissionId);
    Task<Sample?> GetSampleAsync(string avNumber, Guid? sampleId);

    Task AddSampleAsync(Sample sample, string avNumber, string User);

    Task UpdateSampleAsync(Sample sample, string User);
    Task DeleteSampleAsync(Guid sampleId, string userId, byte[] lastModified);
}
