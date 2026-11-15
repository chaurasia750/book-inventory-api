using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Planning;

/// <summary>
/// <c>planning.class_book_list</c>: company-owned (R1); one live list per school, session and class (R2); the school is a plain id
/// (school R16), the class is the school's own spelling.
/// </summary>
public sealed class ClassBookListConfiguration() : ModuleEntityConfiguration<ClassBookList>("planning")
{
    protected override void ConfigureEntity(EntityTypeBuilder<ClassBookList> builder)
    {
        builder.Property(l => l.Session).HasMaxLength(ClassBookList.SessionMaxLength).IsRequired();
        builder.Property(l => l.ClassName).HasMaxLength(ClassBookList.ClassNameMaxLength).IsRequired();

        builder.HasIndex(l => new { l.CompanyId, l.SchoolId, l.Session, l.ClassName }).IsUnique().HasFilter("is_deleted = false");

        builder.HasMany(l => l.Lines).WithOne().HasForeignKey(l => l.ClassBookListId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(l => l.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
