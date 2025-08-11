using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;

namespace Apha.VIR.Core.Interfaces
{
    public interface IIsolateSearchRepository
    {
        Task<PagedData<IsolateSearchResult>> PerformSearchAsync(PaginationParameters<SearchCriteria> criteria);
        Task<IEnumerable<IsolateSearchResult>> GetIsolateSearchExportResultAsync(PaginationParameters<SearchCriteria> criteria);
    }
}
