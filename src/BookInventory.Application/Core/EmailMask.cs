namespace BookInventory.Application.Core;

/// <summary>Spec N10: an address in an application log is masked (<c>a***@example.com</c>), never printed whole.</summary>
public static class EmailMask
{
    public static string Mask(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return string.Empty;

        var at = address.IndexOf('@');
        return at > 0 ? $"{address[0]}***{address[at..]}" : "***";
    }
}
