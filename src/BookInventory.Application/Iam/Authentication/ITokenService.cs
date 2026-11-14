namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// Issues and fingerprints the two tokens of R11. The access token is a signed JWT, the refresh token is an opaque
/// random value of which only the hash is ever stored (D2), so a database leak cannot be replayed as a login.
/// </summary>
public interface ITokenService
{
    /// <summary>Signs an access token for the claims of R11: <c>sub</c>, <c>role</c>, <c>sid</c>, <c>name</c>,
    /// <c>pwd_change</c> and <c>amr = pwd</c>.</summary>
    IssuedAccessToken IssueAccessToken(AccessTokenRequest request);

    /// <summary>Creates a new opaque random token and its hash: the refresh token on login and on every rotation (R13), the password reset token (R17).</summary>
    IssuedOpaqueToken IssueOpaqueToken(TimeSpan lifetime);

    /// <summary>SHA-256 of an opaque token (refresh or reset), the form it is stored and looked up by. Constant-time comparison happens in
    /// the aggregate, not here.</summary>
    string HashOpaqueToken(string token);
}
