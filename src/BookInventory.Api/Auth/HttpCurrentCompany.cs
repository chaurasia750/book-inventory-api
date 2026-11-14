using System.Security.Claims;
using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Application.Iam.Companies;

namespace BookInventory.Api.Auth;

/// <summary>R62: the company of the current request, read from the <c>cid</c> claim of the access token and from nowhere else.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICurrentCompany))]
public sealed class HttpCurrentCompany(IHttpContextAccessor accessor) : ICurrentCompany
{
    public Guid? CompanyId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(AuthClaims.CompanyId), out var id) ? id : null;
}
