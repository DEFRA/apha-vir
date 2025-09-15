using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class VirusCharacteristicDetails
    {
        public Guid Id { get; set; }= Guid.Empty;
        
        [Required(ErrorMessage = "Name must be entered.")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid? CharacteristicType { get; set; }
        public bool NumericSort { get; set; }
        public bool DisplayOnSearch { get; set; }
        public string? Prefix { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public int? DecimalPlaces { get; set; }
        public int? Length { get; set; }

        [Required(ErrorMessage = "Display Order must be entered.")]
        public int? CharacteristicIndex { get; set; }
        public byte[] LastModified { get; set; } = Array.Empty<byte>();
        public List<SelectListItem>? CharacteristicTypeNameList { get; set; } = new List<SelectListItem>();
    }
}
