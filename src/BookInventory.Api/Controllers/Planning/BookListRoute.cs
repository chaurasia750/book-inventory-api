using BookInventory.Application.Planning;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Planning;

/// <summary>Spec planning R2: the route values that name one class book list.</summary>
public sealed class BookListRoute
{
    [FromRoute(Name = "schoolId")]
    public Guid SchoolId { get; init; }

    [FromRoute(Name = "session")]
    public string Session { get; init; } = string.Empty;

    [FromRoute(Name = "className")]
    public string ClassName { get; init; } = string.Empty;

    public BookListKey ToKey() => new(SchoolId, Session, ClassName);
}
