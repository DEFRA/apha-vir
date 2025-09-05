using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusTypeCharacteristicService
    {
        Task AssignCharacteristicToTypeAsync(Guid virusTypeId, Guid characteristicId);
        Task RemoveCharacteristicFromTypeAsync(Guid virusTypeId, Guid characteristicId);
    }
}
