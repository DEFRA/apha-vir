namespace Apha.VIR.Web.Models.Lookup;

public class SenderListViewModel
{
    public List<SenderViewModel> Senders { get; set; } = new List<SenderViewModel>();
    public PaginationModel? Pagination { get; set; }
}
