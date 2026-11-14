using System.Security.Claims;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Configuration.Extensions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Core;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Core;

namespace BookInventory.Api.Auth;

/// <summary>
/// Spec core A4: the actor of a change, read from the access token of the current request. A signed-in user is recorded
/// with their id and user name; the hidden system account (claim <c>sys</c>), a background job (no request) and an
/// anonymous request (a failed login counting up) are "System" with no id.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IAuditActor))]
public sealed class HttpAuditActor(IHttpContextAccessor accessor) : IAuditActor
{
    private static readonly AuditActorInfo System = new(null, null, true);

    public AuditActorInfo Current
    {
        get
        {
            var user = accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true || user.FindFirstValue(AuthClaims.SystemAccount) == AuthClaims.SystemAccountValue)
                return System;

            var id = user.GetUserId();
            return id == Guid.Empty ? System : new AuditActorInfo(id, user.GetUserName(), false);
        }
    }
}
