using System.Collections.Generic;

namespace Apha.VIR.Web.Models
{
  
    public class IsolateCharacteristicsViewModel
    {
        public int Id { get; set; }
        public int IsolateId { get; set; }
        public int CharacteristicId { get; set; }
        public string CharacteristicName { get; set; }
        public string CharacteristicType { get; set; }
        public string Value { get; set; }
        public List<string> Options { get; set; } = new List<string>();
    }
}
