using System.ComponentModel;
using System.Reflection;

namespace BookInventory.BuildingBlocks.Domain.Extensions
{
    public static class EnumExtension
    {
        /// <summary>Returns the <see cref="DescriptionAttribute"/> text of the enum member, or its name.</summary>
        public static string GetDescription(this Enum value)
        {
            var name = value.ToString();
            var member = value.GetType().GetField(name, BindingFlags.Public | BindingFlags.Static);
            return member?.GetCustomAttribute<DescriptionAttribute>(inherit: false)?.Description ?? name;
        }
    }
}
