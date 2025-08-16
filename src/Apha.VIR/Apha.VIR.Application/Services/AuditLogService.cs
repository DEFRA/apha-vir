using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IMapper _mapper;

        public AuditLogService(IAuditRepository auditRepository, ILookupRepository lookupRepository,
           IMapper mapper)
        {
            _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _mapper = mapper;
        }
        public async Task<IEnumerable<AuditCharacteristicLogDTO>> GetCharacteristicsLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            var result = await _auditRepository.GetCharacteristicsLogsAsync(avNumber,dateFrom, dateTo, userid);
            return _mapper.Map<IEnumerable<AuditCharacteristicLogDTO>>(result);
        }

        public async Task<IEnumerable<AuditDispatchLogDTO>> GetDispatchLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            var result = await _auditRepository.GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userid);
            return _mapper.Map<IEnumerable<AuditDispatchLogDTO>>(result);
        }

        public async Task<IEnumerable<AuditViabilityLogDTO>> GetIsolateViabilityLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            var result = await _auditRepository.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);
            return _mapper.Map<IEnumerable<AuditViabilityLogDTO>>(result);
        }

        public async Task<IEnumerable<AuditIsolateLogDTO>> GetIsolatLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            var result = await _auditRepository.GetIsolatLogsAsync(avNumber, dateFrom, dateTo, userid);
            return _mapper.Map<IEnumerable<AuditIsolateLogDTO>>(result); ;
        }

        public async Task<IEnumerable<AuditSampleLogDTO>> GetSamplLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            var result = await _auditRepository.GetSamplLogsAsync(avNumber, dateFrom, dateTo, userid);
            return _mapper.Map<IEnumerable<AuditSampleLogDTO>>(result);
        }

        public async Task<IEnumerable<AuditSubmissionLogDTO>> GetSubmissionLogsAsync(string avNumber, DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            var result = await _auditRepository.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid);
            return _mapper.Map<IEnumerable<AuditSubmissionLogDTO>>(result);
        }
    }
}
