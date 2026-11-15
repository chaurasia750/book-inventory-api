using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Payment_Receipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "payment");

            migrationBuilder.CreateTable(
                name: "receipt",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    financial_year = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    client_request_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("pk_receipt", x => x.id);
                    table.CheckConstraint("ck_receipt_status", "\"status\" IN ('RECORDED', 'CANCELLED')");
                });

            migrationBuilder.CreateTable(
                name: "receipt_allocation",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receipt_allocation", x => x.id);
                    table.CheckConstraint("ck_receipt_allocation_reason", "\"reason\" IN ('ALLOCATED', 'BOUNCED', 'INVOICE_CANCELLED', 'RECEIPT_CANCELLED', 'ADJUSTED')");
                    table.ForeignKey(
                        name: "fk_receipt_allocation_receipt_receipt_id",
                        column: x => x.receipt_id,
                        principalSchema: "payment",
                        principalTable: "receipt",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "receipt_mode",
                schema: "payment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    receipt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode_type = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cheque_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cheque_date = table.Column<DateOnly>(type: "date", nullable: true),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cheque_status = table.Column<string>(type: "text", nullable: true),
                    result_date = table.Column<DateOnly>(type: "date", nullable: true),
                    bounce_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_receipt_mode", x => x.id);
                    table.CheckConstraint("ck_receipt_mode_cheque_status", "\"cheque_status\" IN ('PENDING', 'CLEARED', 'BOUNCED')");
                    table.CheckConstraint("ck_receipt_mode_mode_type", "\"mode_type\" IN ('CASH', 'UPI', 'BANK_TRANSFER', 'CHEQUE')");
                    table.ForeignKey(
                        name: "fk_receipt_mode_receipt_receipt_id",
                        column: x => x.receipt_id,
                        principalSchema: "payment",
                        principalTable: "receipt",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_receipt_company_id_client_request_id",
                schema: "payment",
                table: "receipt",
                columns: new[] { "company_id", "client_request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_receipt_company_id_number",
                schema: "payment",
                table: "receipt",
                columns: new[] { "company_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_receipt_company_id_school_id_receipt_date",
                schema: "payment",
                table: "receipt",
                columns: new[] { "company_id", "school_id", "receipt_date" });

            migrationBuilder.CreateIndex(
                name: "ix_receipt_allocation_invoice_id",
                schema: "payment",
                table: "receipt_allocation",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_receipt_allocation_receipt_id",
                schema: "payment",
                table: "receipt_allocation",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "ix_receipt_mode_receipt_id",
                schema: "payment",
                table: "receipt_mode",
                column: "receipt_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "receipt_allocation",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "receipt_mode",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "receipt",
                schema: "payment");
        }
    }
}
