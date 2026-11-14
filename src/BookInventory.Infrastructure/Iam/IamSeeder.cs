using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BookInventory.Application.Iam;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;
using BookInventory.Infrastructure.Iam.Configuration;
using BookInventory.Infrastructure.Persistence;

namespace BookInventory.Infrastructure.Iam;

/// <summary>
/// R2: a deployment needs one usable Super Admin before anybody can create one through the API, so a fresh database
/// is seeded with the Super Admin configured under <c>Seed:SuperAdmin:*</c>. It is an ordinary, <em>visible</em>
/// account with <c>must_change_password = true</c>; the hidden system account is a different thing and is seeded from
/// <c>SystemAccount:*</c> (Plan 0009).
/// <para>
/// Safety rules, because this runs on every startup of every environment:
/// <list type="bullet">
/// <item>It creates nothing when a visible active Super Admin already exists, so restarting never adds a second one.</item>
/// <item>It refuses to run with a missing, reserved or policy-violating configuration value rather than creating
/// something weak. The password itself is never logged.</item>
/// <item>It refuses to run against a database whose <c>iam</c> schema does not exist yet (migrations not applied) and
/// says so, instead of failing the whole application with a PostgreSQL error.</item>
/// </list>
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class IamSeeder(
    IUnitOfWork unitOfWork,
    IOptions<SuperAdminSeedOptions> seedOptions,
    AuthSettings settings,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider,
    ILogger<IamSeeder> logger)
{
    private readonly IRepository<AppUser> _users = unitOfWork.Repository<AppUser>();

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var usersTableExists = await _users.TableNoTracking.IamSchemaExistsAsync(cancellationToken);
        if (!usersTableExists)
        {
            logger.LogWarning(
                "iam schema not found, skipping the Super Admin seed. Apply the migrations before starting the application.");
            return;
        }

        // Checked before the configuration is validated: once an admin exists the seed values may be removed again,
        // and a missing password must not stop a healthy deployment from starting.
        if (await VisibleSuperAdminExistsAsync(cancellationToken))
        {
            logger.LogInformation("A visible active Super Admin already exists, nothing to seed.");
            return;
        }

        var seed = seedOptions.Value;
        var failures = Validate(seed, settings.Password);
        if (failures.Count > 0)
        {
            throw new InvalidOperationException(
                $"'{SuperAdminSeedOptions.SectionName}' is not usable: {string.Join(" ", failures)}. " +
                "Set it in user-secrets or an environment variable; the password must never be committed.");
        }

        var normalizedUserName = IdentityNormalizer.NormalizeUserName(seed.UserName)!;

        // The hidden system account is the only account allowed to own a reserved name, so it must not collide with the
        // seed; this lookup is one of the two places allowed to look past the HiddenAccount filter.
        if (await _users.Table.IgnoreHiddenAccountFilter().AnyAsync(u => u.UserName == normalizedUserName, cancellationToken))
        {
            logger.LogWarning(
                "No visible active Super Admin exists but the user name '{UserName}' is already taken, so nothing was seeded.",
                normalizedUserName);
            return;
        }

        var user = AppUser.CreateStaff(
            new UserDraft(seed.FullName, seed.UserName, seed.Email, Mobile: null, passwordHasher.Hash(seed.Password)),
            Role.SuperAdmin,
            actorId: Guid.Empty,
            timeProvider.GetUtcNow().UtcDateTime);

        try
        {
            await unitOfWork.StartTransactionAsync(cancellationToken);
            await _users.AddAsync(user, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two instances starting together both saw an empty table; the unique index on user_name let only one
            // insert through. Losing that race is fine when the winner's admin is there, anything else is a real error.
            await unitOfWork.RollbackAsync(cancellationToken);
            if (!await VisibleSuperAdminExistsAsync(cancellationToken))
                throw;

            logger.LogInformation("Another instance seeded the Super Admin first, nothing more to do.");
            return;
        }

        logger.LogInformation(
            "Seeded the Super Admin '{UserName}'. It must change its password on first login.",
            user.UserName);
    }

    private Task<bool> VisibleSuperAdminExistsAsync(CancellationToken cancellationToken) =>
        _users.TableNoTracking.AnyAsync(AppUser.VisibleActiveSuperAdmin, cancellationToken);

    /// <summary>Validates the configuration without touching the database, so a bad value fails fast and clearly.</summary>
    private static List<string> Validate(SuperAdminSeedOptions seed, PasswordPolicy policy)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(seed.UserName)
            || string.IsNullOrWhiteSpace(seed.Email)
            || string.IsNullOrWhiteSpace(seed.FullName)
            || string.IsNullOrWhiteSpace(seed.Password))
        {
            failures.Add("UserName, Email, FullName and Password are all required.");
            return failures;
        }

        if (ReservedUserNames.IsReserved(IdentityNormalizer.NormalizeUserName(seed.UserName)))
            failures.Add($"UserName '{seed.UserName}' is reserved for the system account.");

        failures.AddRange(policy.Validate(seed.Password));
        return failures;
    }
}
