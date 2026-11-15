using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sales_PaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_type",
                schema: "sales",
                table: "school_order",
                type: "text",
                nullable: false,
                defaultValue: "CREDIT");

            migrationBuilder.AddColumn<string>(
                name: "payment_type",
                schema: "sales",
                table: "invoice",
                type: "text",
                nullable: false,
                defaultValue: "CREDIT");

            migrationBuilder.AddCheckConstraint(
                name: "ck_school_order_payment_type",
                schema: "sales",
                table: "school_order",
                sql: "\"payment_type\" IN ('CASH', 'CREDIT')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_invoice_payment_type",
                schema: "sales",
                table: "invoice",
                sql: "\"payment_type\" IN ('CASH', 'CREDIT')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_school_order_payment_type",
                schema: "sales",
                table: "school_order");

            migrationBuilder.DropCheckConstraint(
                name: "ck_invoice_payment_type",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "payment_type",
                schema: "sales",
                table: "school_order");

            migrationBuilder.DropColumn(
                name: "payment_type",
                schema: "sales",
                table: "invoice");
        }
    }
}
