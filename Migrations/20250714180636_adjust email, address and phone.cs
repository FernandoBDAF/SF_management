using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class adjustemailaddressandphone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_BaseAssetHolder_Email",
                table: "BaseAssetHolders");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_BaseAssetHolderId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "BaseAssetHolders");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Complement",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "StreetAddress",
                table: "Addresses");

            migrationBuilder.RenameColumn(
                name: "LocalCode",
                table: "ContactPhone",
                newName: "AreaCode");

            migrationBuilder.AlterColumn<int>(
                name: "PhoneNumber",
                table: "ContactPhone",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "InputPhoneNumber",
                table: "ContactPhone",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BaseAssetHolderId",
                table: "Addresses",
                column: "BaseAssetHolderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Addresses_BaseAssetHolderId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "InputPhoneNumber",
                table: "ContactPhone");

            migrationBuilder.RenameColumn(
                name: "AreaCode",
                table: "ContactPhone",
                newName: "LocalCode");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "ContactPhone",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "BaseAssetHolders",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Addresses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complement",
                table: "Addresses",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Addresses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Addresses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress",
                table: "Addresses",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_BaseAssetHolder_Email",
                table: "BaseAssetHolders",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BaseAssetHolderId",
                table: "Addresses",
                column: "BaseAssetHolderId",
                unique: true);
        }
    }
}
