using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Core.Entities;

namespace Apha.VIR.Application.Interfaces
{
    public interface IIsolateDispatchService
    {
        Task<IEnumerable<IsolateDispatchInfoDTO>> GetDispatchesHistoryAsync(string AVNumber, Guid IsolateId);
        Task DeleteDispatchAsync(Guid DispatchId, byte[] LastModified, string User);

        //Task<IEnumerable<DispatchDTO>> GetDispatchAsync(string AVNumber, Guid DispatchId, Guid DispatchIsolateId);
    }
}
