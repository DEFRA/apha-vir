namespace Apha.VIR.Web.Models;

public class LookupItemViewModel
{
    public string LookupName { get; set; }=string.Empty;
    public Guid LookupId { get; set; } = Guid.Empty;
    public LookupItemListViewModel LookupItemResult { get; set; }=new LookupItemListViewModel();
}
