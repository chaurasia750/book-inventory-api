using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// <c>iam.password_reset_token</c>. Only the SHA-256 of the emailed token is stored and it is unique, so a
/// token can only ever resolve to one row.
/// </summary>
public sealed class PasswordResetTokenConfiguration() : ModuleEntityConfiguration<PasswordResetToken>("iam")
{
    protected override void ConfigureEntity(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();

        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.ExpiresAt);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
