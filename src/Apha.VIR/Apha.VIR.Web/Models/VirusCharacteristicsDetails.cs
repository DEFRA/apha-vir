using System.ComponentModel.DataAnnotations;
using Apha.VIR.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class VirusCharacteristicDetails
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public Guid CharacteristicType { get; set; }
        public bool NumericSort { get; set; }
        public bool DisplayOnSearch { get; set; }
        public string? Prefix { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public int? DecimalPlaces { get; set; }
        public int? Length { get; set; }
        public int? CharacteristicIndex { get; set; }
        public byte[] LastModified { get; set; } = null!;
        public string DataType { get; set; } = null!;
        public string VirusCharacteristicTypeName { get; set; } = null!;
        public List<SelectListItem>? CharacteristicTypeNameList { get; set; }

        public VirusCharacteristicDetails()
        {
            Id = Guid.NewGuid();
            CharacteristicTypeNameList = new List<SelectListItem>();
        }

    }
}
