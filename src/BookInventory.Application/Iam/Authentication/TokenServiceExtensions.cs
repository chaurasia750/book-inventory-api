using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

public static class TokenServiceExtensions
{
    /// <summary>
    /// The access token of a user for one session (R11), built in one place: login, refresh and change-password all hand
    /// out the same claims, so a claim added later (or the forced-change flag of R12) cannot differ between them.
    /// </summary>
    public static IssuedAccessToken IssueAccessTokenFor(this ITokenService tokenService, AppUser user, Guid sessionId, TimeSpan lifetime) =>
        tokenService.IssueAccessToken(new AccessTokenRequest(
            user.Id, user.UserName, user.Role, sessionId, user.MustChangePassword, lifetime, user.IsSystemAccount));
}
