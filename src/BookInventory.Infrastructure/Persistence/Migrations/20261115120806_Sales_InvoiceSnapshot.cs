using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sales_InvoiceSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "company_address",
                schema: "sales",
                table: "invoice",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_bank_account_holder",
                schema: "sales",
                table: "invoice",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_bank_account_number",
                schema: "sales",
                table: "invoice",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_bank_branch",
                schema: "sales",
                table: "invoice",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_bank_ifsc",
                schema: "sales",
                table: "invoice",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_bank_name",
                schema: "sales",
                table: "invoice",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_email",
                schema: "sales",
                table: "invoice",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_footer",
                schema: "sales",
                table: "invoice",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_gstin",
                schema: "sales",
                table: "invoice",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_mobile",
                schema: "sales",
                table: "invoice",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_name",
                schema: "sales",
                table: "invoice",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "company_pan",
                schema: "sales",
                table: "invoice",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "company_print_gstin",
                schema: "sales",
                table: "invoice",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "company_snapshot_state",
                schema: "sales",
                table: "invoice",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_tagline",
                schema: "sales",
                table: "invoice",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_terms",
                schema: "sales",
                table: "invoice",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_upi_id",
                schema: "sales",
                table: "invoice",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "school_address",
                schema: "sales",
                table: "invoice",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            // Invoices made before the snapshot existed take the company's and the school's current printed values, once, so a
            // reprint of them is complete (an invoice made from now on keeps its own).
            migrationBuilder.Sql(
                """
                UPDATE sales.invoice i SET
                    company_name = c.name, company_tagline = c.tagline, company_address = c.address, company_mobile = c.mobile,
                    company_email = c.email, company_gstin = c.gstin, company_pan = c.pan, company_snapshot_state = c.state,
                    company_bank_account_holder = c.bank_account_holder, company_bank_name = c.bank_name,
                    company_bank_account_number = c.bank_account_number, company_bank_ifsc = c.bank_ifsc,
                    company_bank_branch = c.bank_branch, company_upi_id = c.upi_id, company_terms = c.invoice_terms,
                    company_footer = c.invoice_footer, company_print_gstin = c.print_gstin
                FROM company.company c WHERE c.id = i.company_id AND i.company_name = '';
                UPDATE sales.invoice i SET school_address = s.address
                FROM school.school s WHERE s.id = i.school_id AND i.school_address IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "company_address",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_bank_account_holder",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_bank_account_number",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_bank_branch",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_bank_ifsc",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_bank_name",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_email",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_footer",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_gstin",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_mobile",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_name",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_pan",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_print_gstin",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_snapshot_state",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_tagline",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_terms",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "company_upi_id",
                schema: "sales",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "school_address",
                schema: "sales",
                table: "invoice");
        }
    }
}
