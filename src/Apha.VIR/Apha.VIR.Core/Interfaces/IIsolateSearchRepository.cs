using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;

namespace Apha.VIR.Core.Interfaces
{
    public interface IIsolateSearchRepository
    {
        Task<PagedData<IsolateSearchResult>> PerformSearchAsync(PaginationParameters<SearchCriteria> criteria);
        Task<IsolateFullDetailsResult> GetIsolateFullDetailsById(Guid isolateId);
        Task<List<IsolateFullDetailsResult>> GetIsolateSearchExportResultAsync(PaginationParameters<SearchCriteria> criteria);
    }
}
