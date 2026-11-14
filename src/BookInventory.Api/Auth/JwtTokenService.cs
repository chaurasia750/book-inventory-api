using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Configuration.Extensions;
using BookInventory.BuildingBlocks.Configuration.Models;
using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Api.Auth;

/// <summary>
/// Signs the access token and mints the refresh token (R11). The signing key, issuer, audience and clock skew come
/// from the platform's <c>JwtSetting</c>, which is validated at startup, so a weak or missing key never reaches here.
/// <list type="bullet">
/// <item>The access token carries exactly the claims of R11: <c>sub</c>, <c>role</c>, <c>sid</c>, <c>name</c>,
/// <c>pwd_change</c> and <c>amr = pwd</c>, plus <c>sys = 1</c> on the hidden system account's token (R44). The role is the UPPER_SNAKE name, so
/// <c>[Authorize(Roles = "SUPER_ADMIN")]</c> matches.</item>
/// <item>The refresh token is 256 bits of randomness in base64url. Only its SHA-256 is stored (D2), so the database
/// alone cannot be replayed as a login, and the comparison happens in the aggregate in constant time.</item>
/// </list>
/// <para>
/// It lives in the host because signing needs the ASP.NET Core token stack, which the platform already brings in
/// here for JWT validation; the application layer only knows the <see cref="ITokenService"/> port.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ITokenService))]
public sealed class JwtTokenService : ITokenService
{
    private readonly JwtSetting _settings;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _credentials;

    public JwtTokenService(JwtSetting settings, TimeProvider timeProvider)
    {
        _settings = settings;
        _timeProvider = timeProvider;
        _credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.IssuerSigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public IssuedAccessToken IssueAccessToken(AccessTokenRequest request)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.Add(request.Lifetime);
        var token = new JwtSecurityToken(
            issuer: _settings.ValidIssuer,
            audience: _settings.ValidAudience,
            claims: Claims(request),
            notBefore: now,
            expires: expiresAt,
            signingCredentials: _credentials);

        return new IssuedAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public IssuedOpaqueToken IssueOpaqueToken(TimeSpan lifetime)
    {
        var token = Base64Url(RandomNumberGenerator.GetBytes(32));

        return new IssuedOpaqueToken(token, HashOpaqueToken(token), _timeProvider.GetUtcNow().UtcDateTime.Add(lifetime));
    }

    public string HashOpaqueToken(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static IEnumerable<Claim> Claims(AccessTokenRequest request)
    {
        yield return new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString());
        yield return new Claim(AppClaimTypes.Role, SnakeCase.ConvertUpper(request.Role.ToString()));
        yield return new Claim(AppClaimTypes.SessionId, request.SessionId.ToString());
        yield return new Claim(AppClaimTypes.Name, request.UserName);
        yield return new Claim(AuthClaims.MustChangePassword, request.MustChangePassword ? "true" : "false");
        yield return new Claim(AuthClaims.AuthenticationMethod, AuthClaims.Password);

        if (request.IsSystemAccount)
            yield return new Claim(AuthClaims.SystemAccount, AuthClaims.SystemAccountValue);
    }

    /// <summary>Base64url without padding: a token that survives a cookie, a header and a query string unchanged.</summary>
    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
