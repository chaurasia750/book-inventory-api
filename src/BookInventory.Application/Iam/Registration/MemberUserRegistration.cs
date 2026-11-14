using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Registration;

/// <summary>
/// R53: creates the agent's login and then does what <c>Registration:Mode</c> says. With <c>ApprovalRequired</c> the
/// account stays <c>PENDING</c>, nothing is issued and the caller shows the welcome text of
/// <see cref="IamMessages.RegistrationPendingApproval"/>. With <c>InstantLogin</c> the account is active at once and the
/// session is issued by the very code a login uses, so the login-log row, the tokens and the cookie cannot differ.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IMemberUserRegistration))]
public sealed class MemberUserRegistration(
    MemberAccountCreator creator,
    LoginGrantService grant,
    RegistrationSettings settings,
    TimeProvider timeProvider) : IMemberUserRegistration
{
    public async Task<MemberRegistrationResult> RegisterAsync(MemberRegistration registration, CancellationToken cancellationToken = default)
    {
        var instantLogin = settings.Mode == RegistrationMode.InstantLogin;
        var user = await creator.CreateAsync(registration, instantLogin, cancellationToken);

        if (!instantLogin)
            return new MemberRegistrationResult(RegistrationOutcome.PendingApproval, user.Id, IamMessages.RegistrationPendingApproval, Session: null);

        var context = new LoginContext(user.UserName, registration.Client, RememberMe: false);
        var session = await grant.GrantAsync(user, context, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

        return new MemberRegistrationResult(RegistrationOutcome.SignedIn, user.Id, MessageCode: null, session.Value);
    }
}
