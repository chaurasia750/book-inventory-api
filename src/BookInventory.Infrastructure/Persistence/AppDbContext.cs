using BookInventory.BuildingBlocks.Application;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Persistence
{
    /// <summary>
    /// The application's single DbContext (PostgreSQL, snake_case, soft delete, audit - all from the base class).
    /// Add a <c>DbSet&lt;T&gt;</c> per aggregate root and an <c>IEntityTypeConfiguration&lt;T&gt;</c> class per entity;
    /// configurations in this assembly are picked up automatically.
    /// </summary>
    public class AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUser currentUser,
        IDomainEventDispatcher eventDispatcher)
        : BuildingBlocksDbContext(options, currentUser, eventDispatcher)
    {
        protected override void ConfigureModel(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
