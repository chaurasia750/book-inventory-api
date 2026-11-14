using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Item_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "item");

            migrationBuilder.CreateTable(
                name: "item",
                schema: "item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    vendor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    class_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    hsn_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    gst_rate_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    is_stock_managed = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("pk_item", x => x.id);
                    table.CheckConstraint("ck_item_category", "\"category\" IN ('BOOK', 'COPY', 'PEN', 'PENCIL', 'BAG', 'TIE', 'OTHER')");
                    table.CheckConstraint("ck_item_status", "\"status\" IN ('ACTIVE', 'INACTIVE')");
                });

            migrationBuilder.CreateTable(
                name: "item_barcode",
                schema: "item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    barcode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_barcode", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_barcode_item_item_id",
                        column: x => x.item_id,
                        principalSchema: "item",
                        principalTable: "item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_company_id_category",
                schema: "item",
                table: "item",
                columns: new[] { "company_id", "category" });

            migrationBuilder.CreateIndex(
                name: "ix_item_company_id_code",
                schema: "item",
                table: "item",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_item_company_id_status",
                schema: "item",
                table: "item",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_item_company_id_vendor_id",
                schema: "item",
                table: "item",
                columns: new[] { "company_id", "vendor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_item_barcode_company_id_barcode",
                schema: "item",
                table: "item_barcode",
                columns: new[] { "company_id", "barcode" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_item_barcode_item_id",
                schema: "item",
                table: "item_barcode",
                column: "item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_barcode",
                schema: "item");

            migrationBuilder.DropTable(
                name: "item",
                schema: "item");
        }
    }
}
