using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.BuildingBlocks.Persistence.Contracts
{
    public interface IUnitOfWork
    {
        IRepository<T> Repository<T>() where T : AggregateRoot;

        /// <summary>Begins a database transaction. No-op if one is already open.</summary>
        Task StartTransactionAsync(CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);

        /// <summary>Saves pending changes, then commits the transaction (if one is open).</summary>
        Task CommitAsync(CancellationToken ct = default);

        Task RollbackAsync(CancellationToken ct = default);
    }
}
