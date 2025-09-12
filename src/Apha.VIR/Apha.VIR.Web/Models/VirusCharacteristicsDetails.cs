using System.ComponentModel.DataAnnotations;
using Apha.VIR.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class VirusCharacteristicDetails
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Name must be entered.")]
        [StringLength(255)]
        public string? Name { get; set; }

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
        public byte[] LastModified { get; set; } = null!;
        public List<SelectListItem>? CharacteristicTypeNameList { get; set; }

        public VirusCharacteristicDetails()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            CharacteristicTypeNameList = new List<SelectListItem>();
            LastModified = Array.Empty<byte>(); // Initialize LastModified to avoid nullability issues
        }
    }
}
