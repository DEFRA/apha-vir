using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditSubmissionLogDTO>> GetSubmissionLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditSampleLogDTO>> GetSamplLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditIsolateLogDTO>> GetIsolatLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditCharacteristicLogDTO>> GetCharacteristicsLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditDispatchLogDTO>> GetDispatchLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
        Task<IEnumerable<AuditViabilityLogDTO>> GetIsolateViabilityLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid);
    }
}
