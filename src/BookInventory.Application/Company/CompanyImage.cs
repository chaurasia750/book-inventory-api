namespace BookInventory.Application.Company;

/// <summary>R17/R18: which of the company's two images is wanted.</summary>
public enum CompanyImageKind
{
    Logo,
    Signature
}

/// <summary>R17/R18: the bytes of one company image and the content type they are served with.</summary>
public sealed record CompanyImageFile(byte[] Content, string ContentType);
