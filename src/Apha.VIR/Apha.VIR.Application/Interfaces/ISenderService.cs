using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISenderService
    {
        Task<IEnumerable<SenderDTO>> GetAllSenderOrderByOrganisationAsync(Guid? countryId);
        Task<IEnumerable<SenderDTO>> GetAllSenderOrderBySenderAsync(Guid? countryId);
        Task<PaginatedResult<SenderDTO>> GetAllSenderAsync(int pageNo, int pageSize);
        Task AddSenderAsync(SenderDTO sender);
    }
}
