using System.ComponentModel.DataAnnotations;
using Apha.VIR.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Models
{
    public class SubmissionSenderViewModel
    {
        public Guid? SenderId { get; set; }
        [Required(ErrorMessage = "Sender must be entered")]
        public string? SenderName { get; set; }
        [Required(ErrorMessage = "Sender Organisation must be entered")]
        public string? SenderOrganisation { get; set; }
        [Required(ErrorMessage = "Sender must be entered")]
        public string? SenderAddress { get; set; }
        public Guid? Country { get; set; }
        public string? CountryName { get; set; }
        public List<SelectListItem>? CountryList { get; set; }
        private string? ShortOrg
        {
            get
            {
                return SenderOrganisation!.Length > 50
                ? string.Concat(SenderOrganisation.Substring(0, 47), "...")
                : SenderOrganisation;
            }
        }
        public string? SenderAndOrg
        {
            get
            {
                return $"{SenderName} ({ShortOrg}) {CountryName}";
            }
        }
        public string? OrgAndSender
        {
            get
            {
                return $"{ShortOrg} ({SenderName}) {CountryName}";
            }
        }
    }
}
