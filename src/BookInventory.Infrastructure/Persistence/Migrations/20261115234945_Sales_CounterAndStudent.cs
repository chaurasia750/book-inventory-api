using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sales_CounterAndStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "counter",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    session = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    has_bills = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("pk_counter", x => x.id);
                    table.CheckConstraint("ck_counter_status", "\"status\" IN ('ACTIVE', 'CLOSED')");
                });

            migrationBuilder.CreateTable(
                name: "student",
                schema: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    name_normalized = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    class_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    section = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    roll_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    parent_mobile = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
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
                    table.PrimaryKey("pk_student", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_counter_company_id_school_id",
                schema: "sales",
                table: "counter",
                columns: new[] { "company_id", "school_id" });

            migrationBuilder.CreateIndex(
                name: "ix_counter_company_id_status",
                schema: "sales",
                table: "counter",
                columns: new[] { "company_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_student_company_id_school_id_name_normalized_class_name",
                schema: "sales",
                table: "student",
                columns: new[] { "company_id", "school_id", "name_normalized", "class_name" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "counter",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "student",
                schema: "sales");
        }
    }
}
