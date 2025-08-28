using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IMapper _mapper;

        public ReportService(IReportRepository reportRepository, ILookupRepository lookupRepository,
            IMapper mapper)
        {
            _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
            _mapper = mapper;
        }
        public async Task<IEnumerable<IsolateDispatchReportDTO>> GetDispatchesReportAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            var result = await _reportRepository.GetDispatchesReportAsync(dateFrom, dateTo);

            var staffs = await _lookupRepository.GetAllStaffAsync();
            var workgroups = await _lookupRepository.GetAllWorkGroupsAsync();

            foreach (var dispatch in result)
            {
                if (dispatch.RecipientId.HasValue)
                {
                    dispatch.Recipient = workgroups?.FirstOrDefault(wg => wg.Id == dispatch.RecipientId.Value)?.Name;
                }
            }

            foreach (var dispatch in result)
            {
                if (dispatch.DispatchedById.HasValue)
                {
                    dispatch.DispatchedByName = staffs?.FirstOrDefault(s => s.Id == dispatch.DispatchedById)?.Name;
                }
            }

            return _mapper.Map<IEnumerable<IsolateDispatchReportDTO>>(result);
        }
    }
}
