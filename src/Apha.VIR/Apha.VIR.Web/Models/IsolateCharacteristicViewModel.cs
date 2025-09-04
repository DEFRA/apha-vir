using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class IsolateCharacteristicViewModel
    {
        public Guid? IsolateId { get; set; }
        public Guid? CharacteristicId { get; set; }
        public Guid? CharacteristicIsolateId { get; set; }
        public Guid? VirusCharacteristicId { get; set; }
        public string? CharacteristicName { get; set; }
        public string? CharacteristicType { get; set; }
        public string? CharacteristicValue { get; set; }
        public bool? CharacteristicDisplay { get; set; }
        public string? SMSColumnId { get; set; }
        public string? CharacteristicPrefix { get; set; }
        public byte[] LastModified { get; set; } = null!;
        public string? AVNumber { get; set; }
        public List<SelectListItem>? CharacteristicValueDropDownList { get; set; }
    }
}
