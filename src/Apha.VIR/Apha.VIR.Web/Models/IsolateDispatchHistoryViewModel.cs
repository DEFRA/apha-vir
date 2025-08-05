using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models
{
    public class IsolateDispatchHistoryViewModel
    {
        public string Nomenclature { get; set; }

        public IEnumerable<IsolateDispatchHistory> DispatchHistoryRecords { get; set; }

    }


    public class IsolateDispatchHistory
    {
        public Guid DispatchIsolateId { get; set; }

        public Guid IsolateId { get; set; }

        public Guid DispatchId { get; set; }

        [Required]
        [Display(Name = "Number of Aliquots")]
        public int NoOfAliquots { get; set; }

        [Display(Name = "Passage Number")]
        public int PassageNumber { get; set; }

        public string Recipient { get; set; }

        [Display(Name = "Recipient Name")]
        public string RecipientName { get; set; }

        [Display(Name = "Recipient Address")]
        public string RecipientAddress { get; set; }


        [Display(Name = "Reason for Dispatch")]
        public string ReasonForDispatch { get; set; }

        [Required]
        [Display(Name = "Dispatched Date")]
        [DataType(DataType.Date)]
        public DateTime DispatchedDate { get; set; }

        [Required]
        [Display(Name = "Dispatched By")]
        public string DispatchedByName { get; set; }

        //public byte[] LastModified { get; set; }

        [Display(Name = "AV Number")]
        public string Avnumber { get; set; }

        public string Nomenclature { get; set; }

        public Byte[] LastModified { get; set; }

    }
}
