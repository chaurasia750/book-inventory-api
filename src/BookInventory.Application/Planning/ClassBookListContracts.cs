using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Planning;

namespace BookInventory.Application.Planning;

/// <summary>R2: one book of a list: the item and how many copies each student needs.</summary>
public sealed record BookListLineRequest(
    [param: Required] Guid? ItemId,
    [param: Required, Range(ClassBookList.MinQuantityPerStudent, ClassBookList.MaxQuantityPerStudent)] int? QuantityPerStudent);

/// <summary>R2, R3, R5: the whole list of one class (it replaces what is there).</summary>
public sealed record SaveBookListRequest(
    [param: Range(0, ClassBookList.MaxStudentEstimate)] int? StudentEstimate,
    [param: Required, MaxLength(ClassBookList.MaxLines)] IReadOnlyList<BookListLineRequest>? Lines);

/// <summary>R4: copy every class list of a session (of this school, or of <see cref="FromSchoolId"/>).</summary>
public sealed record CopyBookListsRequest(
    Guid? FromSchoolId,
    [param: Required, StringLength(ClassBookList.SessionMaxLength)] string? FromSession,
    bool ReplaceExisting = false);

/// <summary>R4: which classes were copied and which were skipped (they already had a list, or the target school lacks the class).</summary>
public sealed record CopyBookListsResponse(IReadOnlyList<string> Copied, IReadOnlyList<string> Skipped);

/// <summary>R7: how a book differs from the previous session's list of the class.</summary>
public enum BookListChange
{
    Same,
    Added,
    Changed
}

/// <summary>R7: one book of a class list, with its change against the previous session (and the previous quantity when changed).</summary>
public sealed record BookListLineResponse(Guid ItemId, string ItemCode, string ItemName, int QuantityPerStudent, BookListChange Change, int? PreviousQuantityPerStudent);

/// <summary>R7: a book the previous session's list had and this one does not.</summary>
public sealed record RemovedBookResponse(Guid ItemId, string ItemCode, string ItemName, int QuantityPerStudent);

/// <summary>R7: one class of the school: its list (none: <see cref="HasList"/> false), estimate, books and removed books.</summary>
public sealed record ClassBookListResponse(
    string ClassName,
    bool HasList,
    int? StudentEstimate,
    IReadOnlyList<BookListLineResponse> Lines,
    IReadOnlyList<RemovedBookResponse> Removed);

/// <summary>R7: every class of a school for one session, compared with the session before it.</summary>
public sealed record SchoolBookListsResponse(Guid SchoolId, string Session, string PreviousSession, IReadOnlyList<ClassBookListResponse> Classes);
