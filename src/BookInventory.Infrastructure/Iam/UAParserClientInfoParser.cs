using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;
using UAParser;
using ParsedAgent = UAParser.ClientInfo;

namespace BookInventory.Infrastructure.Iam;

/// <summary>
/// R22: parses the raw user agent into the browser name/version, OS name/version and device type that go on every
/// login-log row, using the <c>UAParser</c> package the spec approved. An empty or unrecognised agent yields
/// <see cref="DeviceType.Unknown"/> and null names - the row is still written, because the attempt happened.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IClientInfoParser))]
public sealed class UAParserClientInfoParser : IClientInfoParser
{
    private static readonly Parser Parser = Parser.GetDefault();

    private static readonly string[] MobileOperatingSystems = ["Android", "iOS", "Windows Phone", "BlackBerry OS", "KaiOS"];

    public ClientDevice Parse(string? userAgent) =>
        string.IsNullOrWhiteSpace(userAgent)
            ? Unknown()
            : Describe(Parser.Parse(userAgent), userAgent);

    private static ClientDevice Describe(ParsedAgent info, string userAgent) =>
        new(
            NameOrNull(info.UA?.Family),
            info.UA?.Major is null ? null : $"{info.UA.Major}.{info.UA.Minor}.{info.UA.Patch}".TrimEnd('.'),
            NameOrNull(info.OS?.Family),
            info.OS?.Major is null ? null : $"{info.OS.Major}.{info.OS.Minor}.{info.OS.Patch}".TrimEnd('.'),
            DeviceOf(info, userAgent));

    /// <summary>
    /// UAParser names devices by model ("iPhone", "Samsung SM-S918B", "Generic Smartphone"), so the family alone cannot
    /// tell a phone from a laptop. The operating system and the tokens browsers put in the string decide instead:
    /// a crawler is a bot, a handheld OS is a phone unless the string says tablet (Android without "Mobile" is a
    /// tablet by the vendors' own convention), a recognised browser on anything else is a desktop, and an agent
    /// nobody recognises stays unknown.
    /// </summary>
    private static DeviceType DeviceOf(ParsedAgent info, string userAgent)
    {
        if (info.Device.IsSpider)
            return DeviceType.Bot;

        var family = info.Device.Family ?? string.Empty;
        var isTablet = family.Contains("iPad", StringComparison.OrdinalIgnoreCase)
                       || family.Contains("Tablet", StringComparison.OrdinalIgnoreCase)
                       || family is "Kindle" or "PlayBook"
                       || userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase);
        var isHandheldOs = MobileOperatingSystems.Contains(info.OS?.Family);

        if (isTablet || (isHandheldOs && info.OS?.Family == "Android" && !userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase)))
            return DeviceType.Tablet;

        if (isHandheldOs || userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
            return DeviceType.Mobile;

        return NameOrNull(info.UA?.Family) is null && NameOrNull(info.OS?.Family) is null
            ? DeviceType.Unknown
            : DeviceType.Desktop;
    }

    private static string? NameOrNull(string? family) => family is null or "Other" ? null : family;

    private static ClientDevice Unknown() => new(null, null, null, null, DeviceType.Unknown);
}
