namespace BookInventory.BuildingBlocks.Domain
{
    /// <summary>
    /// Base class for aggregate roots (audited and soft-deletable).
    /// <list type="bullet">
    /// <item>The root owns its child entities and enforces every invariant through its methods; children have no
    /// repository and are not modified from outside the aggregate. Other aggregates are referenced by id, not by
    /// object.</item>
    /// <item>Only the root raises domain events (<see cref="AddDomainEvent"/> is protected); they are dispatched after
    /// the aggregate is saved.</item>
    /// <item><see cref="RowVersion"/> is an optimistic concurrency token (PostgreSQL <c>xmin</c>): two users editing the
    /// same aggregate concurrently cannot silently overwrite each other.</item>
    /// </list>
    /// Prefer <c>private set</c> on properties and expose behaviour (<c>invoice.Issue(schoolId)</c>) instead of setters.
    /// </summary>
    public abstract class AggregateRoot : AuditableDomainBase, IAggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        /// <summary>Row version. Maintained by the database; never set it.</summary>
        public uint RowVersion { get; private set; }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
