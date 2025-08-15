using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces;

public interface IAuditRepository
{
    Task<IEnumerable<AuditSubmissionLog>> GetSubmissionLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
    Task<IEnumerable<AuditSampleLog>> GetSamplLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
    Task<IEnumerable<AuditIsolateLogDetail>> GetIsolatLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
    Task<IEnumerable<AuditCharacteristicLog>> GetCharacteristicsLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
    Task<IEnumerable<AuditDispatchLog>> GetDispatchLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
    Task<IEnumerable<AuditViabilityLog>> GetIsolateViabilityLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);

}
