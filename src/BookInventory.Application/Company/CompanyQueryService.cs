using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Company;
using BookInventory.Domain.Core;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Application.Company;

/// <summary>
/// R9-R11: the company list and detail for the Super Admin, and the company of the caller's own session for everyone
/// else. The detail reads the company's accounts through the <c>iam</c> user directory, never from the <c>iam</c> tables.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class CompanyQueryService(ICompanyStore store, IUserDirectory users, ICurrentCompany currentCompany)
{
    public Task<IPagedList<CompanySummary>> SearchAsync(CompanySearchQuery query, CancellationToken cancellationToken) =>
        store.SearchAsync(query, cancellationToken);

    public async Task<CompanyDetailResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var company = await RequireAsync(id, cancellationToken);
        var accounts = await users.ListCompanyAccountsAsync(id, cancellationToken);
        return ToDetail(company, accounts);
    }

    /// <summary>R6/R10: the company of this request's session (none for a Super Admin who has not entered one); the id is never read from the request.</summary>
    public async Task<CurrentCompanyView> GetCurrentAsync(CancellationToken cancellationToken)
    {
        if (currentCompany.CompanyId is not { } id)
        {
            return new CurrentCompanyView(null);
        }

        var company = await RequireAsync(id, cancellationToken);
        return new CurrentCompanyView(new CurrentCompanyResponse(company.Id, company.Name, company.Slug, company.Status, company.State));
    }

    /// <summary>R21: the master data of the session's company; <c>403</c> when the caller has no company context.</summary>
    public async Task<CompanyProfileResponse> GetProfileAsync(CancellationToken cancellationToken)
    {
        var c = await RequireCurrentAsync(cancellationToken);
        return new CompanyProfileResponse(c.Name, c.State, c.Address, c.Mobile, c.Email, c.Gstin, c.Pan, c.Tagline);
    }

    /// <summary>R20: the invoice identity of the session's company; <c>403</c> when the caller has no company context.</summary>
    public async Task<CompanyInvoiceIdentityResponse> GetInvoiceIdentityAsync(CancellationToken cancellationToken)
    {
        var company = await RequireCurrentAsync(cancellationToken);
        return new CompanyInvoiceIdentityResponse(
            company.Logo is null ? null : ImageUrl(company.Id, CompanyImageKind.Logo),
            company.Signature is null ? null : ImageUrl(company.Id, CompanyImageKind.Signature),
            new CompanyBankDetails(
                company.BankAccountHolder, company.BankName, company.BankAccountNumber, company.BankIfsc,
                company.BankBranch, company.UpiId),
            new CompanyInvoiceSettings(company.InvoiceTerms, company.InvoiceFooter, company.PrintGstin));
    }

    /// <summary>
    /// R17/R18: the bytes of the session company's logo or signature. <c>403</c> without a company context, <c>404</c>
    /// for another company's id (R6) or a company that has no such image.
    /// </summary>
    public async Task<CompanyImageFile> GetImageAsync(Guid id, CompanyImageKind kind, CancellationToken cancellationToken)
    {
        if (currentCompany.CompanyId is not { } ownId)
            throw new ForbiddenException(CoreMessages.CompanyRequired);
        if (ownId != id)
            throw new NotFoundException(CompanyMessages.NotFound);

        return await store.FindImageAsync(id, kind, cancellationToken) ?? throw new NotFoundException(CompanyMessages.ImageNotFound);
    }

    private async Task<CompanyRoot> RequireCurrentAsync(CancellationToken cancellationToken)
    {
        if (currentCompany.CompanyId is not { } id)
            throw new ForbiddenException(CoreMessages.CompanyRequired);

        return await RequireAsync(id, cancellationToken);
    }

    private async Task<CompanyRoot> RequireAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindForReadAsync(id, cancellationToken) ?? throw new NotFoundException(CompanyMessages.NotFound);

    /// <summary>The path the image is served from; the same route the controller exposes (R17/R18).</summary>
    private static string ImageUrl(Guid id, CompanyImageKind kind) =>
        $"/api/v1/companies/{id}/{(kind == CompanyImageKind.Logo ? "logo" : "signature")}";

    private static CompanyDetailResponse ToDetail(CompanyRoot company, IReadOnlyList<UserSummary> accounts) =>
        new(
            company.Id, company.Name, company.Address, company.Mobile, company.Email, company.Gstin, company.Pan,
            company.State, company.Tagline, company.Status, company.StatusReason, company.StatusChangedAt, company.CreatedAt,
            [.. company.StatusHistory
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new CompanyStatusEntry(h.FromStatus, h.ToStatus, h.Reason, h.ChangedBy, h.ChangedAt))],
            accounts);
}
