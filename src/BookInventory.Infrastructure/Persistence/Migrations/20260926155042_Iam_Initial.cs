using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Iam_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "iam");

            migrationBuilder.CreateTable(
                name: "app_user",
                schema: "iam",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    mobile = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    status_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status_changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    suspended_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_system_account = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    password_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_user", x => x.id);
                    table.CheckConstraint("ck_app_user_role", "\"role\" IN ('SUPER_ADMIN', 'SALES_CRM', 'ACCOUNTS', 'MEMBER')");
                    table.CheckConstraint("ck_app_user_status", "\"status\" IN ('PENDING', 'ACTIVE', 'INACTIVE', 'SUSPENDED', 'REJECTED')");
                    table.ForeignKey(
                        name: "fk_app_user_app_user_status_changed_by",
                        column: x => x.status_changed_by,
                        principalSchema: "iam",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_token",
                schema: "iam",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_reset_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_password_reset_token_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "iam",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_role_history",
                schema: "iam",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_role = table.Column<string>(type: "text", nullable: false),
                    to_role = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role_history", x => x.id);
                    table.CheckConstraint("ck_user_role_history_from_role", "\"from_role\" IN ('SUPER_ADMIN', 'SALES_CRM', 'ACCOUNTS', 'MEMBER')");
                    table.CheckConstraint("ck_user_role_history_to_role", "\"to_role\" IN ('SUPER_ADMIN', 'SALES_CRM', 'ACCOUNTS', 'MEMBER')");
                    table.ForeignKey(
                        name: "fk_user_role_history_app_user_changed_by",
                        column: x => x.changed_by,
                        principalSchema: "iam",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_role_history_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "iam",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_session",
                schema: "iam",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refresh_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoke_reason = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_session", x => x.id);
                    table.CheckConstraint("ck_user_session_revoke_reason", "\"revoke_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE')");
                    table.ForeignKey(
                        name: "fk_user_session_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "iam",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_status_history",
                schema: "iam",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "text", nullable: false),
                    to_status = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_status_history", x => x.id);
                    table.CheckConstraint("ck_user_status_history_from_status", "\"from_status\" IN ('PENDING', 'ACTIVE', 'INACTIVE', 'SUSPENDED', 'REJECTED')");
                    table.CheckConstraint("ck_user_status_history_to_status", "\"to_status\" IN ('PENDING', 'ACTIVE', 'INACTIVE', 'SUSPENDED', 'REJECTED')");
                    table.ForeignKey(
                        name: "fk_user_status_history_app_user_changed_by",
                        column: x => x.changed_by,
                        principalSchema: "iam",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_status_history_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "iam",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_login_log",
                schema: "iam",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    identifier_attempted = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    result = table.Column<string>(type: "text", nullable: false),
                    login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    logged_out_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    logout_reason = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    browser_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    browser_version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    os_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    os_version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    device_type = table.Column<string>(type: "text", nullable: false),
                    accept_language = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    is_new_device = table.Column<bool>(type: "boolean", nullable: false),
                    is_new_ip = table.Column<bool>(type: "boolean", nullable: false),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_login_log", x => x.id);
                    table.CheckConstraint("ck_user_login_log_device_type", "\"device_type\" IN ('DESKTOP', 'MOBILE', 'TABLET', 'BOT', 'UNKNOWN')");
                    table.CheckConstraint("ck_user_login_log_logout_reason", "\"logout_reason\" IN ('USER_LOGOUT', 'LOGOUT_ALL', 'EXPIRED', 'REVOKED_BY_ADMIN', 'PASSWORD_CHANGED', 'PASSWORD_RESET', 'ACCOUNT_DEACTIVATED', 'ACCOUNT_SUSPENDED', 'FORCED_LOGOUT', 'REFRESH_REUSE')");
                    table.CheckConstraint("ck_user_login_log_result", "\"result\" IN ('SUCCESS', 'FAILED_BAD_PASSWORD', 'FAILED_UNKNOWN_USER', 'FAILED_LOCKED', 'FAILED_PENDING', 'FAILED_INACTIVE', 'FAILED_SUSPENDED', 'FAILED_REJECTED')");
                    table.ForeignKey(
                        name: "fk_user_login_log_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "iam",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_login_log_user_session_session_id",
                        column: x => x.session_id,
                        principalSchema: "iam",
                        principalTable: "user_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_user_email",
                schema: "iam",
                table: "app_user",
                column: "email",
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_last_login_at",
                schema: "iam",
                table: "app_user",
                column: "last_login_at");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_locked_until",
                schema: "iam",
                table: "app_user",
                column: "locked_until");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_mobile",
                schema: "iam",
                table: "app_user",
                column: "mobile",
                unique: true,
                filter: "mobile IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_role",
                schema: "iam",
                table: "app_user",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_status",
                schema: "iam",
                table: "app_user",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_status_changed_by",
                schema: "iam",
                table: "app_user",
                column: "status_changed_by");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_user_name",
                schema: "iam",
                table: "app_user",
                column: "user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_app_user_single_system_account",
                schema: "iam",
                table: "app_user",
                column: "is_system_account",
                unique: true,
                filter: "is_system_account");

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_token_expires_at",
                schema: "iam",
                table: "password_reset_token",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_token_token_hash",
                schema: "iam",
                table: "password_reset_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_token_user_id",
                schema: "iam",
                table: "password_reset_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_login_log_ip_address_login_at",
                schema: "iam",
                table: "user_login_log",
                columns: new[] { "ip_address", "login_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_login_log_result_login_at",
                schema: "iam",
                table: "user_login_log",
                columns: new[] { "result", "login_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_login_log_session_id_login_at",
                schema: "iam",
                table: "user_login_log",
                columns: new[] { "session_id", "login_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_login_log_user_id_login_at",
                schema: "iam",
                table: "user_login_log",
                columns: new[] { "user_id", "login_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_role_history_changed_by",
                schema: "iam",
                table: "user_role_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_history_user_id_changed_at",
                schema: "iam",
                table: "user_role_history",
                columns: new[] { "user_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_user_session_expires_at",
                schema: "iam",
                table: "user_session",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_session_refresh_token_hash",
                schema: "iam",
                table: "user_session",
                column: "refresh_token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_session_revoked_at",
                schema: "iam",
                table: "user_session",
                column: "revoked_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_session_user_id",
                schema: "iam",
                table: "user_session",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_status_history_changed_by",
                schema: "iam",
                table: "user_status_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "ix_user_status_history_user_id_changed_at",
                schema: "iam",
                table: "user_status_history",
                columns: new[] { "user_id", "changed_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "password_reset_token",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "user_login_log",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "user_role_history",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "user_status_history",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "user_session",
                schema: "iam");

            migrationBuilder.DropTable(
                name: "app_user",
                schema: "iam");
        }
    }
}
