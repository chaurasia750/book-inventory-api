using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class School_PaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_type",
                schema: "school",
                table: "school",
                type: "text",
                nullable: false,
                defaultValue: "CREDIT");

            migrationBuilder.AddCheckConstraint(
                name: "ck_school_payment_type",
                schema: "school",
                table: "school",
                sql: "\"payment_type\" IN ('CASH', 'CREDIT')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_school_payment_type",
                schema: "school",
                table: "school");

            migrationBuilder.DropColumn(
                name: "payment_type",
                schema: "school",
                table: "school");
        }
    }
}
