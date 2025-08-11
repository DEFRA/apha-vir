namespace Apha.VIR.Web.Models
{
    public class PaginationModel
    {
        public int TotalCount { get; set; } = 0;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public bool SortDirection { get; set; } = false;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
