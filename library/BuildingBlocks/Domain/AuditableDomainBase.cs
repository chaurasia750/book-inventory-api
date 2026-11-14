namespace BookInventory.BuildingBlocks.Domain
{
    /// <summary>
    /// Insert-only entities (ledger rows, logs): who created the row and when (UTC).
    /// The values are stamped by the persistence layer; do not set them from business code.
    /// </summary>
    public abstract class TransactionDomainBase : DomainBase
    {
        public DateTime CreatedAt { get; private set; }
        public Guid CreatedBy { get; private set; }

        /// <summary>For the persistence layer only.</summary>
        public void StampCreated(DateTime utcNow, Guid userId)
        {
            if (CreatedAt == default) CreatedAt = utcNow;
            if (CreatedBy == Guid.Empty) CreatedBy = userId;
        }
    }

    /// <summary>Mutable entities: adds who last changed the row and when (UTC).</summary>
    public abstract class AuditableDomainBase : TransactionDomainBase
    {
        public DateTime? UpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        /// <summary>For the persistence layer only. <paramref name="userId"/> is skipped when empty or when the caller set it.</summary>
        public void StampUpdated(DateTime utcNow, Guid userId, bool keepUpdatedBy)
        {
            UpdatedAt = utcNow;
            if (userId != Guid.Empty && !keepUpdatedBy) UpdatedBy = userId;
        }
    }
}
