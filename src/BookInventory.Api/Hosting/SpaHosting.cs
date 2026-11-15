using System.Text;
using System.Text.RegularExpressions;
using CompanyRoot = BookInventory.Domain.Company.Company;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace BookInventory.Api.Hosting;

/// <summary>
/// P13/UAC3: the API serves the three Angular apps from its own <c>wwwroot</c> so the browser only ever talks to one
/// origin (the refresh cookie is <c>SameSite=Strict</c>): the public site at <c>/</c>, the admin app at <c>/admin</c> and
/// the member app at <c>/member</c>. Nothing is served when there is no <c>wwwroot</c> (development, where the UI runs on
/// its own dev server and proxies <c>/api</c> here), so the API behaves the same without the UI.
/// <list type="bullet">
/// <item>A deep link that is not a file answers the <c>index.html</c> of the app it belongs to, so a reload works.</item>
/// <item>An unknown <c>/api</c> path stays a 404; it never turns into an HTML page.</item>
/// <item>A path with a file extension that does not exist is a 404, not <c>index.html</c> (the fallback ignores paths that look like files).</item>
/// <item>Cache: <c>index.html</c> and every unhashed file revalidate (<c>no-cache</c>), files whose name carries the build
/// hash are immutable for a year.</item>
/// </list>
/// </summary>
public static partial class SpaHosting
{
    private static readonly string[] AppFolders = ["admin", "member"];

    /// <summary>The company address the request came in on, kept while the path is rewritten to <c>/member</c> (U12).</summary>
    private const string CompanySlugItem = "company-slug";

    /// <summary>Angular names hashed files <c>main-A1B2C3D4.js</c>; such a name never changes its content.</summary>
    [GeneratedRegex(@"-[A-Z0-9]{8}\.[a-z0-9]+$")]
    private static partial Regex HashedFileName();

    /// <summary>A <c>&lt;base&gt;</c> already in the page (the admin and public apps build one in).</summary>
    [GeneratedRegex(@"<base\b[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex BaseTag();

    /// <summary>The head of the page, where the base address must come before every link and meta tag.</summary>
    [GeneratedRegex(@"<head[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex HeadTag();

    public static bool HasWebRoot(this WebApplication app) =>
        app.Environment.WebRootPath is { Length: > 0 } root && File.Exists(Path.Combine(root, "index.html"));

    /// <summary>
    /// U12: the member app lives at <c>/&lt;company slug&gt;/...</c>. A path whose first segment is not one of the application's own
    /// (reserved) segments and is not a file name is the member app of that company: it is served from <c>/member</c> of the web
    /// root, the app reads the slug from the address itself. Must run before the static files and the fallback.
    /// </summary>
    public static WebApplication UseCompanyAddresses(this WebApplication app)
    {
        if (!app.HasWebRoot())
            return app;

        app.Use((context, next) =>
        {
            if (CompanyAddress(context.Request.Path) is { } address)
            {
                context.Items[CompanySlugItem] = address.Slug;
                context.Request.Path = "/member" + address.Remainder;
            }
            return next(context);
        });
        return app;
    }

    /// <summary>The company slug and the part of the address after it, or <c>null</c> when the path is not a company address.</summary>
    private static (string Slug, string Remainder)? CompanyAddress(PathString path)
    {
        var trimmed = (path.Value ?? string.Empty).TrimStart('/');
        var slash = trimmed.IndexOf('/');
        var slug = slash < 0 ? trimmed : trimmed[..slash];
        var isOwnSegment = slug.Length == 0 || slug.Contains('.') || CompanyRoot.ReservedSlugs.Contains(slug)
            || string.Equals(slug, "member", StringComparison.OrdinalIgnoreCase);
        return isOwnSegment ? null : (slug, slash < 0 ? string.Empty : trimmed[slash..]);
    }

    public static WebApplication UseSpaStaticFiles(this WebApplication app)
    {
        if (app.HasWebRoot())
            app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = context => SetCacheHeader(context.File.Name, context.Context.Response) });

        return app;
    }

    public static WebApplication MapSpaFallback(this WebApplication app)
    {
        if (!app.HasWebRoot())
            return app;

        app.MapFallback(context => AnswerDeepLinkAsync(app, context));
        return app;
    }

    private static async Task AnswerDeepLinkAsync(WebApplication app, HttpContext context)
    {
        var index = IndexFileFor(context.Request.Path);
        var isPageRequest = HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);
        var file = index is null ? null : app.Environment.WebRootFileProvider.GetFileInfo(index);
        var slug = context.Items[CompanySlugItem] as string;

        if (!isPageRequest || file is not { Exists: true })
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-cache";

        // U12: a company page carries its base address in the markup, so the browser resolves the page's preload hints
        // against the company's address; base-href.js runs too late for those. A HEAD request has no body to rewrite.
        if (slug is null || !HttpMethods.IsGet(context.Request.Method))
        {
            await context.Response.SendFileAsync(file);
            return;
        }

        var html = await ReadIndexAsync(file);
        await context.Response.WriteAsync(AddBaseHref(html, $"/{slug}/"));
    }

    private static async Task<string> ReadIndexAsync(IFileInfo file)
    {
        await using var stream = file.CreateReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    /// <summary>The page's base address, written into the head (before every link and meta tag) or in place of the one the app built in.</summary>
    private static string AddBaseHref(string html, string baseHref)
    {
        var tag = $"<base href=\"{baseHref}\">";
        return BaseTag().IsMatch(html)
            ? BaseTag().Replace(html, tag, 1)
            : HeadTag().Replace(html, match => match.Value + tag, 1);
    }

    /// <summary>The <c>index.html</c> a deep link belongs to, or <c>null</c> when the path is not for a page (an API path).</summary>
    private static string? IndexFileFor(PathString path)
    {
        if (path.StartsWithSegments("/api"))
            return null;

        foreach (var folder in AppFolders)
        {
            if (path.StartsWithSegments("/" + folder))
                return $"{folder}/index.html";
        }

        return "index.html";
    }

    private static void SetCacheHeader(string fileName, HttpResponse response) =>
        response.Headers[HeaderNames.CacheControl] = HashedFileName().IsMatch(fileName)
            ? "public,max-age=31536000,immutable"
            : "no-cache";
}
