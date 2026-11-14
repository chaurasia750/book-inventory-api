namespace BookInventory.Domain.Iam;

/// <summary>How a login attempt ended (R22). Written on every attempt, successful or not (R20).</summary>
public enum LoginResult
{
    Success,

    /// <summary>The identifier resolved and the password was wrong.</summary>
    FailedBadPassword,

    /// <summary>The identifier matched no account. Nothing about the account is recorded (R8).</summary>
    FailedUnknownUser,

    /// <summary>The account is inside its lockout window (R9).</summary>
    FailedLocked,

    /// <summary>The password was right but the application is still awaiting approval (R8).</summary>
    FailedPending,

    /// <summary>The password was right but the account is deactivated (R8).</summary>
    FailedInactive,

    /// <summary>The password was right but the account is suspended (R8). The internal reason is never included (R8).</summary>
    FailedSuspended,

    /// <summary>The password was right but the application was rejected (R8).</summary>
    FailedRejected
}

/// <summary>Device class parsed from the user agent (R22). No GeoIP in v1.</summary>
public enum DeviceType
{
    Desktop,
    Mobile,
    Tablet,
    Bot,

    /// <summary>The user agent was missing or not recognised.</summary>
    Unknown
}
