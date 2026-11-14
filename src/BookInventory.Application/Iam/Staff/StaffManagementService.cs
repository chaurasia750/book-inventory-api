using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Passwords;

namespace BookInventory.Application.Iam.Staff;

/// <summary>
/// R66/R67: what an Owner does to their own Staff. Each call first asks <see cref="OwnStaffGuard"/>, then hands the work
/// to the use case the Super Admin already uses (R28-R31, R34, R36, R67), so no rule is written twice: the reason
/// requirement of R30, the session revocation of R31 and the history of R33 are those use cases'.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class StaffManagementService(
    OwnStaffGuard guard,
    UserLifecycleService lifecycle,
    UserAccessService access,
    TemporaryPasswordResetService passwordResets)
{
    public async Task DeactivateAsync(Guid id, StatusChangeRequest request, CancellationToken cancellationToken)
    {
        await guard.RequireAsync(id, cancellationToken);
        await lifecycle.DeactivateAsync(id, request, cancellationToken);
    }

    public async Task ActivateAsync(Guid id, StatusChangeRequest request, CancellationToken cancellationToken)
    {
        await guard.RequireAsync(id, cancellationToken);
        await lifecycle.ActivateAsync(id, request, cancellationToken);
    }

    public async Task UnlockAsync(Guid id, CancellationToken cancellationToken)
    {
        await guard.RequireAsync(id, cancellationToken);
        await access.UnlockAsync(id, cancellationToken);
    }

    public async Task ForceLogoutAsync(Guid id, CancellationToken cancellationToken)
    {
        await guard.RequireAsync(id, cancellationToken);
        await access.ForceLogoutAsync(id, cancellationToken);
    }

    public async Task<TemporaryPasswordResponse> ResetPasswordAsync(Guid id, CancellationToken cancellationToken)
    {
        await guard.RequireAsync(id, cancellationToken);
        return await passwordResets.ResetAsync(id, cancellationToken);
    }
}
