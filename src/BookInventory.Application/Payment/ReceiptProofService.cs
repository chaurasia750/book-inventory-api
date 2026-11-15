using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Payment;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Payment;

/// <summary>
/// R20: the optional proof files of a receipt. The receipt checks the file (by its own bytes), the size, the count of five and
/// that it is still recorded; this class loads, calls one behaviour and reads a proof back. The bytes of the proofs are never
/// loaded with the receipt for any other use case.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ReceiptProofService(IReceiptStore store)
{
    public async Task<ReceiptProofDto> AttachAsync(Guid receiptId, string? fileName, byte[] content, CancellationToken cancellationToken)
    {
        var receipt = await store.FindForManagementAsync(receiptId, cancellationToken) ?? throw new NotFoundException(PaymentMessages.ReceiptNotFound);
        var proof = receipt.AttachProof(fileName, content, await store.CountProofsAsync(receiptId, cancellationToken));
        return new ReceiptProofDto(proof.Id, proof.FileName, proof.ContentType, proof.SizeBytes);
    }

    public async Task<ReceiptProofFile> ReadAsync(Guid receiptId, Guid proofId, CancellationToken cancellationToken) =>
        await store.FindProofAsync(receiptId, proofId, cancellationToken) ?? throw new NotFoundException(PaymentMessages.ProofNotFound);

    public async Task RemoveAsync(Guid receiptId, Guid proofId, CancellationToken cancellationToken)
    {
        var receipt = await store.FindForProofRemovalAsync(receiptId, proofId, cancellationToken) ?? throw new NotFoundException(PaymentMessages.ReceiptNotFound);
        receipt.RemoveProof(proofId);
    }
}
