using Microsoft.AspNetCore.Authentication.JwtBearer;
using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Api.Auth;

/// <summary>
/// D2: a browser is signed in by the access-token cookie (see <see cref="SessionCookieWriter"/>). The JWT bearer handler
/// reads its token from the <c>Authorization</c> header; this adds the cookie as the source when the header is absent, so
/// tools that send <c>Authorization: Bearer</c> keep working unchanged and the header always wins. Everything after the read
/// (signature, lifetime, the session check of <see cref="SessionTokenValidation"/>) is the same for both.
/// </summary>
public static class AccessCookieAuthentication
{
    public static IServiceCollection AddAccessCookieAuthentication(this IServiceCollection services)
    {
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure(options =>
            {
                options.Events ??= new JwtBearerEvents();
                options.Events.OnMessageReceived = ReadTokenFromCookie;
            });

        return services;
    }

    private static Task ReadTokenFromCookie(MessageReceivedContext context)
    {
        if (!string.IsNullOrEmpty(context.Request.Headers.Authorization))
            return Task.CompletedTask;

        var cookieName = context.HttpContext.RequestServices.GetRequiredService<AuthSettings>().Tokens.AccessCookieName;
        if (context.Request.Cookies.TryGetValue(cookieName, out var token) && !string.IsNullOrEmpty(token))
            context.Token = token;

        return Task.CompletedTask;
    }
}
