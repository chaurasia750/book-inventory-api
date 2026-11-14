using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Stock_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "stock");

            migrationBuilder.CreateTable(
                name: "item_stock",
                schema: "stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    main_quantity = table.Column<int>(type: "integer", nullable: false),
                    damaged_quantity = table.Column<int>(type: "integer", nullable: false),
                    sample_quantity = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_item_stock", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stock_entry",
                schema: "stock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_stock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stock_type = table.Column<string>(type: "text", nullable: false),
                    movement_kind = table.Column<string>(type: "text", nullable: false),
                    quantity_in = table.Column<int>(type: "integer", nullable: false),
                    quantity_out = table.Column<int>(type: "integer", nullable: false),
                    movement_date = table.Column<DateOnly>(type: "date", nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    document_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_entry", x => x.id);
                    table.CheckConstraint("ck_stock_entry_movement_kind", "\"movement_kind\" IN ('OPENING', 'ADJUSTMENT', 'PURCHASE', 'SCHOOL_INVOICE', 'COUNTER_SALE', 'SCHOOL_RETURN_GOOD', 'SCHOOL_RETURN_DAMAGED', 'VENDOR_RETURN', 'SAMPLE_FROM_VENDOR', 'SAMPLE_TO_SCHOOL', 'SAMPLE_FROM_SCHOOL', 'SAMPLE_TO_VENDOR', 'REVERSAL')");
                    table.CheckConstraint("ck_stock_entry_stock_type", "\"stock_type\" IN ('MAIN', 'DAMAGED', 'SAMPLE')");
                    table.ForeignKey(
                        name: "fk_stock_entry_item_stock_item_stock_id",
                        column: x => x.item_stock_id,
                        principalSchema: "stock",
                        principalTable: "item_stock",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_item_stock_company_id_item_id",
                schema: "stock",
                table: "item_stock",
                columns: new[] { "company_id", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_entry_company_id_item_id_movement_date",
                schema: "stock",
                table: "stock_entry",
                columns: new[] { "company_id", "item_id", "movement_date" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_entry_document_posting",
                schema: "stock",
                table: "stock_entry",
                columns: new[] { "company_id", "document_type", "document_id", "item_id", "stock_type", "movement_kind" },
                unique: true,
                filter: "document_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_stock_entry_item_stock_id",
                schema: "stock",
                table: "stock_entry",
                column: "item_stock_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_entry_opening",
                schema: "stock",
                table: "stock_entry",
                columns: new[] { "company_id", "item_id", "stock_type" },
                unique: true,
                filter: "movement_kind = 'OPENING'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_entry",
                schema: "stock");

            migrationBuilder.DropTable(
                name: "item_stock",
                schema: "stock");
        }
    }
}
