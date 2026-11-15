using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Planning;

/// <summary><c>planning.class_book_list_line</c>: the books of a class list with the quantity per student (R2).</summary>
public sealed class ClassBookListLineConfiguration() : ModuleEntityConfiguration<ClassBookListLine>("planning")
{
    protected override void ConfigureEntity(EntityTypeBuilder<ClassBookListLine> builder)
    {
        builder.Property(l => l.ItemCode).HasMaxLength(30).IsRequired();
        builder.Property(l => l.ItemName).HasMaxLength(200).IsRequired();
        builder.HasIndex(l => new { l.ClassBookListId, l.SortOrder });
    }
}
