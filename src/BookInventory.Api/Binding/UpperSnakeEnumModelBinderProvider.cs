using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BookInventory.Api.Binding;

/// <summary>
/// Query-string enum binding that accepts the <em>wire</em> spelling of an enum. The API sends and reads enums as
/// UPPER_SNAKE (<c>SUPER_ADMIN</c>) in every JSON body, because that is the platform convention
/// (<c>JsonStringEnumConverter</c> with <see cref="JsonNamingPolicy.SnakeCaseUpper"/>), and a client has to be able to
/// send the same spelling back in <c>?role=</c> and <c>?status=</c>. ASP.NET's own enum binder only knows the C# member
/// name (<c>SuperAdmin</c>), which would make the filter of R38 unusable and would give the same enum two spellings
/// depending on the transport.
/// <para>
/// It reads the names through the same naming policy as the JSON converter, so there is one definition of the wire
/// format: the policy plus the enum itself. A value that is not a member is a 400 (a model error, not a silent null).
/// </para>
/// </summary>
public sealed class UpperSnakeEnumModelBinderProvider : IModelBinderProvider
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, object>> Names = new();

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = context.Metadata.ModelType;
        var underlying = Nullable.GetUnderlyingType(type);
        var enumType = type.IsEnum ? type : underlying;

        // Nullable<Guid>, Nullable<DateTime> and Nullable<bool> also have an underlying type; only an enum is ours to bind.
        return enumType is { IsEnum: true } ? new UpperSnakeEnumModelBinder(enumType, underlying is not null) : null;
    }

    /// <summary>
    /// One key per member - its UPPER_SNAKE spelling, compared case-insensitively like the JSON converter does. Only
    /// that spelling is registered: accepting the C# member name as a second key would collide with it whenever the
    /// member is a single word (<c>ACCOUNTS</c> against <c>Accounts</c>), and the wire format must not have two names
    /// per enum.
    /// </summary>
    private static IReadOnlyDictionary<string, object> NamesFor(Type enumType) =>
        Names.GetOrAdd(enumType, static type =>
        {
            var members = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in Enum.GetNames(type))
            {
                var member = Enum.Parse(type, name);
                // TryAdd: a single-word member (Accounts) has the same text in both spellings, which is one key, not a clash.
                members.TryAdd(JsonNamingPolicy.SnakeCaseUpper.ConvertName(name) ?? name, member);
                members.TryAdd(name, member);
            }

            return members;
        });

    private sealed class UpperSnakeEnumModelBinder(Type enumType, bool isNullable) : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var text = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).ToString();

            if (string.IsNullOrWhiteSpace(text))
            {
                // An absent or empty filter means "no filter", not "a value that is not a member".
                if (isNullable)
                    bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            if (NamesFor(enumType).TryGetValue(text.Trim(), out var member))
            {
                bindingContext.Result = ModelBindingResult.Success(member);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"The value '{text}' is not valid for {enumType.Name}.");
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }
    }
}
