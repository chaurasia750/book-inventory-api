using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sales_OrdersAndInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.CreateTable(
                name: "invoice",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    financial_year = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    invoice_date = table.Column<DateOnly>(type: "date", nullable: false),
                    school_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    school_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    school_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    school_gstin = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    company_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    session = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_gst_applied = table.Column<bool>(type: "boolean", nullable: false),
                    gst_mode = table.Column<string>(type: "text", nullable: false),
                    total_discount_kind = table.Column<string>(type: "text", nullable: false),
                    total_discount_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    cancel_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancelled_on = table.Column<DateOnly>(type: "date", nullable: true),
                    cgst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    grand_total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    igst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    round_off_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sgst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxable_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("pk_invoice", x => x.id);
                    table.CheckConstraint("ck_invoice_gst_mode", "\"gst_mode\" IN ('NOT_APPLIED', 'SAME_STATE', 'OTHER_STATE')");
                    table.CheckConstraint("ck_invoice_status", "\"status\" IN ('ISSUED', 'CANCELLED')");
                    table.CheckConstraint("ck_invoice_total_discount_kind", "\"total_discount_kind\" IN ('NONE', 'PERCENT', 'AMOUNT')");
                });

            migrationBuilder.CreateTable(
                name: "school_order",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    school_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    school_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    order_date = table.Column<DateOnly>(type: "date", nullable: false),
                    session = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_gst_applied = table.Column<bool>(type: "boolean", nullable: false),
                    gst_mode = table.Column<string>(type: "text", nullable: false),
                    total_discount_kind = table.Column<string>(type: "text", nullable: false),
                    total_discount_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    close_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    total_discount_given = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cgst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    grand_total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    igst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    round_off_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sgst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxable_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("pk_school_order", x => x.id);
                    table.CheckConstraint("ck_school_order_gst_mode", "\"gst_mode\" IN ('NOT_APPLIED', 'SAME_STATE', 'OTHER_STATE')");
                    table.CheckConstraint("ck_school_order_status", "\"status\" IN ('DRAFT', 'CONFIRMED', 'CLOSED')");
                    table.CheckConstraint("ck_school_order_total_discount_kind", "\"total_discount_kind\" IN ('NONE', 'PERCENT', 'AMOUNT')");
                });

            migrationBuilder.CreateTable(
                name: "invoice_line",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    item_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    hsn_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    gst_rate_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    price_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_version_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    mrp_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    item_discount_kind = table.Column<string>(type: "text", nullable: false),
                    item_discount_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    gross_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    item_discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    line_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_discount_share_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxable_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cgst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sgst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    igst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_line", x => x.id);
                    table.CheckConstraint("ck_invoice_line_item_discount_kind", "\"item_discount_kind\" IN ('NONE', 'PERCENT', 'AMOUNT')");
                    table.ForeignKey(
                        name: "fk_invoice_line_invoice_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "sales",
                        principalTable: "invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_order_line",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    item_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    hsn_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    gst_rate_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    price_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_version_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    mrp_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    item_discount_kind = table.Column<string>(type: "text", nullable: false),
                    item_discount_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    gross_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    item_discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    line_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_discount_share_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    taxable_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cgst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sgst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    igst_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    invoiced_quantity = table.Column<int>(type: "integer", nullable: false),
                    item_discount_given = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_school_order_line", x => x.id);
                    table.CheckConstraint("ck_school_order_line_item_discount_kind", "\"item_discount_kind\" IN ('NONE', 'PERCENT', 'AMOUNT')");
                    table.ForeignKey(
                        name: "fk_school_order_line_school_order_school_order_id",
                        column: x => x.school_order_id,
                        principalSchema: "sales",
                        principalTable: "school_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_invoice_company_id_number",
                schema: "sales",
                table: "invoice",
                columns: new[] { "company_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoice_company_id_school_id_invoice_date",
                schema: "sales",
                table: "invoice",
                columns: new[] { "company_id", "school_id", "invoice_date" });

            migrationBuilder.CreateIndex(
                name: "ix_invoice_school_order_id",
                schema: "sales",
                table: "invoice",
                column: "school_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoice_line_invoice_id_sort_order",
                schema: "sales",
                table: "invoice_line",
                columns: new[] { "invoice_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_school_order_company_id_number",
                schema: "sales",
                table: "school_order",
                columns: new[] { "company_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_school_order_company_id_school_id_order_date",
                schema: "sales",
                table: "school_order",
                columns: new[] { "company_id", "school_id", "order_date" });

            migrationBuilder.CreateIndex(
                name: "ix_school_order_company_id_status",
                schema: "sales",
                table: "school_order",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_school_order_line_school_order_id_sort_order",
                schema: "sales",
                table: "school_order_line",
                columns: new[] { "school_order_id", "sort_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invoice_line",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "school_order_line",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "invoice",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "school_order",
                schema: "sales");
        }
    }
}
