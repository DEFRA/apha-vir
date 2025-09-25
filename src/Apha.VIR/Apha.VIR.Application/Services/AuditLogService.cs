using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditRepository _auditRepository;
        private readonly IMapper _mapper;

        public AuditLogService(IAuditRepository auditRepository, IMapper mapper)
        {
            _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<AuditCharacteristicLogDto>> GetCharacteristicsLogsAsync(string avNumber,
            DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            ArgumentNullException.ThrowIfNull(avNumber);
            ArgumentNullException.ThrowIfNull(userid);

            var result = await _auditRepository.GetCharacteristicsLogsAsync(avNumber, dateFrom, dateTo, userid);
            return _mapper.Map<IEnumerable<AuditCharacteristicLogDto>>(result);
        }

        public async Task<IEnumerable<AuditDispatchLogDto>> GetDispatchLogsAsync(string avNumber,
            DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            ArgumentNullException.ThrowIfNull(avNumber);
            ArgumentNullException.ThrowIfNull(userid);

            var result = await _auditRepository.GetDispatchLogsAsync(avNumber, dateFrom, dateTo, userid);

            return _mapper.Map<IEnumerable<AuditDispatchLogDto>>(result);
        }

        public async Task<IEnumerable<AuditViabilityLogDto>> GetIsolateViabilityLogsAsync(string avNumber,
            DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            ArgumentNullException.ThrowIfNull(avNumber);
            ArgumentNullException.ThrowIfNull(userid);

            var result = await _auditRepository.GetIsolateViabilityLogsAsync(avNumber, dateFrom, dateTo, userid);

            return _mapper.Map<IEnumerable<AuditViabilityLogDto>>(result);
        }

        public async Task<AuditIsolateLogDetailDto> GetIsolatLogDetailAsync(Guid logid)
        {
            ArgumentNullException.ThrowIfNull(logid);

            var result = await _auditRepository.GetIsolatLogDetailAsync(logid);

            return _mapper.Map<AuditIsolateLogDetailDto>(result.FirstOrDefault()); ;
        }

        public async Task<IEnumerable<AuditIsolateLogDto>> GetIsolatLogsAsync(string avNumber,
            DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            ArgumentNullException.ThrowIfNull(avNumber);
            ArgumentNullException.ThrowIfNull(userid);

            var result = await _auditRepository.GetIsolatLogsAsync(avNumber, dateFrom, dateTo, userid);

            return _mapper.Map<IEnumerable<AuditIsolateLogDto>>(result); ;
        }

        public async Task<IEnumerable<AuditSampleLogDto>> GetSamplLogsAsync(string avNumber,
            DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            ArgumentNullException.ThrowIfNull(avNumber);
            ArgumentNullException.ThrowIfNull(userid);

            var result = await _auditRepository.GetSamplLogsAsync(avNumber, dateFrom, dateTo, userid);

            return _mapper.Map<IEnumerable<AuditSampleLogDto>>(result);
        }

        public async Task<IEnumerable<AuditSubmissionLogDto>> GetSubmissionLogsAsync(string avNumber,
            DateTime? dateFrom, DateTime? dateTo, string userid)
        {
            ArgumentNullException.ThrowIfNull(avNumber);
            ArgumentNullException.ThrowIfNull(userid);

            var result = await _auditRepository.GetSubmissionLogsAsync(avNumber, dateFrom, dateTo, userid);

            return _mapper.Map<IEnumerable<AuditSubmissionLogDto>>(result);
        }
    }
}