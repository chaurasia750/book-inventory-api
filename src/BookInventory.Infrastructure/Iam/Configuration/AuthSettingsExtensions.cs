using System.Net;
using BookInventory.Application.Iam;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Iam.Configuration;

/// <summary>
/// Turns the validated <c>Auth:*</c> configuration into the immutable snapshot the Application layer is allowed to
/// see. Configuration is bound and range-checked at startup (R54); this is the only place that reads it, so a use case
/// never multiplies minutes or names a cookie itself.
/// </summary>
public static class AuthSettingsExtensions
{
    public static AuthSettings ToAuthSettings(this AuthOptions options) => new(
        new LockoutPolicy(options.Lockout.MaxFailedAttempts, TimeSpan.FromMinutes(options.Lockout.Minutes)),
        new PasswordPolicy(
            options.Password.MinimumLength,
            options.Password.RequiredDigit,
            options.Password.RequiredLowerCase,
            options.Password.RequiredUpperCase),
        new TokenSettings(
            TimeSpan.FromMinutes(options.Tokens.AccessTokenMinutes),
            TimeSpan.FromHours(options.Tokens.RefreshTokenHours),
            TimeSpan.FromDays(options.Tokens.RememberMeDays),
            TimeSpan.FromHours(options.Tokens.RefreshCookieHours),
            TimeSpan.FromMinutes(options.Tokens.SystemAccountSessionMinutes),
            TimeSpan.FromDays(options.Tokens.AbsoluteSessionDays),
            options.Tokens.RefreshCookieName,
            options.Tokens.RefreshCookiePath,
            options.Tokens.AccessCookieName,
            options.Tokens.AccessCookiePath),
        new ResetSettings(
            TimeSpan.FromMinutes(options.Reset.TokenMinutes),
            options.Reset.MaxRequestsPerHour,
            options.Reset.LinkUrl));

    public static SystemAccountSettings ToSettings(this SystemAccountOptions options) => new(
        options.Enabled,
        options.AllowedIps.Select(IPAddress.Parse).Select(ip => ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip).ToList(),
        string.IsNullOrWhiteSpace(options.AlertEmail) ? null : options.AlertEmail.Trim(),
        options.AllowFinancialActions);
}
