using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISampleService
    {
        Task<IEnumerable<SampleDTO>> GetSamplesBySubmissionIdAsync(Guid submissionId);
    }
}
