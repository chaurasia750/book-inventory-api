using BookInventory.BuildingBlocks.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.BuildingBlocks.Persistence.Extensions
{
    public static class IQueryableExtensions
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 200;

        /// <summary>
        /// Executes a COUNT and one page query. <paramref name="pageIndex"/> is 1-based; <paramref name="pageSize"/> is
        /// clamped to <c>1..MaxPageSize</c> so a client cannot request the whole table.
        /// </summary>
        public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageIndex,
            int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageIndex < 1) pageIndex = 1;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            var count = await source.CountAsync(cancellationToken);
            var items = await source.Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync(cancellationToken);

            return new PagedList<T>
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                IndexFrom = 1,
                TotalCount = count,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize),
                Items = items
            };
        }
    }
}
