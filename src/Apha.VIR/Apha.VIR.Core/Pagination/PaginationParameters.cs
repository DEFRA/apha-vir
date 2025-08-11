namespace Apha.VIR.Core.Pagination
{
    public class PaginationParameters<TFilter>
    {
        public TFilter? Filter { get; set; }
        public string? GlobalSearch { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PaginationParameters(string? golobalSearch = null, string? sortBy = "", bool descending = false, int page = 1, int pageSize = 10)
        {
            GlobalSearch = golobalSearch;
            SortBy = sortBy;
            Descending = descending;
            Page = page;
            PageSize = pageSize;
        }
    }
}
