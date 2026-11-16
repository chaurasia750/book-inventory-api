using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Purchase_VendorReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "purchase");

            migrationBuilder.CreateTable(
                name: "vendor_return",
                schema: "purchase",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    financial_year = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    return_date = table.Column<DateOnly>(type: "date", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    vendor_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    round_off_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    grand_total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    cancel_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancelled_on = table.Column<DateOnly>(type: "date", nullable: true),
                    company_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    company_bank_account_holder = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    company_bank_account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    company_bank_branch = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    company_bank_ifsc = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    company_bank_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    company_email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    company_footer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    company_gstin = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    company_mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    company_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    company_pan = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    company_print_gstin = table.Column<bool>(type: "boolean", nullable: false),
                    company_snapshot_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    company_tagline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    company_terms = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    company_upi_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("pk_vendor_return", x => x.id);
                    table.CheckConstraint("ck_vendor_return_status", "\"status\" IN ('ACTIVE', 'CANCELLED')");
                });

            migrationBuilder.CreateTable(
                name: "vendor_return_line",
                schema: "purchase",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vendor_return_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    item_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    stock_source = table.Column<string>(type: "text", nullable: false),
                    rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    line_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vendor_return_line", x => x.id);
                    table.CheckConstraint("ck_vendor_return_line_stock_source", "\"stock_source\" IN ('MAIN', 'DAMAGED')");
                    table.ForeignKey(
                        name: "fk_vendor_return_line_vendor_return_vendor_return_id",
                        column: x => x.vendor_return_id,
                        principalSchema: "purchase",
                        principalTable: "vendor_return",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vendor_return_company_id_number",
                schema: "purchase",
                table: "vendor_return",
                columns: new[] { "company_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendor_return_company_id_return_date",
                schema: "purchase",
                table: "vendor_return",
                columns: new[] { "company_id", "return_date" });

            migrationBuilder.CreateIndex(
                name: "ix_vendor_return_company_id_vendor_id",
                schema: "purchase",
                table: "vendor_return",
                columns: new[] { "company_id", "vendor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_vendor_return_line_vendor_return_id_sort_order",
                schema: "purchase",
                table: "vendor_return_line",
                columns: new[] { "vendor_return_id", "sort_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vendor_return_line",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "vendor_return",
                schema: "purchase");
        }
    }
}
