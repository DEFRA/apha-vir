using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<IsolateDispatchReportDto>> GetDispatchesReportAsync(DateTime? dateFrom, DateTime? dateTo);
    }
}