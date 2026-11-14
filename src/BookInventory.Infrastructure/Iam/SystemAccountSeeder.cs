using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BookInventory.Application.Iam;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;
using BookInventory.Infrastructure.Iam.Configuration;
using BookInventory.Infrastructure.Persistence;

namespace BookInventory.Infrastructure.Iam;

/// <summary>
/// R42/R51/R52: creates the hidden system account from <c>SystemAccount:*</c> at startup and replaces its stored hash
/// when the configured secret was rotated. It is the only writer of that account: no endpoint can create, edit or
/// delete it.
/// <list type="bullet">
/// <item>Disabled (<c>Enabled = false</c>) or no password configured: nothing is created; an account that exists stays
/// but cannot log in (the login use case refuses it like an unknown user, R46).</item>
/// <item>A configured password that breaks the password policy, or the optional longer minimum of R50, stops startup
/// rather than seeding a weak always-on account. The password is never logged.</item>
/// <item>Its e-mail is a fixed placeholder on the reserved <c>.invalid</c> domain and it has no mobile, so no real
/// registration can collide with it, and a conflict answer never reveals it (R52).</item>
/// </list>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SystemAccountSeeder(
    IUnitOfWork unitOfWork,
    IOptions<SystemAccountOptions> options,
    IOptions<AuthOptions> authOptions,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider,
    ILogger<SystemAccountSeeder> logger)
{
    private const string PlaceholderEmail = "system-account@book-inventory.invalid";
    private const string FullName = "System";

    private readonly IRepository<AppUser> _users = unitOfWork.Repository<AppUser>();

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.Enabled || string.IsNullOrEmpty(settings.Password))
        {
            logger.LogInformation("The system account is not seeded (disabled, or no {Section}:Password configured).", SystemAccountOptions.SectionName);
            return;
        }

        if (!await _users.TableNoTracking.IamSchemaExistsAsync(cancellationToken))
            return;

        RequireUsablePassword(settings.Password);

        var existing = await _users.Table.IgnoreHiddenAccountFilter().FirstOrDefaultAsync(u => u.IsSystemAccount, cancellationToken);
        if (existing is null)
            await CreateAsync(settings, cancellationToken);
        else
            await RotateIfChangedAsync(existing, settings.Password, cancellationToken);
    }

    private void RequireUsablePassword(string password)
    {
        var failures = authOptions.Value.ToAuthSettings().Password.Validate(password).ToList();
        var minimum = authOptions.Value.Password.SystemAccountMinimumLength;
        if (password.Length < minimum)
            failures.Add($"it must have at least {minimum} characters ({AuthOptions.SectionName}:Password:SystemAccountMinimumLength).");

        if (failures.Count > 0)
            throw new InvalidOperationException($"'{SystemAccountOptions.SectionName}:Password' is not usable: {string.Join(" ", failures)}");
    }

    private async Task CreateAsync(SystemAccountOptions settings, CancellationToken cancellationToken)
    {
        var userName = IdentityNormalizer.NormalizeUserName(settings.UserName)!;
        if (await _users.Table.IgnoreHiddenAccountFilter().AnyAsync(u => u.UserName == userName, cancellationToken))
            throw new InvalidOperationException($"'{SystemAccountOptions.SectionName}:UserName' ('{userName}') is already used by another account.");

        var account = AppUser.CreateSystemAccount(
            new UserDraft(FullName, settings.UserName, PlaceholderEmail, Mobile: null, passwordHasher.Hash(settings.Password)),
            timeProvider.GetUtcNow().UtcDateTime);

        await SaveAsync(() => _users.AddAsync(account, cancellationToken), cancellationToken);
        logger.LogInformation("Seeded the system account '{UserName}'.", userName);
    }

    private async Task RotateIfChangedAsync(AppUser account, string password, CancellationToken cancellationToken)
    {
        if (passwordHasher.Verify(password, account.PasswordHash) == PasswordVerificationResult.Success)
            return;

        account.RefreshSystemAccountCredentials(passwordHasher.Hash(password), PlaceholderEmail, mobile: null, timeProvider.GetUtcNow().UtcDateTime);
        _users.Update(account);
        await SaveAsync(() => Task.CompletedTask, cancellationToken);
        logger.LogInformation("The system account secret changed, so its stored hash was replaced.");
    }

    private async Task SaveAsync(Func<Task> change, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.StartTransactionAsync(cancellationToken);
            await change();
            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Two instances starting together: the unique index let one insert through. Fine when the winner's account is there.
            await unitOfWork.RollbackAsync(cancellationToken);
            if (!await _users.TableNoTracking.IgnoreHiddenAccountFilter().AnyAsync(u => u.IsSystemAccount, cancellationToken))
                throw;
        }
    }
}
