using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Iam.History;

/// <summary>Decides who a log row is shown as (R48). One place, so no screen or export can forget the hidden account.</summary>
public interface IActorLabelResolver
{
    ActorLabel Resolve(LoginLogRow row);
}

/// <summary>
/// R48: the hidden system account is reported as <see cref="ActorLabel.IsSystem"/> with no user name and no id - the screen
/// shows the fixed label "System" - while every other row keeps its real user. A row with no user (an unknown identifier)
/// has no actor at all.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IActorLabelResolver))]
public sealed class ActorLabelResolver : IActorLabelResolver
{
    public ActorLabel Resolve(LoginLogRow row) =>
        row.IsSystemAccount
            ? new ActorLabel(UserId: null, UserName: null, IsSystem: true)
            : new ActorLabel(row.UserId, row.UserName, IsSystem: false);
}
