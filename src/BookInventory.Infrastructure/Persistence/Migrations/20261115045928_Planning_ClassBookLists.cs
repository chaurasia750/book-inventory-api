using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Planning_ClassBookLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "planning");

            migrationBuilder.CreateTable(
                name: "class_book_list",
                schema: "planning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    class_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    student_estimate = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("pk_class_book_list", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "class_book_list_line",
                schema: "planning",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_book_list_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    item_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    quantity_per_student = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_class_book_list_line", x => x.id);
                    table.ForeignKey(
                        name: "fk_class_book_list_line_class_book_list_class_book_list_id",
                        column: x => x.class_book_list_id,
                        principalSchema: "planning",
                        principalTable: "class_book_list",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_class_book_list_company_id_school_id_session_class_name",
                schema: "planning",
                table: "class_book_list",
                columns: new[] { "company_id", "school_id", "session", "class_name" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_class_book_list_line_class_book_list_id_sort_order",
                schema: "planning",
                table: "class_book_list_line",
                columns: new[] { "class_book_list_id", "sort_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "class_book_list_line",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "class_book_list",
                schema: "planning");
        }
    }
}
