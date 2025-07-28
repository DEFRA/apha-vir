namespace Apha.VIR.Core.Pagination
{
    public class PagedData<T>
    {
        public IReadOnlyCollection<T> Items { get; }
        public int TotalCount { get; }

        public PagedData(IReadOnlyCollection<T> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }
    }
}
