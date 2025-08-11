using System.Diagnostics.Metrics;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using AutoMapper;

namespace Apha.VIR.Application.Services
{
    public class IsolateSearchService : IIsolateSearchService
    {
        private readonly IVirusCharacteristicRepository _virusCharacteristicRepository;
        private readonly IVirusCharacteristicListEntryRepository _virusCharacteristicListEntryRepository;
        private readonly IIsolateSearchRepository _isolateSearchRepository;
        private readonly IIsolateRepository _iIsolateRepository;
        private readonly IMapper _mapper;

        public IsolateSearchService(IVirusCharacteristicRepository virusCharacteristicRepository, 
            IVirusCharacteristicListEntryRepository virusCharacteristicListEntryRepository, 
            IIsolateSearchRepository isolateSearchRepository,
            IIsolateRepository iIsolateRepository,
            IMapper mapper)
        {
            _virusCharacteristicRepository = virusCharacteristicRepository ?? throw new ArgumentNullException(nameof(virusCharacteristicRepository));
            _virusCharacteristicListEntryRepository = virusCharacteristicListEntryRepository ?? throw new ArgumentNullException(nameof(virusCharacteristicListEntryRepository));
            _isolateSearchRepository = isolateSearchRepository ?? throw new ArgumentNullException(nameof(isolateSearchRepository));
            _iIsolateRepository = iIsolateRepository ?? throw new ArgumentNullException(nameof(iIsolateRepository));
            _mapper = mapper;
        }

        public async Task<Tuple<List<string>, List<VirusCharacteristicListEntryDTO>>> GetComparatorsAndListValuesAsync(Guid virusCharateristicId)
        {
            List<string> compartaors = new List<string>();
            List<VirusCharacteristicListEntryDTO> listValues = new List<VirusCharacteristicListEntryDTO>();

            var virusCharacteristics = _mapper.Map<IEnumerable<VirusCharacteristicDTO>>(await _virusCharacteristicRepository.GetAllVirusCharacteristicsAsync());
            VirusCharacteristicDTO? characteristic = virusCharacteristics.FirstOrDefault(c => c.Id == virusCharateristicId);
            if (characteristic != null)
            {
                switch (characteristic.DataType)
                {
                    case "Numeric":
                        compartaors.AddRange(new List<string> { "=", "<", ">", "<=", ">=", "between" });
                        break;
                    case "SingleList":
                        compartaors.AddRange(new List<string> { "=", "not equal to", "begins with" });
                        listValues = _mapper.Map<IEnumerable<VirusCharacteristicListEntryDTO>>(await _virusCharacteristicListEntryRepository.GetVirusCharacteristicListEntryByVirusCharacteristic(characteristic.Id)).ToList();
                        break;
                    case "Yes/No":
                        compartaors.AddRange(new List<string> { "=" });
                        break;
                    case "Text":
                        compartaors.AddRange(new List<string> { "=", "contains" });
                        break;
                }
            }
            return Tuple.Create(compartaors, listValues);
        }

        public async Task<PaginatedResult<IsolateSearchResultDTO>> PerformSearchAsync(QueryParameters<SearchCriteriaDTO> criteria)
        {     
            var criteriaData = _mapper.Map<PaginationParameters<SearchCriteria>>(criteria);
            return _mapper.Map<PaginatedResult<IsolateSearchResultDTO>>(await _isolateSearchRepository.PerformSearchAsync(criteriaData));
        }

        public async Task<List<IsolateSearchExportDto>> GetIsolateSearchExportResultAsync(QueryParameters<SearchCriteriaDTO> criteria)
        {
            List<IsolateSearchExportDto> isolateSearchExportData = new List<IsolateSearchExportDto>();
            var criteriaData = _mapper.Map<PaginationParameters<SearchCriteria>>(criteria);            
            List<IsolateSearchResultDTO> isolateRecords = _mapper.Map<List<IsolateSearchResultDTO>>(await _isolateSearchRepository.GetIsolateSearchExportResultAsync(criteriaData));
            foreach (var record in isolateRecords)
            {
                IsolateFullDetailDTO data = _mapper.Map<IsolateFullDetailDTO>(await _iIsolateRepository.GetIsolateFullDetailsByIdAsync(record.IsolateId));
                IsolateSearchExportDto isolateInfo = _mapper.Map<IsolateSearchExportDto>(data.IsolateDetails);
                isolateInfo.ViabilityChecks = string.Join(", ", data.IsolateViabilityDetails
                    .Select(v => $"{v.ViabilityStatus}: checked by {v.CheckedByName} on {v.DateChecked.ToString("dd/MM/yyyy")}"));
                isolateInfo.Characteristics = string.Join(", ", data.IsolateCharacteristicDetails
                    .Select(c => $"{c.CharacteristicName}: {(String.IsNullOrEmpty(c.CharacteristicValue) ? "no value entered" : c.CharacteristicValue)}"));                

                if (criteria.Filter != null && criteria.Filter.FullSearch)// exclude freezer, tray, well values for admin users
                {
                    isolateInfo.Freezer = "";
                    isolateInfo.Tray = "";
                    isolateInfo.Well = "";
                }
                isolateSearchExportData.Add(isolateInfo);
            }            
            return isolateSearchExportData;
        }
    }
}