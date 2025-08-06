using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateViabilityService
    {
        Task<IEnumerable<IsolateViabilityInfoDTO>> GetViabilityHistoryAsync(string AVNumber, Guid IsolateId);
        Task DeleteIsolateViabilityAsync(Guid IsolateId, byte[] lastModified, string userid);
    }
}
