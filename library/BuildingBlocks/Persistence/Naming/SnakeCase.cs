using System.Text;

namespace BookInventory.BuildingBlocks.Persistence.Naming
{
    /// <summary>The single snake_case algorithm used for table names, enum values and check-constraint columns.</summary>
    public static class SnakeCase
    {
        /// <summary><c>AppUser</c> -> <c>app_user</c>, <c>SUPER_ADMIN</c> stays as is in lower form, <c>UserID</c> -> <c>user_id</c>.</summary>
        public static string Convert(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            var sb = new StringBuilder(name.Length + 8);
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c))
                {
                    var previousIsLowerOrDigit = i > 0 && (char.IsLower(name[i - 1]) || char.IsDigit(name[i - 1]));
                    var startsNewWord = i > 0 && char.IsUpper(name[i - 1]) && i + 1 < name.Length && char.IsLower(name[i + 1]);
                    if (previousIsLowerOrDigit || startsNewWord)
                        sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary><c>SuperAdmin</c> -> <c>SUPER_ADMIN</c>.</summary>
        public static string ConvertUpper(string name) => Convert(name).ToUpperInvariant();
    }
}
