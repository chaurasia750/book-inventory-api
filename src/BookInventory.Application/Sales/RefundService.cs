using BookInventory.Application.Core;
using BookInventory.Application.Finance;
using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Sales;

/// <summary>
/// R64-R67: records a refund and cancels it. The school must exist and be owed at least the amount (R65, read through the finance
/// capability); one ledger transaction is posted in the same transaction (R66), and a cancel posts its exact opposite. Everything fails together.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class RefundService(
    IRefundStore refunds, ICreditNoteStore notes, ISchoolDirectory schools, ILedgerBalances balances, RefundPostings postings, RefundStamps stamps)
{
    public async Task<RefundMade> RecordAsync(RecordRefundRequest request, CancellationToken cancellationToken)
    {
        stamps.RequireCompany();
        var school = await schools.GetAsync(request.SchoolId!.Value, cancellationToken) ?? throw new ValidationFailureException(ReturnMessages.SchoolNotFound);
        var note = await CreditNoteOfAsync(request, cancellationToken);
        await RequireCreditAsync(school.Id, request.Amount!.Value, cancellationToken);

        var (number, date, today) = await stamps.NewAsync(request.RefundDate, cancellationToken);
        var refund = SchoolRefund.Record(new RefundInput
        {
            Number = number, RefundDate = date, Today = today, School = new SaleSchool(school.Id, school.Code, school.Name, school.State, school.Address),
            Amount = request.Amount.Value, Mode = request.Mode!.Value, Reference = request.Reference, CreditNoteId = note?.Id, CreditNoteNumber = note?.Number, Remark = request.Remark
        });
        await refunds.AddAsync(refund, cancellationToken);
        await postings.PostAsync(refund, cancellationToken);
        return new RefundMade(refund.Id, refund.Number, refund.Amount);
    }

    public async Task CancelAsync(Guid id, CancelRefundRequest request, CancellationToken cancellationToken)
    {
        stamps.RequireCompany();
        var refund = await refunds.FindForManagementAsync(id, cancellationToken) ?? throw new NotFoundException(ReturnMessages.RefundNotFound);
        var today = stamps.Today();
        refund.Cancel(request.Reason ?? string.Empty, today);
        await postings.ReverseAsync(refund, today, cancellationToken);
    }

    /// <summary>R64: the credit note a refund settles must be a live one of the same school; it limits nothing.</summary>
    private async Task<CreditNote?> CreditNoteOfAsync(RecordRefundRequest request, CancellationToken cancellationToken)
    {
        if (request.CreditNoteId is null)
            return null;

        var note = await notes.FindForReadAsync(request.CreditNoteId.Value, cancellationToken) ?? throw new NotFoundException(ReturnMessages.CreditNoteNotFound);
        return note.SchoolId != request.SchoolId || note.Status == CreditNoteStatus.Cancelled ? throw new ValidationFailureException(ReturnMessages.RefundCreditNoteOfOtherSchool) : note;
    }

    /// <summary>R65 (Q42): at most what the company owes the school, that is, its credit balance below zero.</summary>
    private async Task RequireCreditAsync(Guid schoolId, decimal amount, CancellationToken cancellationToken)
    {
        var balance = await balances.BalanceOfAsync(new LedgerParty(PartyType.School, schoolId), cancellationToken);
        if (amount > -balance)
            throw new ValidationFailureException(ReturnMessages.RefundExceedsCredit);
    }
}
