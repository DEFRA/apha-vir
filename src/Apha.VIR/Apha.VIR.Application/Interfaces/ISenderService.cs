using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISenderService
    {
        Task<IEnumerable<SenderDto>> GetAllSenderOrderByOrganisationAsync(Guid? countryId);
        Task<IEnumerable<SenderDto>> GetAllSenderOrderBySenderAsync(Guid? countryId);
        Task<PaginatedResult<SenderDto>> GetAllSenderAsync(int pageNo, int pageSize);
        Task<SenderDto> GetSenderAsync(Guid senderId);
        Task AddSenderAsync(SenderDto sender);
        Task UpdateSenderAsync(SenderDto sender);
        Task DeleteSenderAsync(Guid senderId);
    }
}
