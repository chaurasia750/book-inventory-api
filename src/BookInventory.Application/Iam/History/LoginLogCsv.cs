using System.Globalization;
using System.Text;

namespace BookInventory.Application.Iam.History;

/// <summary>
/// The CSV of a login-log export (R26), with the fields of R22. Every value is written through <see cref="Escape"/>,
/// which quotes it for CSV and neutralises a leading <c>=</c>, <c>+</c>, <c>-</c>, <c>@</c>, tab or carriage return with an
/// apostrophe, so a hostile user agent or a typed identifier such as <c>=cmd|...</c> is text in a spreadsheet, never a formula.
/// </summary>
public static class LoginLogCsv
{
    private static readonly string[] Columns =
    [
        "login_at", "user", "identifier", "result", "ip_address", "browser", "browser_version", "os", "os_version",
        "device_type", "logged_out_at", "logout_reason", "new_device", "new_ip", "user_agent"
    ];

    public static string Build(IEnumerable<LoginLogItem> rows)
    {
        var csv = new StringBuilder();
        csv.AppendJoin(',', Columns).Append("\r\n");

        foreach (var row in rows)
        {
            var cells = new[]
            {
                Time(row.LoginAt),
                row.Actor.IsSystem ? "System" : row.Actor.UserName,
                row.IdentifierAttempted,
                Wire(row.Result),
                row.IpAddress,
                row.BrowserName,
                row.BrowserVersion,
                row.OsName,
                row.OsVersion,
                Wire(row.DeviceType),
                row.LoggedOutAt is { } loggedOut ? Time(loggedOut) : null,
                row.LogoutReason is { } reason ? Wire(reason) : null,
                row.IsNewDevice ? "yes" : "no",
                row.IsNewIp ? "yes" : "no",
                row.UserAgent
            };

            csv.AppendJoin(',', cells.Select(Escape)).Append("\r\n");
        }

        return csv.ToString();
    }

    /// <summary>Quotes a value for CSV and defuses a spreadsheet formula.</summary>
    public static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
            value = "'" + value;

        return value.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    /// <summary>The wire spelling of an enum (<c>FAILED_BAD_PASSWORD</c>), the same one the JSON API and the database use.</summary>
    private static string Wire<T>(T value) where T : struct, Enum =>
        System.Text.Json.JsonNamingPolicy.SnakeCaseUpper.ConvertName(value.ToString()) ?? value.ToString();

    private static string Time(DateTime value) => value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
}
