using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Vendor;

/// <summary>
/// R2/Q1: the states and union territories a vendor can be in. Publishers are mostly outside Bihar, and the tax rule of
/// BR-GST-05 will compare a vendor's state with the company's. A vendor's state is optional; when given it must be on this list.
/// </summary>
public static class VendorState
{
    public static readonly IReadOnlyList<string> All =
    [
        "Andaman and Nicobar Islands", "Andhra Pradesh", "Arunachal Pradesh", "Assam", "Bihar", "Chandigarh", "Chhattisgarh",
        "Dadra and Nagar Haveli and Daman and Diu", "Delhi", "Goa", "Gujarat", "Haryana", "Himachal Pradesh", "Jammu and Kashmir",
        "Jharkhand", "Karnataka", "Kerala", "Ladakh", "Lakshadweep", "Madhya Pradesh", "Maharashtra", "Manipur", "Meghalaya", "Mizoram",
        "Nagaland", "Odisha", "Puducherry", "Punjab", "Rajasthan", "Sikkim", "Tamil Nadu", "Telangana", "Tripura", "Uttar Pradesh",
        "Uttarakhand", "West Bengal"
    ];

    /// <summary>The stored spelling of a state, or none when none is given; anything outside the list is refused.</summary>
    public static string? RequireOrNone(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;

        return All.FirstOrDefault(s => s.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
            ?? throw new ValidationFailureException(VendorMessages.InvalidState);
    }
}
