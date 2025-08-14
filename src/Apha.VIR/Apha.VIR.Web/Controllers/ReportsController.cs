using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportService _iReportService;
        private readonly IMapper _mapper;

        public ReportsController(IReportService iReportService, IMapper mapper)
        {
            _iReportService = iReportService;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IsolateDispatch()
        {
            var model = new IsolateDispatchReportViewModel
            {
                DateFrom = DateTime.Today,
                DateTo = DateTime.Today
            };
            return View("IsolateDispatchReport", model);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(IsolateDispatchReportViewModel model)
        {
            ModelState.Remove(nameof(model.ReportData));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _iReportService.GetDispatchesReportAsync(model.DateFrom, model.DateTo);

            var reportData = _mapper.Map<IEnumerable<IsolateDispatchReportModel>>(result);

            var viewModel = new IsolateDispatchReportViewModel
            {
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                ReportData = reportData.ToList()
            };
    
            return View("IsolateDispatchReport", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? dateFrom, DateTime? dateTo)
        {
            ModelState.Clear();

            if (dateFrom == null)
            {
                ModelState.AddModelError(nameof(dateFrom), "Date From must be entered");
            }
            if (dateTo == null)
            {
                ModelState.AddModelError(nameof(dateTo), "Date To must be entered");
            }

            if (!ModelState.IsValid)
            {
                var viewmodel = new IsolateDispatchReportViewModel
                {
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };
                return View("IsolateDispatchReport", viewmodel);
            }

            var result = await _iReportService.GetDispatchesReportAsync(dateFrom, dateTo);

            var reportData = _mapper.Map<IEnumerable<IsolateDispatchReportModel>>(result);

            using (var workbook = new XLWorkbook())
            {
                string fileName = $"VIR IsolateDispatchReport {DateTime.Today.Day}{DateTime.Today.ToString("MMMM")}{DateTime.Today.Year}";
                string sheetName = $"VIR IsolateDispatchReport {DateTime.Today.Day}{DateTime.Today.ToString("MMM")}";

                var worksheet = workbook.Worksheets.Add(sheetName);
                var currentRow = 1;
                // Header
                var properties = typeof(IsolateDispatchReportModel).GetProperties()
                  .Where(p => p.Name != "DispatchedBy").ToList(); // Exclude non export prop

                for (int i = 0; i < properties.Count; i++)
                {
                    var displayAttr = properties[i].GetCustomAttribute<DisplayAttribute>();
                    worksheet.Cell(currentRow, i + 1).Value = displayAttr?.Name ?? properties[i].Name;
                }

                // Data
                var filteredList = reportData.Select(p => new IsolateDispatchReportModel
                {
                    AVNumber = p.AVNumber,
                    Nomenclature = p.Nomenclature,
                    NoOfAliquots = p.NoOfAliquots,
                    PassageNumber = p.PassageNumber,
                    Recipient = p.Recipient,
                    RecipientName = p.RecipientName,
                    RecipientAddress = p.RecipientAddress,
                    ReasonForDispatch = p.ReasonForDispatch,
                    DispatchedDate = p.DispatchedDate,
                    DispatchedByName = p.DispatchedByName
                }).ToList();

                foreach (var isolate in filteredList)
                {
                    currentRow++;
                    for (int i = 0; i < properties.Count; i++)
                    {
                        var value = properties[i].GetValue(isolate);
                        worksheet.Cell(currentRow, i + 1).Value = value?.ToString() ?? string.Empty;
                    }
                }

                var range = worksheet.Range(1, 1, currentRow, properties.Count);
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
    }
}
