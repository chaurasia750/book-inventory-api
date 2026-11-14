namespace BookInventory.BuildingBlocks.Domain
{
    /// <summary>
    /// Base of every persisted entity (roots and the child entities inside an aggregate).
    /// The key is a UUID v7 generated here, in the application, so an entity has its identity before it is saved
    /// and ids sort by creation time. <see cref="IsDeleted"/> drives soft delete (see the Persistence layer).
    /// </summary>
    public abstract class DomainBase
    {
        protected DomainBase() => Id = Guid.CreateVersion7();

        public Guid Id { get; protected set; }

        public bool IsDeleted { get; private set; }

        /// <summary>Marks the entity as deleted. Called by the persistence layer when an entity is removed.</summary>
        public void MarkDeleted() => IsDeleted = true;
    }
}
