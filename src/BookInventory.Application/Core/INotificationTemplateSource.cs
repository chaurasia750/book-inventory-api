using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// A module's default templates (spec N13). Each source is a class marked <c>[AutoRegister]</c> for this interface; at
/// startup the seeder inserts the codes that do not exist yet and never touches one that does, so an edited template
/// keeps its edits.
/// </summary>
public interface INotificationTemplateSource
{
    IReadOnlyList<TemplateContent> Templates { get; }
}
