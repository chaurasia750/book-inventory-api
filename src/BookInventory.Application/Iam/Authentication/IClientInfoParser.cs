using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R22: turns a raw user agent into the stored browser/OS/device fields. A port, so the domain never depends on a
/// parser and the parser can be replaced (or removed) without touching a use case.
/// </summary>
public interface IClientInfoParser
{
    /// <summary>Returns <see cref="DeviceType.Unknown"/> and null names for a missing or unrecognised user agent.</summary>
    ClientDevice Parse(string? userAgent);
}
