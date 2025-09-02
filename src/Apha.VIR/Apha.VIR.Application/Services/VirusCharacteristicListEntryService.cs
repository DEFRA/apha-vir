using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Interfaces;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class VirusCharacteristicListEntryService : IVirusCharacteristicListEntryService
    {
        private readonly IVirusCharacteristicListEntryRepository _repository;
        private readonly IMapper _mapper;

        public VirusCharacteristicListEntryService(IVirusCharacteristicListEntryRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper;
        }

        public async Task<IEnumerable<VirusCharacteristicListEntryDTO>> GetEntriesByCharacteristicIdAsync(Guid virusCharacteristicId)
        {
            var entities = await _repository.GetVirusCharacteristicListEntryByVirusCharacteristic(virusCharacteristicId);
            return _mapper.Map<IEnumerable<VirusCharacteristicListEntryDTO>>(entities);
        }
    }
}
