using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Iam_MobileNormalised : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Plan 0013 / R1: a mobile number has one stored form. Values written before the rule keep a leading 91
            // (12 digits) or 0 (11 digits); rewrite them to the 10-digit form. A row whose 10-digit twin already exists is
            // left alone (the unique index would refuse the rewrite) and is reported by the check in the plan instead.
            migrationBuilder.Sql("""
                UPDATE iam.app_user u SET mobile = substr(u.mobile, 3)
                WHERE length(u.mobile) = 12 AND u.mobile LIKE '91%'
                  AND NOT EXISTS (SELECT 1 FROM iam.app_user o WHERE o.mobile = substr(u.mobile, 3));
                UPDATE iam.app_user u SET mobile = substr(u.mobile, 2)
                WHERE length(u.mobile) = 11 AND u.mobile LIKE '0%'
                  AND NOT EXISTS (SELECT 1 FROM iam.app_user o WHERE o.mobile = substr(u.mobile, 2));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data rewrite: the previous spelling of a number is not recoverable, so there is nothing to undo.
        }
    }
}
