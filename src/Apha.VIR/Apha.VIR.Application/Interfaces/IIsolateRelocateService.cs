
using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateRelocateService
    {
        Task<IEnumerable<IsolateRelocateDto>> GetIsolatesByCriteria(string? min, string? max, Guid? freezer, Guid? tray);
        Task UpdateIsolateFreezeAndTrayAsync(IsolateRelocateDto item);
    }
}
