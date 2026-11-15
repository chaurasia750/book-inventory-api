using System.ComponentModel.DataAnnotations;

namespace BookInventory.Application.School;

/// <summary>R19: the two cut-offs (month and day).</summary>
public sealed record SessionCutOffsResponse(int AprMarMonth, int AprMarDay, int JanDecMonth, int JanDecDay);

/// <summary>R19: the Owner's change of the two cut-offs; the domain checks each day against its month and session type.</summary>
public sealed record ChangeSessionCutOffsRequest(
    [param: Required, Range(1, 12)] int? AprMarMonth,
    [param: Required, Range(1, 31)] int? AprMarDay,
    [param: Required, Range(1, 12)] int? JanDecMonth,
    [param: Required, Range(1, 31)] int? JanDecDay);
