using BookInventory.Application.Company;
using BookInventory.Application.Core;
using BookInventory.Application.Finance;
using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R13: the school snapshot (with its GSTIN) and the company state an invoice keeps, read through their capabilities.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class InvoiceParties(ISchoolDirectory schools, ICompanyTaxProfile company)
{
    public async Task<(SaleSchool School, string? Gstin, string? CompanyState)> OfAsync(SchoolOrder order, CancellationToken cancellationToken)
    {
        var school = await schools.GetAsync(order.SchoolId, cancellationToken);
        var tax = await company.CurrentAsync(cancellationToken);
        return (new SaleSchool(order.SchoolId, school?.Code ?? order.SchoolCode, school?.Name ?? order.SchoolName, school?.State ?? order.SchoolState), school?.Gstin, tax?.State);
    }
}
