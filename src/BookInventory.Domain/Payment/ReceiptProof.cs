using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Payment;

/// <summary>
/// Spec payment R20: one proof of payment attached to a <see cref="Receipt"/> (a cheque photo, a UPI screenshot, a bank slip): a PNG,
/// JPEG or PDF of at most 5 MB, told apart by its own bytes (never by the file name or the declared type), kept in the database like
/// the company's logo. A company-owned child (platform P18), added and removed by its receipt; a removal is a soft delete.
/// </summary>
public class ReceiptProof : CompanyOwnedChild
{
    public const int MaxBytes = 5 * 1024 * 1024;
    public const int FileNameMaxLength = 200;

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] PdfSignature = [0x25, 0x50, 0x44, 0x46, 0x2D];

    private ReceiptProof()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        Content = [];
    }

    public Guid ReceiptId { get; private set; }

    public string FileName { get; private set; }

    public string ContentType { get; private set; }

    public byte[] Content { get; private set; }

    public int SizeBytes { get; private set; }

    internal static ReceiptProof Of(Guid receiptId, string? fileName, byte[]? content)
    {
        if (content is not { Length: > 0 } || content.Length > MaxBytes)
            throw new ValidationFailureException(PaymentMessages.ProofInvalid);

        var contentType = Detect(content) ?? throw new ValidationFailureException(PaymentMessages.ProofInvalid);
        return new ReceiptProof { ReceiptId = receiptId, FileName = CleanName(fileName), ContentType = contentType, Content = content, SizeBytes = content.Length };
    }

    /// <summary>The content type of the bytes (PNG, JPEG or PDF), or null when they are none of these.</summary>
    public static string? Detect(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith(PngSignature))
            return "image/png";
        if (content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
            return "image/jpeg";
        return content.StartsWith(PdfSignature) ? "application/pdf" : null;
    }

    private static string CleanName(string? fileName)
    {
        var name = Path.GetFileName(fileName?.Trim() ?? string.Empty);
        if (name.Length == 0)
            return "proof";

        return name.Length > FileNameMaxLength ? name[..FileNameMaxLength] : name;
    }
}
