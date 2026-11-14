using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Iam_RevokedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_user_session_revoke_reason",
                schema: "iam",
                table: "user_session");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_login_log_logout_reason",
                schema: "iam",
                table: "user_login_log");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_session_revoke_reason",
                schema: "iam",
                table: "user_session",
                sql: "\"revoke_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE', 'REVOKED_BY_USER')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_login_log_logout_reason",
                schema: "iam",
                table: "user_login_log",
                sql: "\"logout_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE', 'REVOKED_BY_USER')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_user_session_revoke_reason",
                schema: "iam",
                table: "user_session");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_login_log_logout_reason",
                schema: "iam",
                table: "user_login_log");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_session_revoke_reason",
                schema: "iam",
                table: "user_session",
                sql: "\"revoke_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_login_log_logout_reason",
                schema: "iam",
                table: "user_login_log",
                sql: "\"logout_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE')");
        }
    }
}
