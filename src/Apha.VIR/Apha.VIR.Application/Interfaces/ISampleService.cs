using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISampleService
    {
        Task<SampleDTO> GetSampleAsync(string avNumber, Guid? sampleId);
        Task AddSample(SampleDTO sampleDto, string avNumber, string userName);
        Task UpdateSample(SampleDTO sampleDto, string userName);
    }
}
