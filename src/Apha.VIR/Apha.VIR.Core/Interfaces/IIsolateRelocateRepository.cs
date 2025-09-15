using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IIsolateRelocateRepository
{
    Task<IEnumerable<IsolateRelocate>> GetIsolatesByCriteria(string? min, string? max, Guid? freezer, Guid? tray);
    Task UpdateIsolateFreezeAndTrayAsync(IsolateRelocate item);
}
