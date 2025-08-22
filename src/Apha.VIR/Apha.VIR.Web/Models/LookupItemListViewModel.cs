namespace Apha.VIR.Web.Models;

public class LookupItemListViewModel
{
    public bool HasParent { get; set; }
    public bool HasAlternateName { get; set; }
    public bool IsSMSRelated { get; set; }
    public List<LookupItemModel> LookupItems { get; set; }=new List<LookupItemModel>();
    public PaginationModel? Pagination { get; set; }
}
