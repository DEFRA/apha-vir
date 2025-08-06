using System.ComponentModel.DataAnnotations;
using System.Data;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Web.Models;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
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
        private readonly IMapper _mapper;

        public SearchRepositoryController(ILookupService lookupService, IVirusCharacteristicService virusCharacteristicService, IIsolateSearchService isolateSearchService, IMapper mapper)
        {
            _lookupService = lookupService;
            _virusCharacteristicService = virusCharacteristicService;
            _isolateSearchService = isolateSearchService;            
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var searchModel = await LoadIsolateSearchFilterControlsData(null);
            return View("IsolateSearch", searchModel);
        }
        
        public async Task<IActionResult> Search(SearchCriteria criteria)
        {
            criteria.AVNumber = NormalizeAVNumber(criteria.AVNumber);

            var context = new ValidationContext(criteria);
            var validationResult = criteria.Validate(context);
            foreach (var validation in validationResult)
            {
                foreach (var menberName in validation.MemberNames.Any() ? validation.MemberNames : new[] { "" })
                {
                    ModelState.AddModelError(menberName, validation.ErrorMessage);
                }
            }

            var searchModel = await LoadIsolateSearchFilterControlsData(criteria);

            if (!ModelState.IsValid)
            {
                TempData.Remove("SearchCriteria");
                searchModel.IsolateSearchGird = new IsolateSearchGirdViewModel
                {
                    IsolateSearchResults = new List<IsolateSearchResult>(),
                    Pagination = new PaginationModel()
                };
                return View("IsolateSearch", searchModel);
            }

            criteria = NormalizeCreatedAndReceivedDateRanges(criteria);

            searchModel = UpdateModelStateValuesAndSearchModel(searchModel, criteria, ModelState);           

            criteria.Pagination = new PaginationModel();
            var criteriaPaginationDto = new QueryParameters<SearchCriteriaDTO>
            {
                Filter = _mapper.Map<SearchCriteriaDTO>(criteria),
                SortBy = criteria.Pagination.SortColumn,
                Descending = criteria.Pagination.SortDirection,
                Page = criteria.Pagination.PageNumber,
                PageSize = criteria.Pagination.PageSize,
            };

            var searchResults = await _isolateSearchService.PerformSearchAsync(criteriaPaginationDto);
            searchModel.IsolateSearchGird = new IsolateSearchGirdViewModel
            {
                IsolateSearchResults = _mapper.Map<List<IsolateSearchResult>>(searchResults.data)
            };
            criteria.Pagination.TotalCount = searchResults.TotalCount;
            searchModel.IsolateSearchGird.Pagination = criteria.Pagination;

            TempData["SearchCriteria"] = JsonConvert.SerializeObject(criteriaPaginationDto);
            return View("IsolateSearch", searchModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetVirusTypesByVirusFamily(string? virusFamilyId)
        {
            if (!String.IsNullOrEmpty(virusFamilyId))
            {
                var virusTypesDto = await _lookupService.GetAllVirusTypesByParentAsync(virusFamilyId);
                return Json(virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList());
            }
            else
            {
                var virusTypesDto = await _lookupService.GetAllVirusTypesAsync();
                return Json(virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetHostBreedsByGroup(string? hostSpicyId)
        {
            if (!String.IsNullOrEmpty(hostSpicyId))
            {
                var hostBreedDto = await _lookupService.GetAllHostBreedsByParentAsync(hostSpicyId);
                return Json(hostBreedDto.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name }).ToList());
            }
            else
            {
                var hostBreedDto = await _lookupService.GetAllHostBreedsAsync();
                return Json(hostBreedDto.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name }).ToList());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetVirusCharacteristicsByVirusType(string? virusTypeId)
        {
            if (!String.IsNullOrEmpty(virusTypeId))
            {
                var virusCharacteristicDto = await _virusCharacteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(virusTypeId, false);
                return Json(virusCharacteristicDto.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name.ToString() }).ToList());
            }
            else
            {
                var virusCharacteristicDto = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();
                return Json(virusCharacteristicDto.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name.ToString() }).ToList());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetComparatorsAndListValues(Guid virusCharacteristicId)
        {
            if (!ModelState.IsValid)
            {
                return Json(new {
                    Comparators = new SelectListItem(),
                    ListValues = new SelectListItem()
                });
            }
            var (comparators, listValues) = await _isolateSearchService.GetComparatorsAndListValuesAsync(virusCharacteristicId);
            return Json(new
            {
                Comparators = comparators.Select(c => new SelectListItem { Value = c.ToString(), Text = c.ToString() }).ToList(),
                ListValues = listValues.Select(v => new SelectListItem { Value = v.Id.ToString(), Text = v.Name.ToString() }).ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> BindIsolateGridOnPaginationAndSort(int pageNo, string column, bool sortOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid parameters.");
            }
            var modelIsolateSearchGird = new IsolateSearchGirdViewModel();
            var criteriaString = TempData.Peek("SearchCriteria") as string;
            if (!String.IsNullOrEmpty(criteriaString))
            {
                var criteriaDto = JsonConvert.DeserializeObject<QueryParameters<SearchCriteriaDTO>>(criteriaString);
                if (pageNo != 0)
                {
                    criteriaDto.Page = pageNo;
                }
                else
                {
                    criteriaDto.SortBy = column;
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
                TempData["SearchCriteria"] = JsonConvert.SerializeObject(criteriaDto);
            }

            return PartialView("_IsolateSearchResults", modelIsolateSearchGird);
        }

        [HttpGet]
        public IActionResult BindGirdPagination(PaginationModel pagination)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("Invalid pagination parameter");
            }
            return ViewComponent("Pagination", pagination);
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var criteriaString = TempData.Peek("SearchCriteria") as string;
            if (!String.IsNullOrEmpty(criteriaString))
            {
                var criteriaDto = JsonConvert.DeserializeObject<QueryParameters<SearchCriteriaDTO>>(criteriaString);
                var searchExportRecords = _mapper.Map<List<IsolateSearchExportViewModel>>(
                    await _isolateSearchService.GetIsolateSearchExportResultAsync(criteriaDto));
                using (var workbook = new XLWorkbook())
                {
                    string fileName = $"VIR SearchResults {DateTime.Today.ToString("dMMMMyyyy")}";
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
            else
                return null;
        }

        private static List<int> GenerateYearsList()
        {
            const int seedYear = 1850;
            int currentYear = DateTime.Now.Year;

            return Enumerable.Range(seedYear, currentYear - seedYear + 1).Reverse().ToList();
        }

        private static string? NormalizeAVNumber(string? aVNumber)
        {
            if (!String.IsNullOrEmpty(aVNumber))
            {
                aVNumber = aVNumber.ToUpper();
                if (Submission.AVNumberIsValidPotentially(aVNumber))
                {
                    aVNumber = Submission.AVNumberFormatted(aVNumber);
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

        private static SearchRepositoryViewModel UpdateModelStateValuesAndSearchModel(SearchRepositoryViewModel searchModel, SearchCriteria criteria, ModelStateDictionary modelState)
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
            return searchModel;
        }

        private async Task<SearchRepositoryViewModel> LoadIsolateSearchFilterControlsData(SearchCriteria? criteria)
        {
            SearchRepositoryViewModel searchViewModel = new SearchRepositoryViewModel();
            var virusFamilyDto = await _lookupService.GetAllVirusFamiliesAsync();
            var virusTypesDto = await _lookupService.GetAllVirusTypesAsync();
            var hostSpecyDto = await _lookupService.GetAllHostSpeciesAsync();
            var hostBreedDto = await _lookupService.GetAllHostBreedsAsync();
            var countryDto = await _lookupService.GetAllCountriesAsync();
            var hostPurposeDto = await _lookupService.GetAllHostPurposesAsync();
            var sampleTypeDto = await _lookupService.GetAllSampleTypesAsync();
            var yearsDto = GenerateYearsList();
            var virusCharacteristicDto = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();

            searchViewModel.VirusFamilyList = virusFamilyDto.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
            searchViewModel.VirusTypeList = virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
            searchViewModel.HostSpecyList = hostSpecyDto.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            searchViewModel.HostBreedList = hostBreedDto.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name }).ToList();
            searchViewModel.CountryList = countryDto.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            searchViewModel.HostPurposeList = hostPurposeDto.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
            searchViewModel.SampleTypeList = sampleTypeDto.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            searchViewModel.YearsList = yearsDto.Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString() }).ToList();
            searchViewModel.CharacteristicSearch = Enumerable.Range(0, 3).Select(i => new CharacteristicSearchViewModel
            {
                CharacteristicList = virusCharacteristicDto.Select(c => new CustomSelectListItem { Value = c.Id.ToString(), Text = c.Name.ToString(), DataType = c.DataType.ToString() }).ToList()
            }).ToList();
            if(criteria == null)
            {
                searchViewModel.IsolateSearchGird = new IsolateSearchGirdViewModel
                {
                    IsolateSearchResults = new List<IsolateSearchResult>(),
                    Pagination = new PaginationModel()
                };
            }
            
            return searchViewModel;
        }
    }
}
