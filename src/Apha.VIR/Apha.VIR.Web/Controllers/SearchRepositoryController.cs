using System.ComponentModel.DataAnnotations;
using System.Data;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Apha.VIR.Web.Controllers
{
    public class SearchRepositoryController : Controller
    {
        private readonly IVirusCharacteristicService _virusCharacteristicService;
        private readonly IIsolateSearchService _isolateSearchService;
        private readonly ILookupService _lookupService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private const string keySearchCriteria = "SearchCriteria";

        public SearchRepositoryController(ILookupService lookupService,
            IVirusCharacteristicService virusCharacteristicService,
            IIsolateSearchService isolateSearchService,
            ICacheService cacheService,
            IMapper mapper)
        {
            _lookupService = lookupService;
            _virusCharacteristicService = virusCharacteristicService;
            _isolateSearchService = isolateSearchService;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var searchModel = await LoadIsolateSearchFilterControlsData(null);

            return View("IsolateSearch", searchModel);
        }

        public async Task<IActionResult> Search(SearchCriteria criteria, bool IsNewSearch = false)
        {
            SearchRepositoryViewModel searchModel = new();
            QueryParameters<SearchCriteriaDTO> criteriaPaginationDto;
            if (IsNewSearch)
            {
                criteria.AVNumber = NormalizeAVNumber(criteria.AVNumber);

                ValidateSearchModel(criteria, ModelState);

                if (!ModelState.IsValid)
                {
                    searchModel = await LoadIsolateSearchFilterControlsData(criteria);
                    await _cacheService.RemoveCacheValueAsync(keySearchCriteria);
                    searchModel.IsolateSearchGird = new IsolateSearchGirdViewModel
                    {
                        IsolateSearchResults = new List<IsolateSearchResult>(),
                        Pagination = new PaginationModel()
                    };
                    searchModel.IsFilterApplied = false;
                    return View("IsolateSearch", searchModel);
                }

                criteria = NormalizeCreatedAndReceivedDateRanges(criteria);

                UpdateModelStateValuesAndSearchModel(searchModel, criteria, ModelState);

                criteria.Pagination = new PaginationModel();
                criteriaPaginationDto = new QueryParameters<SearchCriteriaDTO>
                {
                    Filter = _mapper.Map<SearchCriteriaDTO>(criteria),
                    SortBy = criteria.Pagination.SortColumn,
                    Descending = criteria.Pagination.SortDirection,
                    Page = criteria.Pagination.PageNumber,
                    PageSize = criteria.Pagination.PageSize,
                };
            }
            else
            {
                criteriaPaginationDto = await RetriveThePreviousSearchFilter();
                criteria = _mapper.Map<SearchCriteria>(criteriaPaginationDto.Filter);
                criteria.Pagination = new PaginationModel
                {
                    PageNumber = criteriaPaginationDto.Page,
                    PageSize = criteriaPaginationDto.PageSize
                };
            }
            searchModel = await LoadIsolateSearchFilterControlsData(criteria);
            var searchResults = await _isolateSearchService.PerformSearchAsync(criteriaPaginationDto);
            searchModel.IsolateSearchGird = new IsolateSearchGirdViewModel
            {
                IsolateSearchResults = _mapper.Map<List<IsolateSearchResult>>(searchResults.data)
            };
            criteria!.Pagination!.TotalCount = searchResults.TotalCount;
            searchModel.IsolateSearchGird.Pagination = criteria.Pagination;
            searchModel.IsFilterApplied = true;

            await _cacheService.SetCacheValueAsync(keySearchCriteria, JsonConvert.SerializeObject(criteriaPaginationDto));
            return View("IsolateSearch", searchModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetVirusTypesByVirusFamily(Guid? virusFamilyId)
        {
            if (!ModelState.IsValid)
                return Json(new List<SelectListItem>());

            return Json(await GetVirusTypesDropdownList(virusFamilyId));
        }

        [HttpGet]
        public async Task<JsonResult> GetHostBreedsByGroup(Guid? hostSpicyId)
        {
            if (!ModelState.IsValid)
                return Json(new List<SelectListItem>());

            return Json(await GetHostBreedsDropdownList(hostSpicyId));
        }

        [HttpGet]
        public async Task<JsonResult> GetVirusCharacteristicsByVirusType(Guid? virusTypeId)
        {
            if (!ModelState.IsValid)
                return Json(new List<SelectListItem>());

            return Json(await GetVirusCharacteristicsDropdownList(virusTypeId));
        }

        [HttpGet]
        public async Task<JsonResult> GetComparatorsAndListValues(Guid virusCharacteristicId)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    Comparators = new SelectListItem(),
                    ListValues = new SelectListItem()
                });
            }
            var (comparators, listValues) = await GetComparatorsAndListValuesDropDownsList(virusCharacteristicId);
            return Json(new
            {
                Comparators = comparators,
                ListValues = listValues
            });
        }

        [HttpGet]
        public async Task<IActionResult> BindIsolateGridOnPaginationAndSort(int pageNo, string column = "", bool sortOrder = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid parameters.");
            }
            var modelIsolateSearchGird = new IsolateSearchGirdViewModel();
            var criteriaString = await _cacheService.GetCacheValueAsync<string>(keySearchCriteria);
            if (!String.IsNullOrEmpty(criteriaString))
            {
                var criteriaDto = JsonConvert.DeserializeObject<QueryParameters<SearchCriteriaDTO>>(criteriaString);
                if (pageNo != 0)
                {
                    criteriaDto!.Page = pageNo;
                }
                else
                {
                    criteriaDto!.SortBy = column;
                    criteriaDto.Descending = sortOrder;
                }

                var searchResults = await _isolateSearchService.PerformSearchAsync(criteriaDto);
                modelIsolateSearchGird.IsolateSearchResults = _mapper.Map<List<IsolateSearchResult>>(searchResults.data);
                modelIsolateSearchGird.Pagination = new PaginationModel
                {
                    PageNumber = criteriaDto.Page,
                    PageSize = criteriaDto.PageSize,
                    SortColumn = criteriaDto.SortBy,
                    SortDirection = criteriaDto.Descending,
                    TotalCount = searchResults.TotalCount
                };
                await _cacheService.SetCacheValueAsync(keySearchCriteria, JsonConvert.SerializeObject(criteriaDto));
            }

            return PartialView("_IsolateSearchResults", modelIsolateSearchGird);
        }

        [HttpGet]
        public IActionResult BindGirdPagination(PaginationModel pagination)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid pagination parameter");
            }
            return ViewComponent("Pagination", pagination);
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var criteriaString = await _cacheService.GetCacheValueAsync<string>(keySearchCriteria);
            List<IsolateSearchExportViewModel> searchExportRecords = new List<IsolateSearchExportViewModel>();
            var criteriaDto = String.IsNullOrEmpty(criteriaString) ? null : JsonConvert.DeserializeObject<QueryParameters<SearchCriteriaDTO>>(criteriaString);
            if (criteriaDto != null)
            {
                searchExportRecords = _mapper.Map<List<IsolateSearchExportViewModel>>(
                await _isolateSearchService.GetIsolateSearchExportResultAsync(criteriaDto));
            }
            using (var workbook = new XLWorkbook())
            {
                string fileName = $"VIR SearchResults {DateTime.Today.ToString("dMMMyyyy")}";
                var worksheet = workbook.Worksheets.Add(fileName);
                var currentRow = 1;
                // Header
                var properties = typeof(IsolateSearchExportViewModel).GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cell(currentRow, i + 1).Value = properties[i].Name;
                }
                // Data
                foreach (var isolate in searchExportRecords)
                {
                    currentRow++;
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var value = properties[i].GetValue(isolate);
                        worksheet.Cell(currentRow, i + 1).Value = value?.ToString() ?? string.Empty;
                    }
                }
                var range = worksheet.Range(1, 1, currentRow, properties.Length);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                $"{fileName}.xlsx");
                }
            }
        }

        private static List<SelectListItem> GenerateYearsList()
        {
            const int seedYear = 1850;
            int currentYear = DateTime.Now.Year;

            var yearsDto = Enumerable.Range(seedYear, currentYear - seedYear + 1).Reverse().ToList();
            return yearsDto.Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString() }).ToList();
        }

        private static string? NormalizeAVNumber(string? aVNumber)
        {
            if (!String.IsNullOrEmpty(aVNumber))
            {
                aVNumber = aVNumber.ToUpper();
                if (AVNumberUtil.AVNumberIsValidPotentially(aVNumber))
                {
                    aVNumber = AVNumberUtil.AVNumberFormatted(aVNumber);
                }
            }
            return aVNumber;
        }

        private static SearchCriteria NormalizeCreatedAndReceivedDateRanges(SearchCriteria criteria)
        {
            if (criteria.ReceivedFromDate == null ^ criteria.ReceivedToDate == null)
            {
                if (criteria.ReceivedFromDate == null)
                {
                    criteria.ReceivedFromDate = Convert.ToDateTime("01/01/1900");
                }
                if (criteria.ReceivedToDate == null)
                {
                    criteria.ReceivedToDate = DateTime.Today;
                }
            }

            if (criteria.CreatedFromDate == null ^ criteria.CreatedToDate == null)
            {
                if (criteria.CreatedFromDate == null)
                {
                    criteria.CreatedFromDate = Convert.ToDateTime("01/01/1900");
                }
                if (criteria.CreatedToDate == null)
                {
                    criteria.CreatedToDate = DateTime.Today;
                }
            }

            return criteria;
        }

        private static void ValidateSearchModel(SearchCriteria criteria, ModelStateDictionary modelState)
        {
            var context = new ValidationContext(criteria);
            var validationResult = criteria.Validate(context);
            foreach (var validation in validationResult)
            {
                foreach (var menberName in validation.MemberNames.Any() ? validation.MemberNames : new[] { "" })
                {
                    if (validation.ErrorMessage != null)
                        modelState.AddModelError(menberName, validation.ErrorMessage);
                }
            }
        }

        private static void UpdateModelStateValuesAndSearchModel(SearchRepositoryViewModel searchModel, SearchCriteria criteria, ModelStateDictionary modelState)
        {
            modelState.Remove(nameof(criteria.AVNumber));
            modelState.Remove(nameof(criteria.CreatedFromDate));
            modelState.Remove(nameof(criteria.CreatedToDate));
            modelState.Remove(nameof(criteria.ReceivedFromDate));
            modelState.Remove(nameof(criteria.ReceivedToDate));
            searchModel.AVNumber = criteria.AVNumber;
            searchModel.CreatedFromDate = criteria.CreatedFromDate;
            searchModel.CreatedToDate = criteria.CreatedToDate;
            searchModel.ReceivedFromDate = criteria.ReceivedFromDate;
            searchModel.ReceivedToDate = criteria.ReceivedToDate;
        }

        private async Task<SearchRepositoryViewModel> LoadIsolateSearchFilterControlsData(SearchCriteria? criteria)
        {
            SearchRepositoryViewModel searchViewModel = _mapper.Map<SearchRepositoryViewModel>(criteria) ?? new SearchRepositoryViewModel();
            var virusFamilyDto = await _lookupService.GetAllVirusFamiliesAsync();
            var hostSpecyDto = await _lookupService.GetAllHostSpeciesAsync();
            var countryDto = await _lookupService.GetAllCountriesAsync();
            var hostPurposeDto = await _lookupService.GetAllHostPurposesAsync();
            var sampleTypeDto = await _lookupService.GetAllSampleTypesAsync();
            var characteristicsDto = await GetVirusCharacteristicsDropdownList(criteria?.VirusType);
            var characteristicSearchList = new List<CharacteristicSearchViewModel>();
            for (int i = 0; i < 3; i++)
            {
                var (comparators, listValues) = await GetComparatorsAndListValuesDropDownsList(criteria?.CharacteristicSearch[i].Characteristic);
                characteristicSearchList.Add(new CharacteristicSearchViewModel
                {
                    CharacteristicList = characteristicsDto,
                    Characteristic = criteria?.CharacteristicSearch[i].Characteristic,
                    CharacteristicType = criteria?.CharacteristicSearch[i].CharacteristicType,
                    Comparator = criteria?.CharacteristicSearch[i].Comparator,
                    CharacteristicValue1 = criteria?.CharacteristicSearch[i].CharacteristicValue1,
                    CharacteristicValue2 = criteria?.CharacteristicSearch[i].CharacteristicValue2,
                    CharacteristicListValue = criteria?.CharacteristicSearch[i].CharacteristicListValue,
                    CharacteristicValues = listValues,
                    ComparatorList = comparators,
                });
            }

            searchViewModel.VirusFamilyList = virusFamilyDto.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
            searchViewModel.VirusTypeList = await GetVirusTypesDropdownList(criteria?.VirusFamily);
            searchViewModel.HostSpecyList = hostSpecyDto.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            searchViewModel.HostBreedList = await GetHostBreedsDropdownList(criteria?.Group);
            searchViewModel.CountryList = countryDto.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            searchViewModel.HostPurposeList = hostPurposeDto.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
            searchViewModel.SampleTypeList = sampleTypeDto.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            searchViewModel.YearsList = GenerateYearsList();
            searchViewModel.CharacteristicSearch = characteristicSearchList;
            if (criteria == null)
            {
                searchViewModel.IsolateSearchGird = new IsolateSearchGirdViewModel
                {
                    IsolateSearchResults = new List<IsolateSearchResult>(),
                    Pagination = new PaginationModel()
                };
            }

            return searchViewModel;
        }

        private async Task<List<SelectListItem>> GetVirusTypesDropdownList(Guid? virusFamilyId)
        {
            if (SearchCriteria.IsNullOrEmptyGuid(virusFamilyId))
            {
                var virusTypesDto = await _lookupService.GetAllVirusTypesAsync();
                return virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
            }
            else
            {
                var virusTypesDto = await _lookupService.GetAllVirusTypesByParentAsync(virusFamilyId);
                return virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
            }
        }

        private async Task<List<SelectListItem>> GetHostBreedsDropdownList(Guid? hostSpicyId)
        {
            if (SearchCriteria.IsNullOrEmptyGuid(hostSpicyId))
            {
                var hostBreedDto = await _lookupService.GetAllHostBreedsAsync();
                return hostBreedDto.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name }).ToList();
            }
            else
            {
                var hostBreedDto = await _lookupService.GetAllHostBreedsByParentAsync(hostSpicyId);
                return hostBreedDto.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name }).ToList();
            }
        }

        private async Task<List<CustomSelectListItem>> GetVirusCharacteristicsDropdownList(Guid? virusTypeId)
        {
            if (SearchCriteria.IsNullOrEmptyGuid(virusTypeId))
            {
                var virusCharacteristicDto = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();
                return virusCharacteristicDto.Select(c => new CustomSelectListItem { Value = c.Id.ToString(), Text = c.Name.ToString(), DataType = c.DataType.ToString() }).ToList();
            }
            else
            {
                var virusCharacteristicDto = await _virusCharacteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(virusTypeId, false);
                return virusCharacteristicDto.Select(c => new CustomSelectListItem { Value = c.Id.ToString(), Text = c.Name.ToString(), DataType = c.DataType.ToString() }).ToList();
            }
        }

        private async Task<(List<SelectListItem> ComparatorDdl, List<SelectListItem> ListValueDdl)> GetComparatorsAndListValuesDropDownsList(Guid? virusCharacteristicId)
        {
            if (!SearchCriteria.IsNullOrEmptyGuid(virusCharacteristicId))
            {
                var (comparators, listValues) = await _isolateSearchService.GetComparatorsAndListValuesAsync(virusCharacteristicId ?? Guid.Empty);
                var ComparatorsDdl = comparators.Select(c => new SelectListItem { Value = c.ToString(), Text = c.ToString() }).ToList();
                var ListValuesDdl = listValues.Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name.ToString() }).ToList();
                return (ComparatorsDdl, ListValuesDdl);
            }
            else
            {
                return (new List<SelectListItem>(), new List<SelectListItem>());
            }

        }

        private async Task<QueryParameters<SearchCriteriaDTO>> RetriveThePreviousSearchFilter()
        {
            QueryParameters<SearchCriteriaDTO> previousSearch = new QueryParameters<SearchCriteriaDTO>();
            var criteriaString = await _cacheService.GetCacheValueAsync<string>(keySearchCriteria);
            if (criteriaString != null)
            {
                previousSearch = JsonConvert.DeserializeObject<QueryParameters<SearchCriteriaDTO>>(criteriaString)
                    ?? new QueryParameters<SearchCriteriaDTO>();
                return previousSearch;
            }
            return previousSearch;
        }
    }
}
