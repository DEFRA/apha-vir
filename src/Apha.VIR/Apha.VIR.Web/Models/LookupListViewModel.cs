namespace Apha.VIR.Web.Models;

public class LookupListViewModel
{
    public string LookupName { get; set; }=string.Empty;
    public Guid LookupId { get; set; } = Guid.Empty;
    public bool IsReadOnly { get; set; }
    public LookupItemListViewModel LookupItemResult { get; set; }=new LookupItemListViewModel();
}
