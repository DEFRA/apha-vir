using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Web.Models.VirusCharacteristic
{
    public class VirusCharacteristicAssociationViewModel
    {
        public Guid? SelectedFamilyId { get; set; }
        public Guid? SelectedVirusTypeId { get; set; }
        public IEnumerable<LookupItemDto> VirusFamilies { get; set; } = [];
        public IEnumerable<LookupItemDto> VirusTypes { get; set; } = [];
        public IEnumerable<VirusCharacteristicDto> CharacteristicsPresent { get; set; } = [];
        public IEnumerable<VirusCharacteristicDto> CharacteristicsAbsent { get; set; } = [];
    }
}
