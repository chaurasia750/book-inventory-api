using System.ComponentModel.DataAnnotations;

namespace BookInventory.Infrastructure.Iam.Configuration;

/// <summary>
/// Every tunable security value of the <c>iam</c> module (R54), bound from <c>Auth:*</c> and validated at startup, so a
/// token lifetime of 0 or a lockout of 0 attempts stops the application instead of misbehaving later (AC29). The values
/// are only consumed from Plan 0004 (authentication) on; they live here so the domain never reads configuration.
/// </summary>
public sealed class AuthOptions : IValidatableObject
{
    public const string SectionName = "Auth";

    [Required]
    public PasswordOptions Password { get; set; } = new();

    [Required]
    public LockoutOptions Lockout { get; set; } = new();

    [Required]
    public TokenOptions Tokens { get; set; } = new();

    [Required]
    public RateLimitOptions RateLimit { get; set; } = new();

    [Required]
    public ResetOptions Reset { get; set; } = new();

    /// <summary>
    /// Validates the sections. <c>ValidateDataAnnotations</c> only looks at the properties of
    /// <see cref="AuthOptions"/> itself and does not descend into a nested object, so without this every
    /// <c>[Range]</c> below (a lockout of 0 attempts, a token life of 0 minutes) would be accepted and only
    /// misbehave later. This is what makes an out-of-range value stop startup (R54, AC29).
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var section in new object?[] { Password, Lockout, Tokens, RateLimit, Reset })
        {
            if (section is null)
                continue;

            var results = new List<ValidationResult>();
            Validator.TryValidateObject(section, new ValidationContext(section), results, validateAllProperties: true);
            foreach (var result in results)
                yield return result;
        }

        if (Tokens is not null && Tokens.AbsoluteSessionDays < Tokens.RememberMeDays)
            yield return new ValidationResult("Auth:Tokens:AbsoluteSessionDays must not be shorter than Auth:Tokens:RememberMeDays.", [nameof(Tokens)]);
    }
}

/// <summary>R16: at least 8 characters with an upper case letter, a lower case letter and a digit.</summary>
public sealed class PasswordOptions
{
    public const int MinLength = 8;
    public const int DefaultIterations = 210_000;
    public const int MinimumIterations = 1_000;

    [Range(MinLength, 256)]
    public int MinimumLength { get; set; } = MinLength;

    [Range(1, 8)]
    public int RequiredUpperCase { get; set; } = 1;

    [Range(1, 8)]
    public int RequiredLowerCase { get; set; } = 1;

    [Range(1, 8)]
    public int RequiredDigit { get; set; } = 1;

    /// <summary>R11/R50: when set, the system account also needs a password of at least this length. Off by default (D15).</summary>
    [Range(0, 256)]
    public int SystemAccountMinimumLength { get; set; }

    /// <summary>
    /// R16: the PBKDF2 work factor for <em>new</em> hashes (default 210,000, the OWASP value for HMAC-SHA512). Old hashes keep
    /// verifying with the cost stored inside them and are upgraded at the next login. The lower bound exists for tests and
    /// scripted checks that log in hundreds of times; a value below the default is logged as a warning at startup.
    /// </summary>
    [Range(MinimumIterations, 2_000_000)]
    public int Iterations { get; set; } = DefaultIterations;
}

/// <summary>R9: 5 consecutive wrong passwords lock the account for 15 minutes; a success resets the counter.</summary>
public sealed class LockoutOptions
{
    /// <summary>R9/R10. Ten requests per minute per client IP; further attempts get 429 with <c>Retry-After</c>.</summary>
    [Range(1, 1000)]
    public int MaxFailedAttempts { get; set; } = 5;

    [Range(1, 1440)]
    public int Minutes { get; set; } = 15;
}

/// <summary>R11: access token lifetime, refresh token lifetime, remember-me lifetime and the cookie settings (D3).</summary>
public sealed class TokenOptions
{
    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 15;

    [Range(1, 43200)]
    public int RefreshTokenHours { get; set; } = 12;

    [Range(1, 43200)]
    public int RememberMeDays { get; set; } = 30;

    /// <summary>
    /// R13: the absolute life of a session, counted from the login, however active the user is. Refreshing slides the
    /// expiry but never past this (owner decision, Plan 0014). Must not be shorter than <see cref="RememberMeDays"/>.
    /// </summary>
    [Range(1, 365)]
    public int AbsoluteSessionDays { get; set; } = 30;

    /// <summary>Cookie name of the rotating refresh token. D3: <c>SameSite=Strict</c>, HttpOnly, Secure.</summary>
    [Required(AllowEmptyStrings = false)]
    public string RefreshCookieName { get; set; } = "book_inventory_refresh";

    /// <summary>Cookie path, scoped to the auth endpoints only (R11).</summary>
    [Required(AllowEmptyStrings = false)]
    public string RefreshCookiePath { get; set; } = "/api/v1/auth";

    /// <summary>D2: the HttpOnly cookie that carries the access token for every API call of a browser.</summary>
    [Required(AllowEmptyStrings = false)]
    public string AccessCookieName { get; set; } = "book_inventory_access";

    /// <summary>The access cookie is sent to the whole API, not only the auth path.</summary>
    [Required(AllowEmptyStrings = false)]
    public string AccessCookiePath { get; set; } = "/api";

    [Range(0, 24)]
    public int RefreshCookieHours { get; set; } = 12;

    /// <summary>R50: the system account's sessions are capped at 60 minutes and cannot be remembered. Mandatory, not optional.</summary>
    [Range(1, 60)]
    public int SystemAccountSessionMinutes { get; set; } = 60;
}

/// <summary>
/// R10: login (and, from Plan 0005, forgot-password) is limited per client IP. Ten attempts a minute from one address
/// are enough for a person and slow enough down a scripted attack; a blocked caller gets 429 with <c>Retry-After</c>.
/// </summary>
public sealed class RateLimitOptions
{
    [Range(1, 1000)]
    public int LoginPermitPerMinute { get; set; } = 10;

    /// <summary>R10: forgot-password is limited per client IP too, more tightly than login because it sends mail.</summary>
    [Range(1, 1000)]
    public int ForgotPasswordPermitPerMinute { get; set; } = 5;
}

/// <summary>
/// R17: the password reset link. The token lives <see cref="TokenMinutes"/> (30), one user may ask for at most
/// <see cref="MaxRequestsPerHour"/> links an hour, and <see cref="LinkUrl"/> is the UI page the mailed link opens
/// (<c>?token=...</c> is appended). The URL has no default on purpose: a link to the wrong host would be a silent
/// failure in production, so a missing value stops startup (R54).
/// </summary>
public sealed class ResetOptions
{
    [Range(1, 1440)]
    public int TokenMinutes { get; set; } = 30;

    [Range(1, 100)]
    public int MaxRequestsPerHour { get; set; } = 5;

    [Required(AllowEmptyStrings = false), Url]
    public string LinkUrl { get; set; } = string.Empty;
}
