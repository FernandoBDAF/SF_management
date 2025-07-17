using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class Sharerange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Member_Share_Range",
                table: "Members");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Member_Share_Range",
                table: "Members",
                sql: "[Share] >= 0 AND [Share] <= 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Member_Share_Range",
                table: "Members");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Member_Share_Range",
                table: "Members",
                sql: "[Share] >= 0 AND [Share] <= 1");
        }
    }
}
