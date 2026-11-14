using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Persistence.Configurations.Iam;

/// <summary>
/// <c>iam.app_user</c>. Identity is stored normalised, so the three unique indexes of R1 are plain indexes
/// instead of case-insensitive comparisons. The <c>HiddenAccount</c> query filter is the single place R43 is
/// implemented: every normal query therefore hides the system account by default.
/// </summary>
public sealed class AppUserConfiguration() : ModuleEntityConfiguration<AppUser>("iam")
{
    protected override void ConfigureEntity(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.UserName).HasMaxLength(IdentityNormalizer.UsernameMaxLength).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(IdentityNormalizer.EmailMaxLength);
        builder.Property(u => u.Mobile).HasMaxLength(IdentityNormalizer.MobileMaxLength);
        builder.Property(u => u.FullName).HasMaxLength(IdentityNormalizer.FullNameMaxLength).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(u => u.StatusReason).HasMaxLength(AppUser.ReasonMaxLength);
        builder.Property(u => u.FailedLoginCount).HasDefaultValue(0);
        builder.Property(u => u.MustChangePassword).HasDefaultValue(false);
        builder.Property(u => u.IsSystemAccount).HasDefaultValue(false);

        // A partial unique index, not a filtered index on a boolean: it says "at most one", in the database.
        // HasFilter takes the condition only - EF puts the WHERE keyword in front of it.
        builder.HasIndex(u => u.IsSystemAccount)
            .IsUnique()
            .HasFilter("is_system_account")
            .HasDatabaseName("uq_app_user_single_system_account");

        // R1: username, email and mobile are each unique. The values are stored normalised, so a plain unique index
        // is enough and no case-insensitive comparison is needed anywhere.
        builder.HasIndex(u => u.UserName).IsUnique();
        // Email and mobile can be released (R58), so uniqueness has to ignore the nulls a released account leaves behind.
        builder.HasIndex(u => u.Email).IsUnique().HasFilter("email IS NOT NULL");
        builder.HasIndex(u => u.Mobile).IsUnique().HasFilter("mobile IS NOT NULL");

        // R59: a plain id (no foreign key across schemas); the company module guarantees it exists before it is stored.
        builder.HasIndex(u => u.CompanyId);

        builder.HasIndex(u => u.Role);
        builder.HasIndex(u => u.Status);
        builder.HasIndex(u => u.LastLoginAt);
        builder.HasIndex(u => u.LockedUntil);

        // Status history keeps the actor of the current status; a null actor is SYSTEM (R32), which is why there is no
        // required foreign key here and no cascade.
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(u => u.StatusChangedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // R43: hidden by default, for every query, unless the caller opts out on purpose.
        builder.HasQueryFilter(AppUser.HiddenAccountFilterKey, u => !u.IsSystemAccount);
    }
}
