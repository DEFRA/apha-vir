using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<IsolateDispatchReportDTO>> GetDispatchesReportAsync(DateTime? dateFrom, DateTime? dateTo);
    }
}