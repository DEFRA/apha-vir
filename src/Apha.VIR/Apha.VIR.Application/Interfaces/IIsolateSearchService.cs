using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateSearchService
    {
        Task<Tuple<List<string>, List<VirusCharacteristicListEntryDto>>> GetComparatorsAndListValuesAsync(Guid virusCharateristicId);
        Task<PaginatedResult<IsolateSearchResultDto>> PerformSearchAsync(QueryParameters<SearchCriteriaDTO> criteria);
        Task<List<IsolateSearchExportDto>> GetIsolateSearchExportResultAsync(QueryParameters<SearchCriteriaDTO> criteria);
    }
}
