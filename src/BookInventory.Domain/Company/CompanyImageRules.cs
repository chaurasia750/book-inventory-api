using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Company;

/// <summary>
/// R17/R18: the one rule of an uploaded company image (the logo and the signature image): a PNG or JPEG up to 2 MB,
/// told apart by its own bytes, never by the file name or the declared type. Anything else is refused with the caller's
/// message code, so a <c>.txt</c> renamed to <c>.png</c> is still refused.
/// </summary>
internal static class CompanyImageRules
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>R17/R18: the stored content type of the image, or a <see cref="ValidationFailureException"/> with <paramref name="invalidCode"/> when it is not a PNG or JPEG up to the cap.</summary>
    public static string Require(byte[]? content, string invalidCode)
    {
        if (content is not { Length: > 0 } || content.Length > Company.ImageMaxBytes)
            throw new ValidationFailureException(invalidCode);
        return Detect(content) ?? throw new ValidationFailureException(invalidCode);
    }

    /// <summary>The content type of the bytes (PNG or JPEG), or null when they are neither.</summary>
    public static string? Detect(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith(PngSignature))
            return "image/png";
        if (content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
            return "image/jpeg";
        return null;
    }
}
