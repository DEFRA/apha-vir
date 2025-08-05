namespace Apha.VIR.Web.Models
{
    public class PaginationModel
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 2;
        public string? SortColumn { get; set; }
        public bool SortDirection { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
