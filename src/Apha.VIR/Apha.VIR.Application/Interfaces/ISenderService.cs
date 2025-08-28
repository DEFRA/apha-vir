using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface ISenderService
    {
        Task<IEnumerable<SenderDTO>> GetAllSenderOrderByOrganisationAsync(Guid? countryId);
        Task<IEnumerable<SenderDTO>> GetAllSenderOrderBySenderAsync(Guid? countryId);
        Task AddSenderAsync(SenderDTO sender);
    }
}
