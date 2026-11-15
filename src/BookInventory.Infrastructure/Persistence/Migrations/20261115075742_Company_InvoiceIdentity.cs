using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Company_InvoiceIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bank_account_holder",
                schema: "company",
                table: "company",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_account_number",
                schema: "company",
                table: "company",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_branch",
                schema: "company",
                table: "company",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_ifsc",
                schema: "company",
                table: "company",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_name",
                schema: "company",
                table: "company",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_footer",
                schema: "company",
                table: "company",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invoice_terms",
                schema: "company",
                table: "company",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "logo",
                schema: "company",
                table: "company",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logo_content_type",
                schema: "company",
                table: "company",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "print_gstin",
                schema: "company",
                table: "company",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "signature",
                schema: "company",
                table: "company",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "signature_content_type",
                schema: "company",
                table: "company",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "upi_id",
                schema: "company",
                table: "company",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bank_account_holder",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "bank_account_number",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "bank_branch",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "bank_ifsc",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "bank_name",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "invoice_footer",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "invoice_terms",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "logo",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "logo_content_type",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "print_gstin",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "signature",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "signature_content_type",
                schema: "company",
                table: "company");

            migrationBuilder.DropColumn(
                name: "upi_id",
                schema: "company",
                table: "company");
        }
    }
}
