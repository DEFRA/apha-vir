using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IReportRepository
{
    Task<IEnumerable<IsolateDispatchInfo>> GetDispatchesReportAsync(DateTime? dateFrom, DateTime? dateTo);
}
