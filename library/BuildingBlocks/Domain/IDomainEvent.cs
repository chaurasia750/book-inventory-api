namespace BookInventory.BuildingBlocks.Domain
{
    public interface IDomainEvent
    {
        Guid Id { get; }

        /// <summary>UTC time at which the event happened.</summary>
        DateTime OccurredOn { get; }
    }
}
