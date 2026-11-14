using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.BuildingBlocks.Application
{
    public interface IExternalEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
    }
}
