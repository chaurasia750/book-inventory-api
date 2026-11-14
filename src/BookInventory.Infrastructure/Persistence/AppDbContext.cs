using BookInventory.BuildingBlocks.Application;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Core;
using BookInventory.Application.Iam.Companies;
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
    /// <para>
    /// And it keeps company-owned data apart (spec platform P19, P20): every company-owned entity is filtered to the
    /// company of the session and a new row gets that company before it is saved.
    /// </para>
    /// </summary>
    public class AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUser currentUser,
        IDomainEventDispatcher eventDispatcher,
        IAuditActor auditActor,
        ICurrentCompany currentCompany)
        : BuildingBlocksDbContext(options, currentUser, eventDispatcher)
    {
        /// <summary>P19: the company the query filter compares with; <see cref="Guid.Empty"/> (matching no row) when the session has none.</summary>
        public Guid SessionCompanyId => currentCompany.CompanyId ?? Guid.Empty;

        protected override void ConfigureModel(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            CompanyScope.ApplyFilter(modelBuilder, this);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            PrepareSave();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            PrepareSave();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>The company is stamped first, so the audit entry of a new row already carries it (P21).</summary>
        private void PrepareSave()
        {
            ChangeTracker.DetectChanges();
            CompanyScope.StampAddedRows(ChangeTracker, currentCompany);

            var entries = AuditCapture.Capture(ChangeTracker, auditActor.Current);
            if (entries.Count > 0)
                Set<AuditEntry>().AddRange(entries);
        }
    }
}
