using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Application.Interfaces
{
    public interface IVirusCharacteristicListEntryService
    {
        Task<IEnumerable<VirusCharacteristicListEntryDTO>> GetEntriesByCharacteristicIdAsync(Guid virusCharacteristicId);
        Task<VirusCharacteristicListEntryDTO?> GetEntryByIdAsync(Guid id);
        Task AddEntryAsync(VirusCharacteristicListEntryDTO dto);
        Task UpdateEntryAsync(VirusCharacteristicListEntryDTO dto);
        Task DeleteEntryAsync(Guid id, byte[] lastModified);
    }
}
