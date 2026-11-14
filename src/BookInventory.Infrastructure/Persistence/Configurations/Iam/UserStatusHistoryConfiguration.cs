using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// <c>iam.user_status_history</c>: the append-only record of R33. Indexed the way R39 reads it (all changes
/// of one user, newest first). A null <c>changed_by</c> is <c>SYSTEM</c> (R32), so the actor is a nullable foreign key.
/// </summary>
public sealed class UserStatusHistoryConfiguration() : ModuleEntityConfiguration<UserStatusHistory>("iam")
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserStatusHistory> builder)
    {
        builder.Property(h => h.Reason).HasMaxLength(AppUser.ReasonMaxLength).IsRequired();

        builder.HasIndex(h => new { h.UserId, h.ChangedAt });

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(h => h.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
