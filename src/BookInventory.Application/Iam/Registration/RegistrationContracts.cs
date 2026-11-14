using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Application.Iam.Registration;

/// <summary>R53: what happens after an agent registers; the value of <c>Registration:Mode</c>.</summary>
public enum RegistrationMode
{
    ApprovalRequired,
    InstantLogin
}

/// <summary>The one value of <see cref="RegistrationMode"/> the Application layer sees.</summary>
public sealed record RegistrationSettings(RegistrationMode Mode);

/// <summary>
/// What the registering module hands over when an agent registers (R53): the user id it generated (R1), the values that must be
/// unique, the password the agent chose and the client the request came from (for the login-log row of an instant sign-in).
/// </summary>
public sealed record MemberRegistration(
    string UserId,
    string FullName,
    string Email,
    string Mobile,
    string Password,
    ClientContext Client);

public enum RegistrationOutcome
{
    /// <summary>The account waits as <c>PENDING</c>; no session was issued.</summary>
    PendingApproval,

    /// <summary>The account is <c>ACTIVE</c> and a session was issued.</summary>
    SignedIn
}

/// <summary>
/// The result the registering module receives. <see cref="Session"/> is only set for <see cref="RegistrationOutcome.SignedIn"/>
/// and carries the refresh cookie the caller must write; <see cref="ToResponse"/> is what may go into the response body.
/// </summary>
public sealed record MemberRegistrationResult(
    RegistrationOutcome Outcome,
    Guid UserId,
    string? MessageCode,
    IssuedSession<LoginResponse>? Session)
{
    public MemberRegistrationResponse ToResponse() => new(Outcome, UserId, MessageCode, Session?.Response);
}

/// <summary>The body of a registration answer: the outcome, the message code for the welcome text (R55) and the sign-in for an instant login.</summary>
public sealed record MemberRegistrationResponse(
    RegistrationOutcome Outcome,
    Guid UserId,
    string? MessageCode,
    LoginResponse? SignIn);
