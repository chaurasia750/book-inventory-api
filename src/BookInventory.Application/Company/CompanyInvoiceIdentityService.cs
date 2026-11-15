using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Application.Company;

/// <summary>
/// R17-R20: the Owner of a company (or a Super Admin inside it) sets its invoice identity. One guard decides "this is
/// the caller's own company" and answers the same 404 for any other id, so nobody can probe other companies (R6). Every
/// rule of the identity is the aggregate's; this class loads, calls one behaviour and lets the request transaction commit.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CompanyInvoiceIdentityService(ICompanyStore store, ICurrentCompany currentCompany)
{
    /// <summary>R19/R20: sets the one bank account and the invoice settings.</summary>
    public async Task SetAsync(Guid id, SetCompanyInvoiceIdentityRequest request, CancellationToken cancellationToken = default)
    {
        var company = await RequireOwnedAsync(id, cancellationToken);
        company.SetInvoiceIdentity(request.ToDetails());
    }

    /// <summary>R17: uploads or replaces the logo (PNG or JPEG up to 2 MB).</summary>
    public async Task SetLogoAsync(Guid id, byte[]? content, CancellationToken cancellationToken = default)
    {
        var company = await RequireOwnedAsync(id, cancellationToken);
        company.SetLogo(content);
    }

    /// <summary>R17: removes the logo.</summary>
    public async Task RemoveLogoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await RequireOwnedAsync(id, cancellationToken);
        company.RemoveLogo();
    }

    /// <summary>R18: uploads or replaces the signature (PNG or JPEG up to 2 MB).</summary>
    public async Task SetSignatureAsync(Guid id, byte[]? content, CancellationToken cancellationToken = default)
    {
        var company = await RequireOwnedAsync(id, cancellationToken);
        company.SetSignature(content);
    }

    /// <summary>R18: removes the signature.</summary>
    public async Task RemoveSignatureAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await RequireOwnedAsync(id, cancellationToken);
        company.RemoveSignature();
    }

    /// <summary>The one guard: the id must be the caller's own company (an Owner's own, or the one a Super Admin entered), else 404.</summary>
    private async Task<CompanyRoot> RequireOwnedAsync(Guid id, CancellationToken cancellationToken)
    {
        if (currentCompany.CompanyId != id)
            throw new NotFoundException(CompanyMessages.NotFound);

        return await store.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(CompanyMessages.NotFound);
    }
}
