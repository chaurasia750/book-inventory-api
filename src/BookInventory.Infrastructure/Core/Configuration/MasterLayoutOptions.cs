namespace BookInventory.Infrastructure.Core.Configuration;

/// <summary>The company details around every HTML email (spec core N4), from <c>MasterLayout:*</c>. All optional.</summary>
public sealed class MasterLayoutOptions
{
    public const string SectionName = "MasterLayout";

    public string CompanyName { get; set; } = "Book Inventory";

    public string CompanyLogo { get; set; } = string.Empty;

    public string CompanyWebsite { get; set; } = string.Empty;

    public string CompanyAddress { get; set; } = string.Empty;

    public string SupportEmail { get; set; } = string.Empty;

    public string SupportPhone { get; set; } = string.Empty;
}
