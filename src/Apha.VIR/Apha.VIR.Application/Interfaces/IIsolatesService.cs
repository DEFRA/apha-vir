using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolatesService
    {
        Task<IsolateFullDetailDTO> GetIsolateFullDetailsAsync(Guid IsolateId);
    }
}
