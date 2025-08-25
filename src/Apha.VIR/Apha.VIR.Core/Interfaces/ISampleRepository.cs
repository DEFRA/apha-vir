using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface ISampleRepository
{
    Task<Sample?> GetSampleAsync(string avNumber, Guid? sampleId);
}
