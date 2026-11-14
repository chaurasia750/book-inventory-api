using System.Linq.Expressions;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Models;

namespace BookInventory.BuildingBlocks.Persistence.Contracts
{
    /// <summary>
    /// Generic repository. Mutating methods only change the EF change tracker; nothing is written until
    /// <see cref="IUnitOfWork.SaveChangesAsync"/> (or the request transaction commit) runs. Reads honour the
    /// soft delete filter; use <c>Table.IgnoreQueryFilters()</c> to see deleted rows.
    /// </summary>
    public interface IRepository<T> where T : AggregateRoot
    {
        /// <summary>Change-tracked query. Use for entities you are about to modify.</summary>
        IQueryable<T> Table { get; }

        /// <summary>Read-only query (<c>AsNoTracking</c>).</summary>
        IQueryable<T> TableNoTracking { get; }

        IQueryable<T> IncludeMultiple(params Expression<Func<T, object>>[] includes);

        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<TResult?> GetFirstOrDefaultAsync<TResult>(
            Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken ct = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        Task<TResult[]> FetchAsync<TResult>(
            Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken ct = default);
        Task<TResult[]> FetchAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken ct = default);

        /// <param name="orderBy">"Property", "Property DESC" or comma separated list; validated against <typeparamref name="T"/>.</param>
        /// <param name="pageIndex">1-based.</param>
        Task<IPagedList<TResult>> GetPagedListAsync<TResult>(
            Expression<Func<T, bool>>? predicate,
            Expression<Func<T, TResult>> selector,
            string? orderBy = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default) where TResult : class;

        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>Marks the entity deleted. The DbContext turns this into a soft delete (<c>is_deleted = true</c>).</summary>
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>Set-based soft delete (single UPDATE). Bypasses the change tracker, audit and domain events.</summary>
        Task<int> SoftDeleteWhereAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        /// <summary>Parameterised raw SQL, e.g. <c>ExecuteSqlAsync($"select sp_close_day({date})")</c>.</summary>
        Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken ct = default);
    }
}
