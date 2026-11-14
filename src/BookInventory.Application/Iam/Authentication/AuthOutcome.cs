using System.Net;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// One refused authentication attempt, as a stable message code plus the status the spec asks for (R8: 401 generic,
/// 401 specific after the correct password, 423 for a lockout, section 5 "423/429 locked or rate limited").
/// </summary>
public sealed record AuthFailure(string Code, HttpStatusCode Status)
{
    public static AuthFailure Unauthenticated(string code) => new(code, HttpStatusCode.Unauthorized);

    public static AuthFailure Locked(string code) => new(code, HttpStatusCode.Locked);
}

/// <summary>
/// The result of a use case that can fail in an expected way. It returns instead of throwing on purpose: the login
/// attempt and the lockout counter must be committed even though the request answers 401/423, and a thrown exception
/// would roll the request transaction back (R21, <c>[PersistOnFailure]</c>).
/// </summary>
public sealed record AuthOutcome<T>(T? Granted, AuthFailure? Refused) where T : class
{
    public static AuthOutcome<T> GrantedWith(T value) => new(value, null);

    public static AuthOutcome<T> Denied(string code) => new(null, AuthFailure.Unauthenticated(code));

    public static AuthOutcome<T> Denied(AuthFailure failure) => new(null, failure);

    /// <summary>Fails fast when a caller uses a refused outcome as if it were a success; a bug, not a user error.</summary>
    public T Value => Granted ?? throw new InvalidOperationException("The authentication attempt was refused.");
}
