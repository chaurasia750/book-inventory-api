using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Persistence.Configurations.Iam;

/// <summary><c>iam.user_password_reset</c>: the insert-only record of R67 (which account, who reset it, when); it never holds a password.</summary>
public sealed class UserPasswordResetConfiguration() : ModuleEntityConfiguration<UserPasswordReset>("iam")
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserPasswordReset> builder)
    {
        builder.HasIndex(r => new { r.UserId, r.ResetAt });

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.ResetBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
