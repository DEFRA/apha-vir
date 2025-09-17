using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditSubmissionLogDto>> GetSubmissionLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditSampleLogDto>> GetSamplLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditIsolateLogDto>> GetIsolatLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditCharacteristicLogDto>> GetCharacteristicsLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditDispatchLogDto>> GetDispatchLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditViabilityLogDto>> GetIsolateViabilityLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<AuditIsolateLogDetailDto> GetIsolatLogDetailAsync(Guid logid);
    }
}
