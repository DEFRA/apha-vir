using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class IsolateDispatchReportViewModel
    {
        [Display(Name = "Date From")]
        [Required(ErrorMessage = "Date From must be entered")]
        [DataType(DataType.Date, ErrorMessage = "Date From must be a valid date")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Date To")]
        [Required(ErrorMessage = "Date To must be entered")]
        [DataType(DataType.Date, ErrorMessage = "Date To must be a valid date")]
        public DateTime? DateTo { get; set; }

        public List<IsolateDispatchReportModel> ReportData { get; set; }

        public IsolateDispatchReportViewModel()
        {
            ReportData = new List<IsolateDispatchReportModel>();
        }
    }
}
