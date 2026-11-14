using System.Linq.Expressions;
using System.Reflection;

namespace BookInventory.BuildingBlocks.Persistence.Extensions
{
    /// <summary>
    /// String based ordering, e.g. <c>query.OrderBy("Name, CreatedDate DESC")</c>. Property names are matched
    /// case-insensitively against the entity and unknown names are rejected, so client supplied sort keys are safe.
    /// Null values always sort last. Defaults to <c>Id DESC</c>.
    /// </summary>
    public static class OrderByExtension
    {
        private const string DefaultOrder = "Id DESC";

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string? orderBy) =>
            Apply(source, orderBy, initial: true);

        public static IOrderedQueryable<T> ThenBy<T>(this IQueryable<T> source, string? orderBy) =>
            Apply(source, orderBy, initial: false);

        public static IOrderedQueryable<T> OrderBy<T>(this IEnumerable<T> source, string? orderBy) =>
            source.AsQueryable().OrderBy(orderBy);

        public static IOrderedQueryable<T> ThenBy<T>(this IEnumerable<T> source, string? orderBy) =>
            source.AsQueryable().ThenBy(orderBy);

        private static IOrderedQueryable<T> Apply<T>(IQueryable<T> source, string? orderBy, bool initial)
        {
            if (string.IsNullOrWhiteSpace(orderBy))
                orderBy = DefaultOrder;

            var parameter = Expression.Parameter(typeof(T), "x");
            var expression = source.Expression;

            foreach (var (propertyPath, descending) in Parse(orderBy))
            {
                var member = ResolveMember(parameter, propertyPath);

                if (!member.Type.IsValueType || Nullable.GetUnderlyingType(member.Type) is not null)
                {
                    // CASE WHEN member IS NULL THEN 0 ELSE 1 END DESC  => nulls last
                    var nullRank = Expression.Condition(
                        Expression.Equal(member, Expression.Constant(null, member.Type)),
                        Expression.Constant(0), Expression.Constant(1));
                    expression = CallOrderBy(expression, Expression.Lambda(nullRank, parameter), descending: true, initial);
                    initial = false;
                }

                expression = CallOrderBy(expression, Expression.Lambda(member, parameter), descending, initial);
                initial = false;
            }

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(expression);
        }

        private static Expression ResolveMember(Expression root, string propertyPath)
        {
            var current = root;
            foreach (var name in propertyPath.Split('.'))
            {
                var property = current.Type.GetProperty(name,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                    ?? throw new ArgumentException($"'{propertyPath}' is not a sortable property of {root.Type.Name}.");
                current = Expression.Property(current, property);
            }
            return current;
        }

        private static MethodCallExpression CallOrderBy(Expression source, LambdaExpression selector, bool descending, bool initial)
        {
            var method = (descending, initial) switch
            {
                (false, true) => nameof(Queryable.OrderBy),
                (true, true) => nameof(Queryable.OrderByDescending),
                (false, false) => nameof(Queryable.ThenBy),
                (true, false) => nameof(Queryable.ThenByDescending)
            };

            return Expression.Call(typeof(Queryable), method,
                [selector.Parameters[0].Type, selector.Body.Type], source, Expression.Quote(selector));
        }

        private static IEnumerable<(string PropertyPath, bool Descending)> Parse(string orderBy)
        {
            foreach (var item in orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var parts = item.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length is 0 or > 2)
                    throw new ArgumentException(
                        $"Invalid order by '{item}'. Format: Property, Property2 ASC, Property3 DESC");

                yield return (parts[0], parts.Length == 2 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
