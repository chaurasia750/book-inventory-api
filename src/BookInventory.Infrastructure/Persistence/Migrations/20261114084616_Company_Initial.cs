using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Company_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "company");

            migrationBuilder.CreateTable(
                name: "company",
                schema: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    name_normalized = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    gstin = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    pan = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tagline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    status_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company", x => x.id);
                    table.CheckConstraint("ck_company_status", "\"status\" IN ('ACTIVE', 'INACTIVE')");
                });

            migrationBuilder.CreateTable(
                name: "company_status_history",
                schema: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "text", nullable: true),
                    to_status = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_status_history", x => x.id);
                    table.CheckConstraint("ck_company_status_history_from_status", "\"from_status\" IN ('ACTIVE', 'INACTIVE')");
                    table.CheckConstraint("ck_company_status_history_to_status", "\"to_status\" IN ('ACTIVE', 'INACTIVE')");
                    table.ForeignKey(
                        name: "fk_company_status_history_company_company_id",
                        column: x => x.company_id,
                        principalSchema: "company",
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_name_normalized",
                schema: "company",
                table: "company",
                column: "name_normalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_status",
                schema: "company",
                table: "company",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_company_status_history_company_id_changed_at",
                schema: "company",
                table: "company_status_history",
                columns: new[] { "company_id", "changed_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_status_history",
                schema: "company");

            migrationBuilder.DropTable(
                name: "company",
                schema: "company");
        }
    }
}
