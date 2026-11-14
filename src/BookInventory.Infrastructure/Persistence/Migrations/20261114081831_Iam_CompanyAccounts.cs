using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Iam_CompanyAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_user_session_revoke_reason",
                schema: "iam",
                table: "user_session");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_role_history_from_role",
                schema: "iam",
                table: "user_role_history");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_role_history_to_role",
                schema: "iam",
                table: "user_role_history");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_login_log_logout_reason",
                schema: "iam",
                table: "user_login_log");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_login_log_result",
                schema: "iam",
                table: "user_login_log");

            migrationBuilder.DropCheckConstraint(
                name: "ck_app_user_role",
                schema: "iam",
                table: "app_user");

            migrationBuilder.AddColumn<Guid>(
                name: "active_company_id",
                schema: "iam",
                table: "user_session",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "company_id",
                schema: "iam",
                table: "app_user",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_session_active_company_id",
                schema: "iam",
                table: "user_session",
                column: "active_company_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_session_revoke_reason",
                schema: "iam",
                table: "user_session",
                sql: "\"revoke_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE', 'REVOKED_BY_USER', 'COMPANY_DEACTIVATED')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_role_history_from_role",
                schema: "iam",
                table: "user_role_history",
                sql: "\"from_role\" IN ('SUPER_ADMIN', 'SALES_CRM', 'ACCOUNTS', 'MEMBER', 'OWNER', 'STAFF')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_role_history_to_role",
                schema: "iam",
                table: "user_role_history",
                sql: "\"to_role\" IN ('SUPER_ADMIN', 'SALES_CRM', 'ACCOUNTS', 'MEMBER', 'OWNER', 'STAFF')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_login_log_logout_reason",
                schema: "iam",
                table: "user_login_log",
                sql: "\"logout_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE', 'REVOKED_BY_USER', 'COMPANY_DEACTIVATED')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_login_log_result",
                schema: "iam",
                table: "user_login_log",
                sql: "\"result\" IN ('SUCCESS', 'FAILED_BAD_PASSWORD', 'FAILED_UNKNOWN_USER', 'FAILED_LOCKED', 'FAILED_PENDING', 'FAILED_INACTIVE', 'FAILED_SUSPENDED', 'FAILED_REJECTED', 'FAILED_COMPANY_INACTIVE')");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_company_id",
                schema: "iam",
                table: "app_user",
                column: "company_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_app_user_role",
                schema: "iam",
                table: "app_user",
                sql: "\"role\" IN ('SUPER_ADMIN', 'SALES_CRM', 'ACCOUNTS', 'MEMBER', 'OWNER', 'STAFF')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_session_active_company_id",
                schema: "iam",
                table: "user_session");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_session_revoke_reason",
                schema: "iam",
                table: "user_session");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_role_history_from_role",
                schema: "iam",
                table: "user_role_history");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_role_history_to_role",
                schema: "iam",
                table: "user_role_history");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_login_log_logout_reason",
                schema: "iam",
                table: "user_login_log");

            migrationBuilder.DropCheckConstraint(
                name: "ck_user_login_log_result",
                schema: "iam",
                table: "user_login_log");

            migrationBuilder.DropIndex(
                name: "ix_app_user_company_id",
                schema: "iam",
                table: "app_user");

            migrationBuilder.DropCheckConstraint(
                name: "ck_app_user_role",
                schema: "iam",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "active_company_id",
                schema: "iam",
                table: "user_session");

            migrationBuilder.DropColumn(
                name: "company_id",
                schema: "iam",
                table: "app_user");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_session_revoke_reason",
                schema: "iam",
                table: "user_session",
                sql: "\"revoke_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE', 'REVOKED_BY_USER')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_role_history_from_role",
                schema: "iam",
                table: "user_role_history",
                sql: "\"from_role\" IN ('SUPER_ADMIN', 'SALES_CRM', 'ACCOUNTS', 'MEMBER')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_role_history_to_role",
                schema: "iam",
                table: "user_role_history",
                sql: "\"to_role\" IN ('SUPER_ADMIN', 'SALES_CRM', 'ACCOUNTS', 'MEMBER')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_login_log_logout_reason",
                schema: "iam",
                table: "user_login_log",
                sql: "\"logout_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE', 'REVOKED_BY_USER')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_user_login_log_result",
                schema: "iam",
                table: "user_login_log",
                sql: "\"result\" IN ('SUCCESS', 'FAILED_BAD_PASSWORD', 'FAILED_UNKNOWN_USER', 'FAILED_LOCKED', 'FAILED_PENDING', 'FAILED_INACTIVE', 'FAILED_SUSPENDED', 'FAILED_REJECTED')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_app_user_role",
                schema: "iam",
                table: "app_user",
                sql: "\"role\" IN ('SUPER_ADMIN', 'SALES_CRM', 'ACCOUNTS', 'MEMBER')");
        }
    }
}
