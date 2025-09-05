using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Web.Models.VirusCharacteristic
{
    public class VirusFamilyAndTypeViewModel
    {
        public Guid? SelectedFamilyId { get; set; }
        public Guid? SelectedTypeId { get; set; }
        public IEnumerable<LookupItemDTO> VirusFamilies { get; set; } = [];
        public IEnumerable<LookupItemDTO> VirusTypes { get; set; } = [];
        public IEnumerable<VirusCharacteristicDTO> CharacteristicsPresent { get; set; } = [];
        public IEnumerable<VirusCharacteristicDTO> CharacteristicsAbsent { get; set; } = [];
    }
}
