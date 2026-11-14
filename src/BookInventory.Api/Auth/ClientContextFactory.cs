using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Api.Auth;

/// <summary>
/// Builds the client part of a login attempt (R22). The IP is read from the connection, which is already the real
/// caller address because the forwarded headers were resolved against the trusted proxies before this point (R23) -
/// a forged <c>X-Forwarded-For</c> from an untrusted address is ignored there and never reaches this code.
/// </summary>
public static class ClientContextFactory
{
    public static ClientContext Create(HttpContext context) => new(
        context.Connection.RemoteIpAddress?.ToString(),
        Header(context, "User-Agent"),
        Header(context, "Accept-Language"),
        context.TraceIdentifier);

    private static string? Header(HttpContext context, string name)
    {
        var value = context.Request.Headers[name].ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
