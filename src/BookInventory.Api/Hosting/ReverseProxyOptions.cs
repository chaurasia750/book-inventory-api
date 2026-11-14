using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using HttpOverrides = Microsoft.AspNetCore.HttpOverrides;

namespace BookInventory.Api.Hosting;

/// <summary>
/// R23: the addresses whose forwarded headers may be believed. Empty by default, which means no header is believed
/// and every log row carries the socket address - a spoofed <c>X-Forwarded-For</c> from anywhere else is ignored.
/// A deployment behind IIS/nginx or a load balancer lists its address here.
/// </summary>
public sealed class ReverseProxyOptions : IValidatableObject
{
    public const string SectionName = "ReverseProxy";

    /// <summary>Exact proxy addresses, e.g. <c>"127.0.0.1"</c>.</summary>
    public string[] KnownProxies { get; set; } = [];

    /// <summary>Proxy networks in CIDR form, e.g. <c>"10.0.0.0/8"</c>.</summary>
    public string[] KnownNetworks { get; set; } = [];

    /// <summary>How many proxies may be chained. One is enough for a single reverse proxy and stops a client from
    /// appending a fake address to the header.</summary>
    [Range(1, 5)]
    public int ForwardLimit { get; set; } = 1;

    /// <summary>Every entry has to parse, otherwise a typo would silently leave the API without forwarded headers.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var proxy in KnownProxies)
        {
            if (!IPAddress.TryParse(proxy, out _))
                yield return new ValidationResult(
                    $"ReverseProxy:KnownProxies contains '{proxy}', which is not an IP address.",
                    [nameof(KnownProxies)]);
        }

        foreach (var network in KnownNetworks)
        {
            if (ParseNetwork(network) is null)
                yield return new ValidationResult(
                    $"ReverseProxy:KnownNetworks contains '{network}', which is not a network in CIDR form.",
                    [nameof(KnownNetworks)]);
        }
    }

    /// <summary>Turns one CIDR entry into a network, or <c>null</c> when it is not one. Used by both the validation
    /// and the pipeline, so a value that passed startup can never fail later.</summary>
    public static IPNetwork? ParseNetwork(string? value)
    {
        var parts = value?.Split('/', 2);
        if (parts is null || parts.Length != 2 || !IPAddress.TryParse(parts[0], out var address) || !int.TryParse(parts[1], out var prefix))
            return null;

        var maxPrefix = address.AddressFamily == AddressFamily.InterNetworkV6 ? 128 : 32;

        return prefix < 0 || prefix > maxPrefix ? null : new IPNetwork(address, prefix);
    }
}

/// <summary>Applies R23 to the pipeline. It has to run first, so every later layer - logs, rate limiter, CORS - and
/// the login-log row itself see the real client address.</summary>
public static class ForwardedHeadersSetup
{
    public static IApplicationBuilder UseTrustedForwardedHeaders(this IApplicationBuilder app, ReverseProxyOptions options)
    {
        // The framework middleware trusts EVERY caller when both lists are empty, so with no proxy configured it must
        // not run at all: every header is then ignored and the row gets the socket address (R23).
        if (options.KnownProxies.Length == 0 && options.KnownNetworks.Length == 0)
            return app;

        var forwarded = new ForwardedHeadersOptions
        {
            ForwardedHeaders = HttpOverrides.ForwardedHeaders.XForwardedFor | HttpOverrides.ForwardedHeaders.XForwardedProto,
            ForwardLimit = options.ForwardLimit
        };

        // Loopback is trusted by default; only what is configured here may be believed (R23).
        forwarded.KnownProxies.Clear();
        forwarded.KnownIPNetworks.Clear();

        foreach (var proxy in options.KnownProxies)
            forwarded.KnownProxies.Add(IPAddress.Parse(proxy));

        foreach (var network in options.KnownNetworks)
        {
            if (ReverseProxyOptions.ParseNetwork(network) is { } parsed)
                forwarded.KnownIPNetworks.Add(parsed);
        }

        return app.UseForwardedHeaders(forwarded);
    }
}
