﻿namespace Apha.VIR.Core.Pagination
{
    public class PaginationParameters
    {
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PaginationParameters(string? search = null, string? sortBy = "", bool descending = false, int page = 1, int pageSize = 10)
        {
            Search = search;
            SortBy = sortBy;
            Descending = descending;
            Page = page;
            PageSize = pageSize;
        }
    }
}
