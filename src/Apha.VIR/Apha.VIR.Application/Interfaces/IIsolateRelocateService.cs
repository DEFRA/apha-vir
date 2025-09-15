
using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateRelocateService
    {
        Task<IEnumerable<IsolateRelocateDTO>> GetIsolatesByCriteria(string? min, string? max, Guid? freezer, Guid? tray);
        Task UpdateIsolateFreezeAndTrayAsync(IsolateRelocateDTO item);
    }
}
