using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Companies;

/// <summary>
/// R69: stores the entered company on the caller's own session; only a <c>SUPER_ADMIN</c> session may carry one. It also
/// issues the access token that carries the new company (R62), so the caller never has to refresh to see the change.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISessionCompanySelector))]
public sealed class SessionCompanySelector(
    IAuthenticationStore store,
    ICurrentUser currentUser,
    ITokenService tokens,
    AuthSettings settings) : ISessionCompanySelector
{
    private sealed record SuperAdminSession(UserSession Session, AppUser User);

    public async Task<SessionCompanyChange> EnterAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var caller = await RequireSuperAdminSessionAsync(cancellationToken);
        var previous = caller.Session.ActiveCompanyId;

        caller.Session.EnterCompany(companyId);
        return Changed(previous, caller);
    }

    public async Task<SessionCompanyChange> LeaveAsync(CancellationToken cancellationToken = default)
    {
        var caller = await RequireSuperAdminSessionAsync(cancellationToken);
        var previous = caller.Session.ActiveCompanyId;

        caller.Session.LeaveCompany();
        return Changed(previous, caller);
    }

    private SessionCompanyChange Changed(Guid? previousCompanyId, SuperAdminSession caller) =>
        new(previousCompanyId, tokens.IssueAccessTokenFor(caller.User, caller.Session, settings.Tokens.AccessToken));

    private async Task<SuperAdminSession> RequireSuperAdminSessionAsync(CancellationToken cancellationToken)
    {
        var sessionId = currentUser.SessionId ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);
        var session = await store.FindSessionAsync(sessionId, cancellationToken)
            ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);
        var user = await store.FindUserByIdAsync(session.UserId, cancellationToken)
            ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);

        return user.Role == Role.SuperAdmin
            ? new SuperAdminSession(session, user)
            : throw new ForbiddenException(IamMessages.UserCompanyInvalid);
    }
}
