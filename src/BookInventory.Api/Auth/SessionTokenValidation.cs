using BookInventory.BuildingBlocks.Configuration.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Api.Auth;

/// <summary>
/// Adds one rule to JWT bearer validation: a token whose <c>sid</c> session is no longer usable is rejected with 401
/// (R31 groundwork). It is added here, in the composition root, and not inside the platform's
/// <c>AddJwtAuthentication</c>, because the platform knows nothing about sessions - they are this application's
/// business (the same reason no iam name appears under <c>library/</c>).
/// </summary>
public static class SessionTokenValidation
{
    public static IServiceCollection AddSessionTokenValidation(this IServiceCollection services)
    {
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure(options =>
            {
                options.Events ??= new JwtBearerEvents();
                options.Events.OnTokenValidated = ValidateSessionAsync;
            });

        return services;
    }

    /// <summary>Runs per request, so the scoped guard is taken from the request's own provider, not the root one.</summary>
    private static async Task ValidateSessionAsync(TokenValidatedContext context)
    {
        if (context.Principal?.GetSessionId() is not { } sessionId)
        {
            context.Fail("The access token carries no session.");
            return;
        }

        var guard = context.HttpContext.RequestServices.GetRequiredService<ISessionGuard>();
        if (!await guard.IsActiveAsync(sessionId, context.HttpContext.RequestAborted))
            context.Fail("The session is no longer active.");
    }
}
