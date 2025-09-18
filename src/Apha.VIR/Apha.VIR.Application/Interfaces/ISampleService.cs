using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISampleService
    {
        Task<SampleDto> GetSampleAsync(string avNumber, Guid? sampleId);
        Task AddSample(SampleDto SampleDto, string avNumber, string userName);
        Task UpdateSample(SampleDto SampleDto, string userName);
        Task DeleteSampleAsync(Guid sampleId, string userId, byte[] lastModified);
        Task<IEnumerable<SampleDto>> GetSamplesBySubmissionIdAsync(Guid submissionId);
    }
}
