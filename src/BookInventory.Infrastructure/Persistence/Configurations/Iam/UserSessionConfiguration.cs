using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// <c>iam.user_session</c> (ERD §1). The refresh token hash is unique, which is what makes "find the session by its
/// token" a single-row lookup; the session duration lives in <c>expires_at</c> and is never duplicated (R24).
/// </summary>
public sealed class UserSessionConfiguration() : ModuleEntityConfiguration<UserSession>("iam")
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserSession> builder)
    {
        builder.Property(s => s.RefreshTokenHash).HasMaxLength(128).IsRequired();
        builder.Property(s => s.PreviousRefreshTokenHash).HasMaxLength(128);
        builder.Property(s => s.IpAddress).HasMaxLength(64);
        builder.Property(s => s.UserAgent).HasMaxLength(512);

        builder.HasIndex(s => s.RefreshTokenHash).IsUnique();
        builder.HasIndex(s => s.PreviousRefreshTokenHash);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.ExpiresAt);
        builder.HasIndex(s => s.RevokedAt);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
