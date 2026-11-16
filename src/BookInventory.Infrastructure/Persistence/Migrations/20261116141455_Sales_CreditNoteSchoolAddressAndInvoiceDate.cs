using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sales_CreditNoteSchoolAddressAndInvoiceDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "invoice_date",
                schema: "sales",
                table: "credit_note",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "school_address",
                schema: "sales",
                table: "credit_note",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "invoice_date",
                schema: "sales",
                table: "credit_note");

            migrationBuilder.DropColumn(
                name: "school_address",
                schema: "sales",
                table: "credit_note");
        }
    }
}
