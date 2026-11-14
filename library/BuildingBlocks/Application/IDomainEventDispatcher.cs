using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.BuildingBlocks.Application
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);

        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
    }
}
