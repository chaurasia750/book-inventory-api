using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Companies;

/// <summary>R69: stores the entered company on the caller's own session; only a <c>SUPER_ADMIN</c> session may carry one.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISessionCompanySelector))]
public sealed class SessionCompanySelector(IAuthenticationStore store, ICurrentUser currentUser) : ISessionCompanySelector
{
    public async Task EnterAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var session = await RequireSuperAdminSessionAsync(cancellationToken);
        session.EnterCompany(companyId);
    }

    public async Task LeaveAsync(CancellationToken cancellationToken = default)
    {
        var session = await RequireSuperAdminSessionAsync(cancellationToken);
        session.LeaveCompany();
    }

    private async Task<UserSession> RequireSuperAdminSessionAsync(CancellationToken cancellationToken)
    {
        var sessionId = currentUser.SessionId ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);
        var session = await store.FindSessionAsync(sessionId, cancellationToken)
            ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);
        var user = await store.FindUserByIdAsync(session.UserId, cancellationToken)
            ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);

        return user.Role == Role.SuperAdmin ? session : throw new ForbiddenException(IamMessages.UserCompanyInvalid);
    }
}
