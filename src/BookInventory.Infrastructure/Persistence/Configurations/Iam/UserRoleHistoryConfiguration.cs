using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// <c>iam.user_role_history</c>: the append-only record of R6, read the same way as the status history.
/// </summary>
public sealed class UserRoleHistoryConfiguration() : ModuleEntityConfiguration<UserRoleHistory>("iam")
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserRoleHistory> builder)
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
