namespace Apha.VIR.Web.Models;

public class LookupItemListViewModel
{
    public List<LookupItemModel> LookupItems { get; set; }=new List<LookupItemModel>();
    public PaginationModel? Pagination { get; set; }
}
