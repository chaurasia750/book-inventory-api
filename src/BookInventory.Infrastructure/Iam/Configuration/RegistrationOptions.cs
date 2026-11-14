using BookInventory.Application.Iam.Registration;

namespace BookInventory.Infrastructure.Iam.Configuration;

/// <summary>
/// <c>Registration:Mode</c> (R53): what happens after an agent registers. <c>ApprovalRequired</c> (default) leaves the
/// account pending and issues no session; <c>InstantLogin</c> activates it and signs the agent in. An unknown value stops
/// startup (R54); flipping it needs a restart, no code change (AC25).
/// </summary>
public sealed class RegistrationOptions
{
    public const string SectionName = "Registration";

    public RegistrationMode Mode { get; set; } = RegistrationMode.ApprovalRequired;
}
