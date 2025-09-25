using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateSearchService
    {
        Task<Tuple<List<string>, List<string>>> GetComparatorsAndListValuesAsync(Guid virusCharateristicId);
        Task<PaginatedResult<IsolateSearchResultDto>> PerformSearchAsync(QueryParameters<SearchCriteriaDto> criteria);
        Task<List<IsolateSearchExportDto>> GetIsolateSearchExportResultAsync(QueryParameters<SearchCriteriaDto> criteria);
    }
}
