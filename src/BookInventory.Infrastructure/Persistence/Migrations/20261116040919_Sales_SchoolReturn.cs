using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sales_SchoolReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credit_note",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    financial_year = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    note_date = table.Column<DateOnly>(type: "date", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    school_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    school_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    school_gstin = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    company_state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invoice_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    session = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_gst_applied = table.Column<bool>(type: "boolean", nullable: false),
                    gst_mode = table.Column<string>(type: "text", nullable: false),
                    total_discount_kind = table.Column<string>(type: "text", nullable: false),
                    total_discount_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("pk_credit_note", x => x.id);
                    table.CheckConstraint("ck_credit_note_gst_mode", "\"gst_mode\" IN ('NOT_APPLIED', 'SAME_STATE', 'OTHER_STATE')");
                    table.CheckConstraint("ck_credit_note_reason", "\"reason\" IN ('SESSION_END', 'DAMAGED', 'WRONG_BOOK', 'OTHER')");
                    table.CheckConstraint("ck_credit_note_status", "\"status\" IN ('ACTIVE', 'CANCELLED')");
                    table.CheckConstraint("ck_credit_note_total_discount_kind", "\"total_discount_kind\" IN ('NONE', 'PERCENT', 'AMOUNT')");
                });

            migrationBuilder.CreateTable(
                name: "school_refund",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    financial_year = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    refund_date = table.Column<DateOnly>(type: "date", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    school_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    credit_note_id = table.Column<Guid>(type: "uuid", nullable: true),
                    credit_note_number = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    mode = table.Column<string>(type: "text", nullable: false),
                    reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    cancel_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    cancelled_on = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("pk_school_refund", x => x.id);
                    table.CheckConstraint("ck_school_refund_mode", "\"mode\" IN ('CASH', 'UPI', 'BANK_TRANSFER')");
                    table.CheckConstraint("ck_school_refund_status", "\"status\" IN ('ACTIVE', 'CANCELLED')");
                });

            migrationBuilder.CreateTable(
                name: "credit_note_line",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    credit_note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    invoice_line_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_line_id = table.Column<Guid>(type: "uuid", nullable: true),
                    class_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    condition = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_credit_note_line", x => x.id);
                    table.CheckConstraint("ck_credit_note_line_condition", "\"condition\" IN ('GOOD', 'DAMAGED')");
                    table.CheckConstraint("ck_credit_note_line_item_discount_kind", "\"item_discount_kind\" IN ('NONE', 'PERCENT', 'AMOUNT')");
                    table.ForeignKey(
                        name: "fk_credit_note_line_credit_note_credit_note_id",
                        column: x => x.credit_note_id,
                        principalSchema: "sales",
                        principalTable: "credit_note",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_credit_note_company_id_invoice_id",
                schema: "sales",
                table: "credit_note",
                columns: new[] { "company_id", "invoice_id" });

            migrationBuilder.CreateIndex(
                name: "ix_credit_note_company_id_number",
                schema: "sales",
                table: "credit_note",
                columns: new[] { "company_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_credit_note_company_id_school_id",
                schema: "sales",
                table: "credit_note",
                columns: new[] { "company_id", "school_id" });

            migrationBuilder.CreateIndex(
                name: "ix_credit_note_line_credit_note_id_sort_order",
                schema: "sales",
                table: "credit_note_line",
                columns: new[] { "credit_note_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_credit_note_line_invoice_line_id",
                schema: "sales",
                table: "credit_note_line",
                column: "invoice_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_school_refund_company_id_credit_note_id",
                schema: "sales",
                table: "school_refund",
                columns: new[] { "company_id", "credit_note_id" });

            migrationBuilder.CreateIndex(
                name: "ix_school_refund_company_id_number",
                schema: "sales",
                table: "school_refund",
                columns: new[] { "company_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_school_refund_company_id_school_id",
                schema: "sales",
                table: "school_refund",
                columns: new[] { "company_id", "school_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_note_line",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "school_refund",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "credit_note",
                schema: "sales");
        }
    }
}
