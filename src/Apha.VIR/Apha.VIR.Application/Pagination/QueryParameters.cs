﻿namespace Apha.VIR.Application.Pagination
{
    public class QueryParameters<TFilter>
    {
        private int _pageSize = 10;
        private int _page = 1;
        private const int MaxPageSize = 100;

        public TFilter Filter { get; set; }
        public string? GlobalSearch { get; set; }
        public string? SortBy { get; set; } = "";
        public bool Descending { get; set; } = false;

        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
