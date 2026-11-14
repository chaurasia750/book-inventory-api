namespace BookInventory.BuildingBlocks.Domain
{
    public abstract record DomainEventBase : IDomainEvent
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
