using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.BuildingBlocks.Persistence.Naming
{
    /// <summary>
    /// Base for every entity configuration. Places the entity in its module's PostgreSQL schema and gives it a singular
    /// snake_case table name derived from the class (<c>AppUser</c> in schema <c>iam</c> -> <c>iam.app_user</c>).
    /// Derive per entity and implement <see cref="ConfigureEntity"/>; never call <c>ToTable</c> by hand.
    /// </summary>
    /// <param name="schema">The module schema, e.g. <c>iam</c>, <c>mlm</c>, <c>finance</c>, <c>realty</c>, <c>core</c>.</param>
    public abstract class ModuleEntityConfiguration<T>(string schema) : IEntityTypeConfiguration<T> where T : class
    {
        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder.ToTable(SnakeCase.Convert(typeof(T).Name), schema);
            ConfigureEntity(builder);
        }

        protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);
    }
}
