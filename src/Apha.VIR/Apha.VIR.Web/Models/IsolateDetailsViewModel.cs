namespace Apha.VIR.Web.Models
{
    public class IsolateDetailsViewModel
    {

        public Guid? IsolateId { get; set; }
        public string AVNumber { get; set; }
        public string Nomenclature { get; set; }
        public string CheckedBy { get; set; }
        public Guid? VirusFamilyId { get; set; }
        public Guid? VirusTypeId { get; set; }
        public int? YearOfIsolation { get; set; }
        public bool IsMixedIsolate { get; set; }
        public Guid? IsolationMethodId { get; set; }
        public bool AntiserumProduced { get; set; }
        public bool AntigenProduced { get; set; }
        public string PhylogeneticAnalysis { get; set; }
        public string MTALocation { get; set; }
        public string Comment { get; set; }
        public bool ValidToIssue { get; set; }
        public string WhyNotValidToIssue { get; set; }
        public Guid? FreezerId { get; set; }
        public Guid? TrayId { get; set; }
        public string Viability { get; set; }
        public DateTime? DateChecked { get; set; }

        public string Well { get; set; }
        public int NoOfAliquots { get; set; }
        public bool MaterialTransferAgreement { get; set; }
        public bool SampleAvailable { get; set; }
        public int FirstViablePassageNumber { get; set; }

        public List<VirusFamily> VirusFamilies { get; set; }
        public List<Person> personList { get; set; }

        public List<Viability> ViabilityList { get; set; }

        public List<IsolationMethod> IsolationMethods { get; set; }

        public List<VirusType> VirusTypes { get; set; }

        public List<Freezer> FreezerList { get; set; }

        public List<Tray> TrayList { get; set; }

        public string? IsoSMSReferenceNumber { get; set; }

        public string? PhylogeneticFile { get; set; }

        public Guid PreviousCurrentViability { get; set; }
        public DateTime? PreviousDateChecked { get; set; }

        public Guid PreviousCheckedBy { get; set; }

    }

    public class VirusFamily
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class Viability
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class IsolationMethod
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class VirusType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid FamilyId { get; set; }
    }

    public class Freezer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class Tray
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
