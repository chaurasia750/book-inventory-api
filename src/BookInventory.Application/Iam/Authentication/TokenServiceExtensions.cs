using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

public static class TokenServiceExtensions
{
    /// <summary>
    /// The access token of a user for one session (R11), built in one place: login, refresh and change-password all hand
    /// out the same claims, so a claim added later (or the forced-change flag of R12) cannot differ between them.
    /// The company (R62) is the one a Super Admin entered in this session (R69), otherwise the account's own (R59).
    /// </summary>
    public static IssuedAccessToken IssueAccessTokenFor(this ITokenService tokenService, AppUser user, UserSession session, TimeSpan lifetime) =>
        tokenService.IssueAccessToken(new AccessTokenRequest(
            user.Id, user.UserName, user.Role, session.Id, user.MustChangePassword, lifetime, user.IsSystemAccount,
            session.ActiveCompanyId ?? user.CompanyId));
}
