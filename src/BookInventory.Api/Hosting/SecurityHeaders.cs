using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BookInventory.Api.Hosting;

/// <summary>
/// Response headers that harden the browser side of a same-origin deployment: no MIME sniffing, no framing, a strict
/// referrer, and a Content-Security-Policy on every HTML page. The policy is written for the real Angular build and every
/// relaxation is listed here:
/// <list type="bullet">
/// <item><c>style-src 'unsafe-inline'</c>: Angular inlines the critical CSS of each page and components set inline styles.</item>
/// <item><c>script-src 'unsafe-hashes' 'sha256-...'</c>: only the one inline handler Angular's critical-CSS loader emits
/// (<c>onload="this.media='all'"</c>); no other inline script is allowed.</item>
/// <item>Google Fonts and cdnjs (Font Awesome) are allowed as stylesheet and font sources because the three apps load them.</item>
/// <item><c>img-src https:</c> and <c>data:</c>/<c>blob:</c>: property images and uploaded pictures.</item>
/// </list>
/// A JSON answer gets no CSP; it is never rendered.
/// </summary>
public static class SecurityHeaders
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-hashes' 'sha256-MhtPZXr7+LpJUY5qtMutB+qWfQtMaPccfe7QXtCcEYc='; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
        "font-src 'self' data: https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
        "img-src 'self' data: blob: https:; " +
        "connect-src 'self'; " +
        "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";

    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.XContentTypeOptions = "nosniff";
                headers.XFrameOptions = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

                if (context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true)
                    headers.ContentSecurityPolicy = ContentSecurityPolicy;

                return Task.CompletedTask;
            });

            await next();
        });

        return app;
    }
}
