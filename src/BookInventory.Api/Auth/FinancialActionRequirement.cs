using Microsoft.AspNetCore.Authorization;
using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Api.Auth;

/// <summary>
/// R49: the policy the money-moving and approval endpoints of <c>finance</c> put next to their role, as
/// <c>[Authorize(Policy = AuthPolicies.FinancialAction)]</c>. It only ever refuses the hidden system account, and only
/// while <c>SystemAccount:AllowFinancialActions</c> is <c>false</c>; every other caller is judged by their role alone.
/// </summary>
public sealed class FinancialActionRequirement : IAuthorizationRequirement;

public static class AuthPolicies
{
    public const string FinancialAction = "FinancialAction";
}

public sealed class FinancialActionHandler(SystemAccountSettings systemAccount) : AuthorizationHandler<FinancialActionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FinancialActionRequirement requirement)
    {
        var isSystemAccount = context.User.HasClaim(AuthClaims.SystemAccount, AuthClaims.SystemAccountValue);
        if (!isSystemAccount || systemAccount.AllowFinancialActions)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
