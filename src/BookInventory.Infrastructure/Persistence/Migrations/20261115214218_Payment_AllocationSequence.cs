using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Payment_AllocationSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sequence",
                schema: "payment",
                table: "receipt_allocation",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sequence",
                schema: "payment",
                table: "receipt_allocation");
        }
    }
}
