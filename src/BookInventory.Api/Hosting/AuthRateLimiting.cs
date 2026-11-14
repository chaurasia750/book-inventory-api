using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using BookInventory.Api.Auth;
using BookInventory.Infrastructure.Iam.Configuration;

namespace BookInventory.Api.Hosting;

/// <summary>
/// R10: login and forgot-password are limited per client IP with the framework's own rate limiter - ten and five
/// attempts a minute by default, configurable in <c>Auth:RateLimit:*</c> (R54); each endpoint has its own bucket. A caller over the limit gets 429 with
/// <c>Retry-After</c> and the <c>iam.auth.rate_limited</c> code, so the UI can say the same thing it says for any
/// other refused login.
/// </summary>
public static class AuthRateLimiting
{
    public const string PolicyName = "login";
    public const string ForgotPasswordPolicyName = "forgot-password";

    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            AddPolicy(options, PolicyName, limits => limits.LoginPermitPerMinute);
            AddPolicy(options, ForgotPasswordPolicyName, limits => limits.ForgotPasswordPermitPerMinute);

            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = RetryAfterSeconds(context).ToString(System.Globalization.CultureInfo.InvariantCulture);
                var problem = context.HttpContext.RequestServices.GetRequiredService<AuthenticationProblemFactory>().RateLimited();
                context.HttpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(problem);
            };
        });

        return services;
    }

    /// <summary>One fixed-window bucket per client address for the named policy, with the permit count read per request from the validated options.</summary>
    private static void AddPolicy(RateLimiterOptions options, string name, Func<RateLimitOptions, int> permits) =>
        options.AddPolicy(name, context => RateLimitPartition.GetFixedWindowLimiter(
            Partition(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = PermitLimit(context, permits),
                Window = Window,
                QueueLimit = 0
            }));

    /// <summary>R10: how long the caller has to wait, taken from the limiter itself; a full window is the safe answer if it gave none.</summary>
    private static int RetryAfterSeconds(OnRejectedContext context) =>
        context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
            ? Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
            : (int)Window.TotalSeconds;

    /// <summary>One bucket per client address; an address that cannot be read gets its own, so it cannot block others.</summary>
    private static string Partition(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    /// <summary>Read per request from the validated options, so the setting has exactly one home (R54).</summary>
    private static int PermitLimit(HttpContext context, Func<RateLimitOptions, int> permits) =>
        permits(context.RequestServices.GetRequiredService<IOptions<AuthOptions>>().Value.RateLimit);
}
