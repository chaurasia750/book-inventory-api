using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookInventory.BuildingBlocks.Persistence.Naming
{
    /// <summary>Stores an enum as readable UPPER_SNAKE text (<c>SuperAdmin</c> &lt;-&gt; <c>SUPER_ADMIN</c>).</summary>
    public sealed class UpperSnakeEnumConverter<TEnum>() : ValueConverter<TEnum, string>(
        value => SnakeCase.ConvertUpper(value.ToString()),
        text => FromDatabase(text)) where TEnum : struct, Enum
    {
        private static TEnum FromDatabase(string text)
        {
            foreach (var value in Enum.GetValues<TEnum>())
            {
                if (SnakeCase.ConvertUpper(value.ToString()) == text)
                    return value;
            }

            throw new InvalidOperationException($"'{text}' is not a valid {typeof(TEnum).Name} value.");
        }
    }
}
