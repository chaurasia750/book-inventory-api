using System.Linq.Expressions;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.BuildingBlocks.Persistence.Repositories
{
    public class Repository<T>(DbContext dbContext) : IRepository<T> where T : AggregateRoot
    {
        protected DbContext DbContext { get; } = dbContext;
        protected DbSet<T> DbSet => DbContext.Set<T>();

        public IQueryable<T> Table => DbSet;
        public IQueryable<T> TableNoTracking => DbSet.AsNoTracking();

        public IQueryable<T> IncludeMultiple(params Expression<Func<T, object>>[] includes) =>
            includes.Aggregate(Table, (query, include) => query.Include(include));

        // FindAsync would skip the soft delete filter, so query instead.
        public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Table.FirstOrDefaultAsync(e => e.Id == id, ct);

        public Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
            Table.SingleOrDefaultAsync(predicate, ct);

        public Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
            Table.FirstOrDefaultAsync(predicate, ct);

        public Task<TResult?> GetFirstOrDefaultAsync<TResult>(
            Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken ct = default) =>
            TableNoTracking.Where(predicate).Select(selector).FirstOrDefaultAsync(ct);

        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
            TableNoTracking.AnyAsync(predicate, ct);

        public Task<TResult[]> FetchAsync<TResult>(
            Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken ct = default) =>
            TableNoTracking.Where(predicate).Select(selector).ToArrayAsync(ct);

        public Task<TResult[]> FetchAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken ct = default) =>
            TableNoTracking.Select(selector).ToArrayAsync(ct);

        public Task<IPagedList<TResult>> GetPagedListAsync<TResult>(
            Expression<Func<T, bool>>? predicate,
            Expression<Func<T, TResult>> selector,
            string? orderBy = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken ct = default) where TResult : class
        {
            var query = TableNoTracking;
            if (predicate is not null)
                query = query.Where(predicate);

            // Order on the entity (so any column can be sorted on), then project.
            return query.OrderBy(orderBy).Select(selector).ToPagedListAsync(pageIndex, pageSize, ct);
        }

        public async Task AddAsync(T entity, CancellationToken ct = default) =>
            await DbSet.AddAsync(entity, ct);

        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default) =>
            DbSet.AddRangeAsync(entities, ct);

        public void Update(T entity)
        {
            // Tracked entities already know what changed; only detached ones need attaching.
            if (DbContext.Entry(entity).State == EntityState.Detached)
                DbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
                Update(entity);
        }

        public void Remove(T entity) => DbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => DbSet.RemoveRange(entities);

        public Task<int> SoftDeleteWhereAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
            Table.Where(predicate).ExecuteUpdateAsync(s => s.SetProperty(e => e.IsDeleted, true), ct);

        public Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken ct = default) =>
            DbContext.Database.ExecuteSqlAsync(sql, ct);
    }
}
