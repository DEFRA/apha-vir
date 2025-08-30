using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface ISampleRepository
{
    Task<IEnumerable<Sample>> GetSamplesBySubmissionIdAsync(Guid submissionId);
}
