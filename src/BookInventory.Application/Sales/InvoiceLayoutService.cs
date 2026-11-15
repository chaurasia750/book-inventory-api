using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>R26: reads and changes the layout a school's invoices open in; the first change creates the school's row.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class InvoiceLayoutService(IInvoiceLayoutStore store, ISchoolDirectory schools, PriceCompanyGuard companyGuard)
{
    public async Task<InvoiceLayoutResponse> GetAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        await RequireSchoolAsync(schoolId, cancellationToken);
        var row = await store.FindForReadAsync(schoolId, cancellationToken);
        return new InvoiceLayoutResponse(schoolId, row?.Layout ?? SchoolInvoiceLayout.Default);
    }

    public async Task<InvoiceLayoutResponse> ChangeAsync(Guid schoolId, ChangeInvoiceLayoutRequest request, CancellationToken cancellationToken)
    {
        companyGuard.Require();
        await RequireSchoolAsync(schoolId, cancellationToken);
        var layout = request.Layout!.Value;
        var row = await store.FindForManagementAsync(schoolId, cancellationToken);
        if (row is null)
            await store.AddAsync(SchoolInvoiceLayout.For(schoolId, layout), cancellationToken);
        else
            row.Choose(layout);

        return new InvoiceLayoutResponse(schoolId, layout);
    }

    private async Task RequireSchoolAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        if (await schools.GetAsync(schoolId, cancellationToken) is null)
            throw new NotFoundException(SalesMessages.SchoolNotFound);
    }
}
