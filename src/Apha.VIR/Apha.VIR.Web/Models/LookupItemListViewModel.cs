namespace Apha.VIR.Web.Models;

public class LookupItemListViewModel
{
    public bool ShowParent { get; set; }
    public bool ShowAlternateName { get; set; }
    public bool ShowSMSRelated { get; set; }
    public List<LookupItemModel> LookupItems { get; set; }=new List<LookupItemModel>();
    public PaginationModel? Pagination { get; set; }
}
