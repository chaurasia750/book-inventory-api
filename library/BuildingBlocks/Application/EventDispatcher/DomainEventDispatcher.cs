using System.Collections.Concurrent;
using BookInventory.BuildingBlocks.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.BuildingBlocks.Application.EventDispatcher
{
    /// <summary>
    /// Resolves every <see cref="IDomainEventHandler{TEvent}"/> registered for the runtime type of the
    /// event and runs them sequentially, in registration order. No reflection on the hot path:
    /// a typed invoker is built once per event type and cached.
    /// </summary>
    public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
    {
        private static readonly ConcurrentDictionary<Type, HandlerInvoker> Invokers = new();

        public Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            var invoker = Invokers.GetOrAdd(domainEvent.GetType(),
                static type => (HandlerInvoker)Activator.CreateInstance(typeof(HandlerInvoker<>).MakeGenericType(type))!);

            return invoker.InvokeAsync(serviceProvider, domainEvent, ct);
        }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(domainEvents);

            foreach (var domainEvent in domainEvents)
            {
                await DispatchAsync(domainEvent, ct);
            }
        }

        private abstract class HandlerInvoker
        {
            public abstract Task InvokeAsync(IServiceProvider services, IDomainEvent domainEvent, CancellationToken ct);
        }

        private sealed class HandlerInvoker<TEvent> : HandlerInvoker where TEvent : IDomainEvent
        {
            public override async Task InvokeAsync(IServiceProvider services, IDomainEvent domainEvent, CancellationToken ct)
            {
                foreach (var handler in services.GetServices<IDomainEventHandler<TEvent>>())
                {
                    await handler.HandleAsync((TEvent)domainEvent, ct);
                }
            }
        }
    }
}
