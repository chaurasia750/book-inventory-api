using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class School_SessionCutOffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "prefix",
                schema: "core",
                table: "company_code_series",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateTable(
                name: "session_setting",
                schema: "school",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    apr_mar_cut_off_month = table.Column<int>(type: "integer", nullable: false),
                    apr_mar_cut_off_day = table.Column<int>(type: "integer", nullable: false),
                    jan_dec_cut_off_month = table.Column<int>(type: "integer", nullable: false),
                    jan_dec_cut_off_day = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("pk_session_setting", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_session_setting_company_id",
                schema: "school",
                table: "session_setting",
                column: "company_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "session_setting",
                schema: "school");

            migrationBuilder.AlterColumn<string>(
                name: "prefix",
                schema: "core",
                table: "company_code_series",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
