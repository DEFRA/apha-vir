namespace Apha.VIR.Web.Models.Lookup;

public class SenderListViewModel
{
    public List<SenderMViewModel> Senders { get; set; } = new List<SenderMViewModel>();
    public PaginationModel? Pagination { get; set; }
}
