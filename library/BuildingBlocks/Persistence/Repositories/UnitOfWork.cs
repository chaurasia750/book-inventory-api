using System.Collections.Concurrent;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookInventory.BuildingBlocks.Persistence.Repositories
{
    /// <summary>Scoped unit of work over a single DbContext: repositories, save and one database transaction.</summary>
    public sealed class UnitOfWork(DbContext dbContext) : IUnitOfWork, IAsyncDisposable, IDisposable
    {
        private readonly ConcurrentDictionary<Type, object> _repositories = new();
        private IDbContextTransaction? _transaction;

        public IRepository<T> Repository<T>() where T : AggregateRoot =>
            (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(dbContext));

        public async Task StartTransactionAsync(CancellationToken ct = default) =>
            _transaction ??= await dbContext.Database.BeginTransactionAsync(ct);

        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            dbContext.SaveChangesAsync(ct);

        public async Task CommitAsync(CancellationToken ct = default)
        {
            try
            {
                await dbContext.SaveChangesAsync(ct);
                if (_transaction is not null)
                    await _transaction.CommitAsync(ct);
            }
            catch
            {
                await RollbackAsync(CancellationToken.None);
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            try
            {
                if (_transaction is not null)
                    await _transaction.RollbackAsync(ct);
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public ValueTask DisposeAsync() => DisposeTransactionAsync();

        public void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;
        }

        private async ValueTask DisposeTransactionAsync()
        {
            if (_transaction is null)
                return;

            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
