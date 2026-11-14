using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// <c>iam.user_login_log</c> (ERD §1). The indexes are the query patterns of R25/§4: a user's own logins newest first,
/// the security search by IP and by result, and a time-ordered scan. The table is kept partition-ready by
/// <c>login_at</c> (R25) - no partition exists yet, adding one later needs no rewrite.
/// <para>
/// <c>user_id</c> is nullable (R22: a failed attempt with an unknown identifier has no user) and <c>session_id</c> is
/// nullable (null for a failed login), so both foreign keys are optional. Nothing cascades: a log row outlives every
/// account and is never deleted (R25, R37).
/// </para>
/// </summary>
public sealed class UserLoginLogConfiguration() : ModuleEntityConfiguration<UserLoginLog>("iam")
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserLoginLog> builder)
    {
        builder.Property(l => l.IdentifierAttempted).HasMaxLength(256).IsRequired();
        builder.Property(l => l.UserAgent).HasMaxLength(512);
        builder.Property(l => l.AcceptLanguage).HasMaxLength(128);
        builder.Property(l => l.BrowserName).HasMaxLength(64);
        builder.Property(l => l.BrowserVersion).HasMaxLength(32);
        builder.Property(l => l.OsName).HasMaxLength(64);
        builder.Property(l => l.OsVersion).HasMaxLength(32);
        builder.Property(l => l.TraceId).HasMaxLength(64);

        builder.HasIndex(l => new { l.UserId, l.LoginAt });
        builder.HasIndex(l => new { l.IpAddress, l.LoginAt });
        builder.HasIndex(l => new { l.Result, l.LoginAt });
        builder.HasIndex(l => new { l.SessionId, l.LoginAt });
        // R26: the unfiltered "newest first" page and a date-range-only search of the Super Admin have no user, IP or result to lead with.
        builder.HasIndex(l => l.LoginAt);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserSession>()
            .WithMany()
            .HasForeignKey(l => l.SessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
