using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class IsolateDispatchHistoryViewModel
    {
        public string? Nomenclature { get; set; }

        public IEnumerable<IsolateDispatchHistory>? DispatchHistoryRecords { get; set; }

    }


    public class IsolateDispatchHistory
    {
        public Guid DispatchIsolateId { get; set; }

        public Guid IsolateId { get; set; }

        public Guid DispatchId { get; set; }

        public int NoOfAliquots { get; set; }

        public int PassageNumber { get; set; }

        public string? Recipient { get; set; }

        public string? RecipientName { get; set; }

        public string? RecipientAddress { get; set; }

        public string? ReasonForDispatch { get; set; }

        public DateTime DispatchedDate { get; set; }

        public string? DispatchedByName { get; set; }

        [Display(Name = "AV Number")]
        public string? Avnumber { get; set; }

        public string? Nomenclature { get; set; }

        public Byte[]? LastModified { get; set; }

    }
}
