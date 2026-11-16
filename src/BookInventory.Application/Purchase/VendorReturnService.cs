using BookInventory.Application.Core;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Purchase;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Purchase;

/// <summary>
/// R2-R8: makes a vendor return and cancels it. The vendor must be active, the items active; the stock out and the vendor ledger
/// debit are posted in the same transaction, so the whole vendor return, its number included, fails when anything is refused; a
/// cancel reverses both.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class VendorReturnService(IVendorReturnStore returns, VendorReturnComposer composer, ICompanyCodes codes, BusinessCalendar calendar, VendorReturnPostings postings)
{
    public async Task<VendorReturnMade> MakeAsync(MakeVendorReturnRequest request, CancellationToken cancellationToken)
    {
        composer.RequireCompany();
        var today = calendar.Today();
        var returnDate = request.ReturnDate ?? today;
        var financialYear = FinancialYear.Of(returnDate);
        var number = VendorReturn.NumberOf(financialYear, await codes.NextNumberAsync(VendorReturn.NumberSeries(financialYear), cancellationToken));
        var vendorReturn = VendorReturn.Make(await composer.ComposeAsync(request, new VendorReturnStamp(number, returnDate, today), cancellationToken));
        await returns.AddAsync(vendorReturn, cancellationToken);

        var low = await postings.PostAsync(vendorReturn, cancellationToken);
        return new VendorReturnMade(vendorReturn.Id, vendorReturn.Number, vendorReturn.GrandTotalAmount, low);
    }

    public async Task CancelAsync(Guid id, CancelVendorReturnRequest request, CancellationToken cancellationToken)
    {
        composer.RequireCompany();
        var vendorReturn = await returns.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(PurchaseMessages.VendorReturnNotFound);
        var today = calendar.Today();
        vendorReturn.Cancel(request.Reason ?? string.Empty, today);
        await postings.ReverseAsync(vendorReturn, today, cancellationToken);
    }
}
