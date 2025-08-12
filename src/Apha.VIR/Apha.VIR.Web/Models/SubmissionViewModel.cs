namespace Apha.VIR.Web.Models
{
    public class SubmissionViewModel
    {
        public int Id { get; set; }
        public string AVNumber { get; set; }
        public string SendersReferenceNumber { get; set; }
        public string SenderOrganisation { get; set; }
        public string CountryOfOriginName { get; set; }
        public List<Sample> Samples { get; set; }
        public List<Isolate> Isolates { get; set; }
        public bool ShowLetterLink { get; set; } 
        public bool IsLetterRequired { get; set; }

        public bool ShowDeleteLink { get; set; }

        public string SubmissionValidationMessage { get; set; } 

        public string WarnSMSubmissionMessage { get; set; } 

        public string SampleValidationMessage { get; set; }

        public string IsolatesHeader { get; set; } 

        public string IsolateValidationMessage { get; set; }

    }

    public class Sample
    {
        public int Id { get; set; }
        public string SampleNumber { get; set; }
        public string SMSReferenceNumber { get; set; }
        public string SenderReferenceNumber { get; set; }
        public string HostSpeciesName { get; set; }
        public string HostBreedName { get; set; }
    }

    public class Isolate
    {
        public int Id { get; set; }
        public string SampleNumber { get; set; }
        public string SMSReferenceNumber { get; set; }
        public string SenderReferenceNumber { get; set; }
        public string TypeName { get; set; }
        public int YearOfIsolation { get; set; }
        public string Characteristics { get; set; }
        public string Nomenclature { get; set; }

        public bool isUniqueNomenclature { get; set; }
    }
}
