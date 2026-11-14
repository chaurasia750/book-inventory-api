using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Core_CompanyCodeSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_code_series",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
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
                    table.PrimaryKey("pk_company_code_series", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_code_series_company_id_prefix",
                schema: "core",
                table: "company_code_series",
                columns: new[] { "company_id", "prefix" },
                unique: true);

            // Platform P23: the school's own counter becomes the series with prefix SCH, so no school code changes and the next school gets the next number.
            migrationBuilder.Sql(
                """
                insert into core.company_code_series (id, prefix, last_number, is_deleted, created_at, created_by, updated_at, updated_by, company_id)
                select id, 'SCH', last_number, is_deleted, created_at, created_by, updated_at, updated_by, company_id
                from school.school_code_counter;
                """);

            migrationBuilder.DropTable(
                name: "school_code_counter",
                schema: "school");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "school_code_counter",
                schema: "school",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    last_number = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_school_code_counter", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_school_code_counter_company_id",
                schema: "school",
                table: "school_code_counter",
                column: "company_id",
                unique: true);

            migrationBuilder.Sql(
                """
                insert into school.school_code_counter (id, company_id, created_at, created_by, is_deleted, last_number, updated_at, updated_by)
                select id, company_id, created_at, created_by, is_deleted, last_number, updated_at, updated_by
                from core.company_code_series where prefix = 'SCH';
                """);

            migrationBuilder.DropTable(
                name: "company_code_series",
                schema: "core");
        }
    }
}
