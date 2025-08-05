namespace Apha.VIR.Web.Models
{
    public class IsolateSearchGirdViewModel
    {
        public List<IsolateSearchResult> IsolateSearchResults { get; set; }
        public PaginationModel Pagination { get; set; }
        public string SortOrderFor(string column)
        {
            return Pagination.SortColumn == column && Pagination.SortDirection ? "0" : "1";
        }
    }
}
