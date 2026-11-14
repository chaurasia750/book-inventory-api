using BookInventory.BuildingBlocks.Application;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Core;
using BookInventory.Domain.Core;
using BookInventory.Infrastructure.Core;

namespace BookInventory.Infrastructure.Persistence
{
    /// <summary>
    /// The application's single DbContext (PostgreSQL, snake_case, soft delete, audit - all from the base class).
    /// Add a <c>DbSet&lt;T&gt;</c> per aggregate root and an <c>IEntityTypeConfiguration&lt;T&gt;</c> class per entity;
    /// configurations in this assembly are picked up automatically.
    /// <para>
    /// It also writes the audit trail (spec core A1, AD1): before every save the changes of the audited rows become audit
    /// entries that are saved in the same transaction, so no module has to remember to record a change.
    /// </para>
    /// </summary>
    public class AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUser currentUser,
        IDomainEventDispatcher eventDispatcher,
        IAuditActor auditActor)
        : BuildingBlocksDbContext(options, currentUser, eventDispatcher)
    {
        protected override void ConfigureModel(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            AddAuditEntries();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            AddAuditEntries();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void AddAuditEntries()
        {
            var entries = AuditCapture.Capture(ChangeTracker, auditActor.Current);
            if (entries.Count > 0)
                Set<AuditEntry>().AddRange(entries);
        }
    }
}
