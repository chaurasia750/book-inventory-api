using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sales_WholesaleOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "class_name",
                schema: "sales",
                table: "school_order_line",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "class_name",
                schema: "sales",
                table: "invoice_line",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "school_invoice_layout",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    layout = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_school_invoice_layout", x => x.id);
                    table.CheckConstraint("ck_school_invoice_layout_layout", "\"layout\" IN ('BOOK_LIST', 'BY_CLASS')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_school_invoice_layout_company_id_school_id",
                schema: "sales",
                table: "school_invoice_layout",
                columns: new[] { "company_id", "school_id" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "school_invoice_layout",
                schema: "sales");

            migrationBuilder.DropColumn(
                name: "class_name",
                schema: "sales",
                table: "school_order_line");

            migrationBuilder.DropColumn(
                name: "class_name",
                schema: "sales",
                table: "invoice_line");
        }
    }
}
