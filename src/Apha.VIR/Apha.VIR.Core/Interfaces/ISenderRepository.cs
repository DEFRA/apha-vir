using Apha.VIR.Core.Entities;

namespace Apha.VIR.Core.Interfaces
{
    public interface ISenderRepository
    {
        Task<IEnumerable<Sender>> GetAllSenderOrderBySenderAsync(Guid? countryId);
        Task<IEnumerable<Sender>> GetAllSenderOrderByOrganisationAsync(Guid? countryId);
        Task AddSenderAsync(Sender sender);
    }
}