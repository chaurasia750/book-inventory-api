using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Finance_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "finance");

            migrationBuilder.CreateTable(
                name: "ledger_transaction",
                schema: "finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    financial_year = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    session = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    posting_kind = table.Column<string>(type: "text", nullable: false),
                    narration = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("pk_ledger_transaction", x => x.id);
                    table.CheckConstraint("ck_ledger_transaction_posting_kind", "\"posting_kind\" IN ('POST', 'REVERSAL')");
                });

            migrationBuilder.CreateTable(
                name: "ledger_line",
                schema: "finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ledger_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account = table.Column<string>(type: "text", nullable: false),
                    party_type = table.Column<string>(type: "text", nullable: true),
                    party_id = table.Column<Guid>(type: "uuid", nullable: true),
                    debit_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    financial_year = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    session = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ledger_line", x => x.id);
                    table.CheckConstraint("ck_ledger_line_account", "\"account\" IN ('SCHOOL_RECEIVABLE', 'CUSTOMER_RECEIVABLE', 'VENDOR_PAYABLE', 'SALES', 'SALES_RETURN', 'PURCHASE', 'PURCHASE_RETURN', 'GST_OUTPUT', 'GST_INPUT', 'CASH', 'BANK', 'CHEQUE_IN_HAND', 'DISCOUNT_ALLOWED', 'ROUND_OFF', 'OPENING_BALANCE')");
                    table.CheckConstraint("ck_ledger_line_one_side", "(debit_amount > 0 AND credit_amount = 0) OR (credit_amount > 0 AND debit_amount = 0)");
                    table.CheckConstraint("ck_ledger_line_party_type", "\"party_type\" IN ('SCHOOL', 'VENDOR', 'CUSTOMER')");
                    table.ForeignKey(
                        name: "fk_ledger_line_ledger_transaction_ledger_transaction_id",
                        column: x => x.ledger_transaction_id,
                        principalSchema: "finance",
                        principalTable: "ledger_transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_line_company_id_account",
                schema: "finance",
                table: "ledger_line",
                columns: new[] { "company_id", "account" });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_line_company_id_party_type_party_id_transaction_date",
                schema: "finance",
                table: "ledger_line",
                columns: new[] { "company_id", "party_type", "party_id", "transaction_date" });

            migrationBuilder.CreateIndex(
                name: "ix_ledger_line_ledger_transaction_id",
                schema: "finance",
                table: "ledger_line",
                column: "ledger_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transaction_document_posting",
                schema: "finance",
                table: "ledger_transaction",
                columns: new[] { "company_id", "document_type", "document_id", "posting_kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ledger_line",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "ledger_transaction",
                schema: "finance");
        }
    }
}
