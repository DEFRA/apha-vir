using System.ComponentModel.DataAnnotations;

namespace Apha.VIR.Core.Entities
{
    public class SearchCriteria
    {
        public string? AVNumber { get; set; }
        public Guid? VirusFamily { get; set; }
        public Guid? VirusType { get; set; }
        public Guid? Group { get; set; }
        public Guid? Species { get; set; }
        public Guid? CountryOfOrigin { get; set; }
        public Guid? HostPurpose { get; set; }
        public Guid? SampleType { get; set; }
        public int YearOfIsolation { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ReceivedFromDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ReceivedToDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? CreatedFromDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? CreatedToDate { get; set; }
        public List<CharacteristicCriteria> CharacteristicSearch { get; set; } = new List<CharacteristicCriteria>();
    }
}
