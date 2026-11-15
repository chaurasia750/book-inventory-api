using BookInventory.Application.Payment;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Payment;

/// <summary>Spec payment R10, R11: the result of a cheque of a receipt, for the Owner and a Super Admin who entered the company.</summary>
[Route("receipts/{receiptId:guid}/cheques/{modeId:guid}")]
public sealed class ChequesController(ChequeResultService cheques) : ApiControllerBase
{
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R10: the cheque cleared; posts bank against cheque in hand. <c>409 payment.cheque_result_set</c> when it has a result.</summary>
    [HttpPost("clear"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Clear(Guid receiptId, Guid modeId, [FromBody] ClearChequeRequest request, CancellationToken cancellationToken)
    {
        await cheques.ClearAsync(receiptId, modeId, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R10, R11: the cheque bounced; the school owes it again and the invoices it paid reopen.</summary>
    [HttpPost("bounce"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Bounce(Guid receiptId, Guid modeId, [FromBody] BounceChequeRequest request, CancellationToken cancellationToken)
    {
        await cheques.BounceAsync(receiptId, modeId, request, cancellationToken);
        return NoContent();
    }
}
