using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class CharacteristicSearchViewModel
    {
        public List<CustomSelectListItem>? CharacteristicList { get; set; }
        public string? Characteristic { get; set; }
        public string? CharacteristicType { get; set; }
        public string? Comparator { get; set; }
        public string? CharacteristicValue1 { get; set; }
        public string? CharacteristicValue2 { get; set; }
        public string? CharacteristicListValue { get; set; }
        public List<SelectListItem>? CharacteristicValues { get; set; }
    }
}
