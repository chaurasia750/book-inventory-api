using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Company;

/// <summary>
/// One business using the system (spec company R1-R14). The aggregate root that owns the master data (R3), the
/// <c>ACTIVE</c>/<c>INACTIVE</c> status with its reason (R7) and the append-only status timeline (R8); every rule of those
/// is enforced here, and nothing outside changes its properties.
/// <list type="bullet">
/// <item><b>Name (R3).</b> Required, 2-150 characters, unique ignoring case and surrounding or repeated spaces: the
/// normalised form is what the unique index holds.</item>
/// <item><b>Owner user id (R4, D4).</b> The login of the company's Owner is generated from the name by
/// <see cref="OwnerUserId"/>; <c>iam</c> only checks that it is free.</item>
/// <item><b>Status (R7).</b> Deactivating needs a reason of 5-500 characters; each change adds one timeline row. A
/// company is never deleted.</item>
/// </list>
/// </summary>
public class Company : AggregateRoot, IAuditCompanyScope
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 150;
    public const int ReasonMinLength = 5;
    public const int ReasonMaxLength = 500;

    private readonly List<CompanyStatusHistory> _statusHistory = [];

    private Company()
    {
        Name = string.Empty;
        NameNormalized = string.Empty;
    }

    public string Name { get; private set; }

    /// <summary>The name in the form the unique index holds (trimmed, single spaces, lower case).</summary>
    public string NameNormalized { get; private set; }

    public string? Address { get; private set; }

    public string? Mobile { get; private set; }

    public string? Email { get; private set; }

    public string? Gstin { get; private set; }

    public string? Pan { get; private set; }

    public string? State { get; private set; }

    public string? Tagline { get; private set; }

    public CompanyStatus Status { get; private set; }

    /// <summary>Why the company was last deactivated, or the note of a reactivation. Null when there is none.</summary>
    public string? StatusReason { get; private set; }

    public DateTime? StatusChangedAt { get; private set; }

    public IReadOnlyList<CompanyStatusHistory> StatusHistory => _statusHistory;

    public bool IsActive => Status == CompanyStatus.Active;

    /// <summary>A5: the audit entries of a company belong to that company.</summary>
    Guid? IAuditCompanyScope.AuditCompanyId => Id;

    /// <summary>R4/D4: the Owner's user id, generated from the company's name (see <see cref="CompanyOwnerUserId"/>).</summary>
    public string OwnerUserId => CompanyOwnerUserId.From(Name);

    /// <summary>R16: the company's URL segment, derived from its name (nothing is stored, a rename changes it).</summary>
    public string Slug => CompanyRules.Slug(Name);

    public static string NormalizeName(string? name) => CompanyRules.NormalizeName(name);

    /// <summary>R16: the URL segment a name would get; used to compare two names.</summary>
    public static string SlugOf(string? name) => CompanyRules.Slug(name);

    /// <summary>R3/R7: a new company is active at once and its timeline starts with the creation row.</summary>
    public static Company Create(CompanyDetails details, Guid actorId, DateTime utcNow)
    {
        var company = new Company { Status = CompanyStatus.Active, StatusChangedAt = utcNow };
        company.Apply(details);
        company._statusHistory.Add(CompanyStatusHistory.Of(company.Id, null, CompanyStatus.Active, string.Empty, actorId, utcNow));
        company.AddDomainEvent(new CompanyCreated(company.Id, company.Name));
        return company;
    }

    /// <summary>R3: changes the master data; the id and the status never change here.</summary>
    public void Edit(CompanyDetails details) => Apply(details);

    /// <summary>R7/R8: switches the company off. A reason is required; the timeline gets one row.</summary>
    public void Deactivate(string? reason, Guid actorId, DateTime utcNow)
    {
        if (Status == CompanyStatus.Inactive)
            throw new ConflictException(CompanyMessages.AlreadyInactive);

        var validated = CompanyRules.RequireReason(reason);
        ChangeStatus(CompanyStatus.Inactive, validated, actorId, utcNow);
        AddDomainEvent(new CompanyDeactivated(Id, validated));
    }

    /// <summary>R7/R8: switches the company on again; the note is optional.</summary>
    public void Reactivate(string? note, Guid actorId, DateTime utcNow)
    {
        if (Status == CompanyStatus.Active)
            throw new ConflictException(CompanyMessages.AlreadyActive);

        ChangeStatus(CompanyStatus.Active, CompanyRules.OptionalReason(note), actorId, utcNow);
        AddDomainEvent(new CompanyReactivated(Id));
    }

    private void ChangeStatus(CompanyStatus to, string reason, Guid actorId, DateTime utcNow)
    {
        _statusHistory.Add(CompanyStatusHistory.Of(Id, Status, to, reason, actorId, utcNow));
        Status = to;
        StatusReason = reason.Length == 0 ? null : reason;
        StatusChangedAt = utcNow;
    }

    private void Apply(CompanyDetails details)
    {
        Name = CompanyRules.RequireName(details.Name);
        CompanyRules.RequireSlug(Name);
        NameNormalized = CompanyRules.NormalizeName(Name);
        Address = CompanyRules.Optional(details.Address, 500);
        Mobile = CompanyRules.Optional(details.Mobile, 20);
        Email = CompanyRules.Optional(details.Email, 254);
        Gstin = CompanyRules.Optional(details.Gstin, 15)?.ToUpperInvariant();
        Pan = CompanyRules.Optional(details.Pan, 10)?.ToUpperInvariant();
        State = CompanyRules.Optional(details.State, 100);
        Tagline = CompanyRules.Optional(details.Tagline, 200);
    }
}
