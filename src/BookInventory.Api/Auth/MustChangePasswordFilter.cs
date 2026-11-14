using System.Security.Claims;
using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Api.Auth;

/// <summary>
/// R12: while <c>must_change_password</c> is set, the <em>server</em> allows only change-password, me and the two
/// logout actions; every other endpoint answers 403 with <c>iam.auth.password_change_required</c>. The flag comes from
/// the access token, so the check costs nothing and cannot be bypassed by calling an endpoint directly - the UI guard
/// is a convenience, this is the rule.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed partial class MustChangePasswordFilter(AuthenticationProblemFactory problems) : IAsyncAuthorizationFilter
{
    /// <summary>The whole route, not a suffix: a future <c>/reports/auth/me</c> must not slip through. The version segment is free.</summary>
    [GeneratedRegex(@"^/api/v[0-9]+/auth/(me|logout|logout-all|change-password)/?$", RegexOptions.IgnoreCase)]
    private static partial Regex AllowedRoute();

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (NeedsCheck(context) && !IsAllowed(context.HttpContext.Request.Path))
            context.Result = new ObjectResult(problems.PasswordChangeRequired()) { StatusCode = StatusCodes.Status403Forbidden };

        return Task.CompletedTask;
    }

    /// <summary>An endpoint that needs no signed-in user cannot be blocked by one user's password: health, docs and
    /// login itself stay reachable, otherwise a forced change would take the API's liveness probe down.</summary>
    private static bool NeedsCheck(AuthorizationFilterContext context) =>
        context.Filters.Any(filter => filter is IAllowAnonymousFilter) is false && MustChangePassword(context.HttpContext.User);

    private static bool MustChangePassword(ClaimsPrincipal user) =>
        user.FindFirstValue(AuthClaims.MustChangePassword) is "true";

    private static bool IsAllowed(PathString path) => AllowedRoute().IsMatch(path.Value ?? string.Empty);
}
