using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Web.Models.VirusCharacteristic
{
    public class VirusCharacteristicListEntryEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public Guid VirusCharacteristicId { get; set; }

        [Required(ErrorMessage = "Name must be entered.")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        public byte[]? LastModified { get; set; }
        
    }
}
