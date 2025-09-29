using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class IsolateDispatchConfirmatioViewModel
    {
        public int RemainingAliquots { get; set; }
        public string DispatchConfirmationMessage { get; set; } = string.Empty;
        public required IEnumerable<IsolateDispatchHistory> DispatchHistorys { get; set; }
        public string AVNumber { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
