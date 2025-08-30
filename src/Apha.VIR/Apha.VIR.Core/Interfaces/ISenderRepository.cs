using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;

namespace Apha.VIR.Core.Interfaces
{
    public interface ISenderRepository
    {
        Task<IEnumerable<Sender>> GetAllSenderOrderBySenderAsync(Guid? countryId);
        Task<IEnumerable<Sender>> GetAllSenderOrderByOrganisationAsync(Guid? countryId);
        Task<PagedData<Sender>> GetAllSenderAsync(int pageNo, int pageSize);
        Task AddSenderAsync(Sender sender);
    }
}