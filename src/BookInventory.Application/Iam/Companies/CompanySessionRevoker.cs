using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Companies;

/// <summary>R63: ends every usable session of a company's accounts, each through <see cref="SessionEndService"/> so its login-log row closes with the same reason.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICompanySessionRevoker))]
public sealed class CompanySessionRevoker(
    IAuthenticationStore store,
    SessionEndService sessionEnd,
    TimeProvider timeProvider) : ICompanySessionRevoker
{
    public async Task RevokeAllAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var session in await store.FindActiveSessionsOfCompanyAsync(companyId, cancellationToken))
            await sessionEnd.EndAsync(session, LogoutReason.CompanyDeactivated, now, cancellationToken);
    }
}
