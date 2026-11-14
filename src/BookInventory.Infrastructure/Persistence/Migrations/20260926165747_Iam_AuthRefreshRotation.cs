using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Iam_AuthRefreshRotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_remember_me",
                schema: "iam",
                table: "user_session",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "previous_refresh_token_hash",
                schema: "iam",
                table: "user_session",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_session_previous_refresh_token_hash",
                schema: "iam",
                table: "user_session",
                column: "previous_refresh_token_hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_session_previous_refresh_token_hash",
                schema: "iam",
                table: "user_session");

            migrationBuilder.DropColumn(
                name: "is_remember_me",
                schema: "iam",
                table: "user_session");

            migrationBuilder.DropColumn(
                name: "previous_refresh_token_hash",
                schema: "iam",
                table: "user_session");
        }
    }
}
