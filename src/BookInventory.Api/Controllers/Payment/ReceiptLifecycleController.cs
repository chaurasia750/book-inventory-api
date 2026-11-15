using BookInventory.Application.Payment;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Payment;

/// <summary>Spec payment R13, R14: adjusting a receipt's advance and cancelling a receipt, for the Owner and a Super Admin who entered the company.</summary>
[Route("receipts/{id:guid}")]
public sealed class ReceiptLifecycleController(ReceiptLifecycleService lifecycle) : ApiControllerBase
{
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R13: applies the advance to open invoices (oldest first, or the given allocations); <c>409 payment.no_advance</c> when there is none.</summary>
    [HttpPost("adjust-advance"), Authorize(Roles = Owners)]
    public async Task<IActionResult> AdjustAdvance(Guid id, [FromBody] AdjustAdvanceRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.AdjustAdvanceAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R14: cancels a recorded receipt with a reason; <c>409 payment.receipt_has_cheque_result</c> when a cheque has a result.</summary>
    [HttpPost("cancel"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReceiptRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.CancelAsync(id, request, cancellationToken);
        return NoContent();
    }
}
