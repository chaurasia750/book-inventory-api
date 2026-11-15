using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Payment_ReceiptCancel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancel_reason",
                schema: "payment",
                table: "receipt",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "cancelled_on",
                schema: "payment",
                table: "receipt",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancel_reason",
                schema: "payment",
                table: "receipt");

            migrationBuilder.DropColumn(
                name: "cancelled_on",
                schema: "payment",
                table: "receipt");
        }
    }
}
