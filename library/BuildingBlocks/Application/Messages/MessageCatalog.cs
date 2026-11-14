using BookInventory.BuildingBlocks.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookInventory.BuildingBlocks.Application.Messages
{
    /// <summary>The message codes and texts one module contributes to the catalogue.</summary>
    public sealed record MessageSource(IReadOnlyDictionary<string, string> Messages);

    /// <summary>Merges every registered <see cref="MessageSource"/>; a code registered twice fails at startup.</summary>
    public sealed class MessageCatalog : IMessageCatalog
    {
        private readonly Dictionary<string, string> _messages = new(StringComparer.Ordinal);

        public MessageCatalog(IEnumerable<MessageSource> sources)
        {
            foreach (var pair in sources.SelectMany(s => s.Messages))
            {
                if (!_messages.TryAdd(pair.Key, pair.Value))
                    throw new InvalidOperationException($"Message code '{pair.Key}' is registered more than once.");
            }
        }

        public string? Find(string code) => _messages.GetValueOrDefault(code);
    }

    public static class MessageCatalogExtensions
    {
        /// <summary>Registers a module's message codes and texts (call once per module).</summary>
        public static IServiceCollection AddMessages(this IServiceCollection services, IReadOnlyDictionary<string, string> messages)
        {
            services.AddSingleton(new MessageSource(messages));
            services.TryAddSingleton<IMessageCatalog, MessageCatalog>();
            return services;
        }
    }
}
