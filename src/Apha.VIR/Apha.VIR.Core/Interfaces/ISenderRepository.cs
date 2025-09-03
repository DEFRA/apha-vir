using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Pagination;

namespace Apha.VIR.Core.Interfaces
{
    public interface ISenderRepository
    {
        Task<IEnumerable<Sender>> GetAllSenderOrderBySenderAsync(Guid? countryId);
        Task<IEnumerable<Sender>> GetAllSenderOrderByOrganisationAsync(Guid? countryId);
        Task<PagedData<Sender>> GetAllSenderAsync(int pageNo, int pageSize);
        Task<Sender> GetSenderAsync(Guid senderId);
        Task AddSenderAsync(Sender sender);
        Task UpdateSenderAsync(Sender sender);
        Task DeleteSenderAsync(Guid senderId);
    }
}