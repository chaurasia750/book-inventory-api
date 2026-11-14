namespace BookInventory.BuildingBlocks.Domain
{
    /// <summary>
    /// An aggregate root: the single entry point to a cluster of entities that must stay consistent together.
    /// Outside code loads, changes and saves the cluster only through the root; a repository exists per root.
    /// </summary>
    public interface IAggregateRoot
    {
        Guid Id { get; }
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
    }
}
