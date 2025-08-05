using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Apha.VIR.Web.Models
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
        public int? YearOfIsolation { get; set; }
        public int? YearOfSubmissionRecieved { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ReceivedFromDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? ReceivedToDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? CreatedFromDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? CreatedToDate { get; set; }
        public List<CharacteristicCriteria> CharacteristicSearch { get; set; } = new List<CharacteristicCriteria>();
        [ValidateNever]
        public PaginationModel? Pagination { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (string.IsNullOrEmpty(AVNumber) &&
                (IsNullOrEmptyGuid(VirusFamily)) &&
                (IsNullOrEmptyGuid(VirusType)) &&
                (IsNullOrEmptyGuid(Group)) &&
                (IsNullOrEmptyGuid(Species)) &&
                (IsNullOrEmptyGuid(CountryOfOrigin)) &&
                (IsNullOrEmptyGuid(HostPurpose)) &&
                (IsNullOrEmptyGuid(SampleType)) &&
                YearOfIsolation == 0 &&
                !ReceivedFromDate.HasValue &&
                !ReceivedToDate.HasValue &&
                !CreatedFromDate.HasValue &&
                !CreatedToDate.HasValue &&
                CharacteristicSearch.All(c => (IsNullOrEmptyGuid(c.Characteristic))))
            {
                results.Add(new ValidationResult("You must supply at least one criteria for the Search."));
            }
            else
            {
                if (!string.IsNullOrEmpty(AVNumber) && !Submission.IsAVNumberValid(AVNumber))
                {
                    results.Add(new ValidationResult("The correct format for an AV number is either AVNNNNNN-YY, PDNNNN-NN SINNNNNN-YY. Please amend and try again"));
                }

                if (ReceivedFromDate > DateTime.Today)
                {
                    results.Add(new ValidationResult("The 'Received From' date must be before today's date. Please amend and try again"));
                }
                else if (ReceivedFromDate.HasValue && ReceivedToDate.HasValue && ReceivedFromDate > ReceivedToDate)
                {
                    results.Add(new ValidationResult("The 'Received From' date must be before the 'Received To' date. Please amend and try again"));
                }

                if (CreatedFromDate > DateTime.Today)
                {
                    results.Add(new ValidationResult("The 'Created From' date must be before today's date. Please amend and try again"));
                }
                else if (CreatedFromDate.HasValue && CreatedToDate.HasValue && CreatedFromDate > CreatedToDate)
                {
                    results.Add(new ValidationResult("The 'Created From' date must be before the 'Created To' date. Please amend and try again"));
                }

                foreach (CharacteristicCriteria characteristicCriteria in CharacteristicSearch)
                {
                    if (characteristicCriteria.CharacteristicType == "Numeric")
                    {
                        bool isValid1 = string.IsNullOrEmpty(characteristicCriteria.CharacteristicValue1) || double.TryParse(characteristicCriteria.CharacteristicValue1, out _);
                        bool isValid2 = string.IsNullOrEmpty(characteristicCriteria.CharacteristicValue2) || double.TryParse(characteristicCriteria.CharacteristicValue2, out _);

                        if (!isValid1 || !isValid2)
                        {
                            results.Add(new ValidationResult("The characteristic you have selected is numeric, therefore the value(s) to search must also be numeric. Please amend and try again"));
                            break;
                        }
                    }
                }
            }

            return results;
        }        

        private static bool IsNullOrEmptyGuid(Guid? guid)
        {
            return !guid.HasValue || guid.Value == Guid.Empty;
        }
    }
}
