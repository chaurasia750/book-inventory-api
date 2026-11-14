namespace BookInventory.BuildingBlocks.Domain.Models
{
    public class PagedList<T> : IPagedList<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int IndexFrom { get; set; }
        public List<T> Items { get; set; } = [];

        public bool HasPreviousPage => PageIndex - IndexFrom > 0;
        public bool HasNextPage => PageIndex - IndexFrom + 1 < TotalPages;

        public PagedList() { }

        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int indexFrom)
            : this(source.AsQueryable(), pageIndex, pageSize, indexFrom) { }

        public PagedList(IQueryable<T> source, int pageIndex, int pageSize, int indexFrom)
        {
            PagedListGuard.Validate(pageIndex, pageSize, indexFrom);

            PageIndex = pageIndex;
            PageSize = pageSize;
            IndexFrom = indexFrom;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            Items = source.Skip((PageIndex - IndexFrom) * PageSize).Take(PageSize).ToList();
        }
    }

    /// <summary>A paged list whose items were projected from another element type.</summary>
    public class PagedList<TSource, TResult> : IPagedList<TResult>
    {
        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public int IndexFrom { get; }
        public List<TResult> Items { get; }

        public bool HasPreviousPage => PageIndex - IndexFrom > 0;
        public bool HasNextPage => PageIndex - IndexFrom + 1 < TotalPages;

        /// <summary>Optional aggregate data (totals etc.) shipped alongside the page.</summary>
        public object? Summary { get; set; }

        public PagedList(IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter,
            int pageIndex, int pageSize, int indexFrom)
        {
            PagedListGuard.Validate(pageIndex, pageSize, indexFrom);

            PageIndex = pageIndex;
            PageSize = pageSize;
            IndexFrom = indexFrom;

            var all = source as ICollection<TSource> ?? source.ToList();
            TotalCount = all.Count;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            var page = all.Skip((PageIndex - IndexFrom) * PageSize).Take(PageSize).ToArray();
            Items = [.. converter(page)];
        }

        public PagedList(IPagedList<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter)
        {
            PageIndex = source.PageIndex;
            PageSize = source.PageSize;
            IndexFrom = source.IndexFrom;
            TotalCount = source.TotalCount;
            TotalPages = source.TotalPages;
            Items = [.. converter(source.Items)];
        }
    }

    internal static class PagedListGuard
    {
        public static void Validate(int pageIndex, int pageSize, int indexFrom)
        {
            if (indexFrom > pageIndex)
                throw new ArgumentException($"indexFrom: {indexFrom} > pageIndex: {pageIndex}, must indexFrom <= pageIndex");
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        }
    }
}
