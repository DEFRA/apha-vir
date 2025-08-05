using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateSearchService
    {
        Task<Tuple<List<string>, List<VirusCharacteristicListEntryDTO>>> GetComparatorsAndListValuesAsync(Guid virusCharateristicId);
        Task<PaginatedResult<IsolateSearchResultDTO>> PerformSearchAsync(QueryParameters<SearchCriteriaDTO> criteria);
        Task<List<IsolateSearchExportDto>> GetIsolateSearchExportResultAsync(QueryParameters<SearchCriteriaDTO> criteria);
    }
}
