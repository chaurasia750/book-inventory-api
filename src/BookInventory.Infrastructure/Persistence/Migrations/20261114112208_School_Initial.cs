using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class School_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "school");

            migrationBuilder.CreateTable(
                name: "school",
                schema: "school",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    mobile = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    gstin = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    session_type = table.Column<string>(type: "text", nullable: false),
                    sale_type = table.Column<string>(type: "text", nullable: false),
                    is_gst_default = table.Column<bool>(type: "boolean", nullable: false),
                    opening_balance_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("pk_school", x => x.id);
                    table.CheckConstraint("ck_school_sale_type", "\"sale_type\" IN ('SCHOOL_ORDER', 'COUNTER_SALE', 'BOTH')");
                    table.CheckConstraint("ck_school_session_type", "\"session_type\" IN ('APR_MAR', 'JAN_DEC')");
                    table.CheckConstraint("ck_school_status", "\"status\" IN ('ACTIVE', 'INACTIVE')");
                });

            migrationBuilder.CreateTable(
                name: "school_code_counter",
                schema: "school",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_number = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_school_code_counter", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "school_class",
                schema: "school",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name_normalized = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_school_class", x => x.id);
                    table.ForeignKey(
                        name: "fk_school_class_school_school_id",
                        column: x => x.school_id,
                        principalSchema: "school",
                        principalTable: "school",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_school_company_id_code",
                schema: "school",
                table: "school",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_school_company_id_name",
                schema: "school",
                table: "school",
                columns: new[] { "company_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_school_company_id_status",
                schema: "school",
                table: "school",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_school_class_school_id_name_normalized",
                schema: "school",
                table: "school_class",
                columns: new[] { "school_id", "name_normalized" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_school_class_school_id_sort_order",
                schema: "school",
                table: "school_class",
                columns: new[] { "school_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_school_code_counter_company_id",
                schema: "school",
                table: "school_code_counter",
                column: "company_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "school_class",
                schema: "school");

            migrationBuilder.DropTable(
                name: "school_code_counter",
                schema: "school");

            migrationBuilder.DropTable(
                name: "school",
                schema: "school");
        }
    }
}
