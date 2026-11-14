using System.Linq.Expressions;
using System.Reflection;
using BookInventory.BuildingBlocks.Domain.Models;

namespace BookInventory.BuildingBlocks.Persistence.Extensions
{
    public static class SortHelper
    {
        /// <summary>
        /// Builds an order-by string ("Name Asc") for <see cref="OrderByExtension"/> from a client supplied property
        /// name, validating it against <typeparamref name="TEntity"/>. Falls back to <paramref name="sortExpression"/>.
        /// </summary>
        public static string? OrderBy<TEntity>(string? orderByProperty, SortingOrder? sortingOrder = SortingOrder.Asc,
            Expression<Func<TEntity, object>>? sortExpression = null)
        {
            if (!string.IsNullOrWhiteSpace(orderByProperty))
            {
                var property = typeof(TEntity).GetProperty(orderByProperty.Trim(),
                                   BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                               ?? throw new ArgumentException(
                                   $"'{orderByProperty}' is not a sortable property of {typeof(TEntity).Name}.");
                orderByProperty = property.Name;
            }
            else if (sortExpression is not null)
            {
                orderByProperty = sortExpression.ToStringForOrdering();
            }

            return string.IsNullOrWhiteSpace(orderByProperty) ? null : $"{orderByProperty} {sortingOrder}";
        }
    }

    public static class ExpressionExtensions
    {
        public static string ToStringForOrdering<T, TKey>(this Expression<Func<T, TKey>> expression, bool isDesc = false)
        {
            var body = expression.Body;
            while (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
                body = unary.Operand;

            var path = body.ToString();
            var prefix = expression.Parameters[0].Name + ".";
            if (path.StartsWith(prefix, StringComparison.Ordinal))
                path = path[prefix.Length..];

            return isDesc ? path + " desc" : path;
        }
    }
}
