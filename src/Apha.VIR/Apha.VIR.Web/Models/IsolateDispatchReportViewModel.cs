using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class IsolateDispatchReportViewModel
    {
        [Display(Name = "Date From")]
        [Required(ErrorMessage = "Date From must be entered")]
        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; }

        [Display(Name = "Date To")]
        [Required(ErrorMessage = "Date To must be entered")]
        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; }

        public List<IsolateDispatchReportModel> ReportData { get; set; }

        public IsolateDispatchReportViewModel()
        {
            ReportData = new List<IsolateDispatchReportModel>();
        }
    }
}
