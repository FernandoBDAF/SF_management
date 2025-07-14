using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class updateinitialbalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BalanceUnit",
                table: "InitialBalances",
                newName: "AssetType");

            migrationBuilder.RenameIndex(
                name: "IX_InitialBalance_BaseAssetHolder_BalanceUnit",
                table: "InitialBalances",
                newName: "IX_InitialBalance_BaseAssetHolder_AssetType");

            migrationBuilder.AddColumn<int>(
                name: "AssetGroup",
                table: "InitialBalances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InitialBalance_BaseAssetHolder_AssetGroup",
                table: "InitialBalances",
                columns: new[] { "BaseAssetHolderId", "AssetGroup" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_InitialBalance_AssetType_AssetGroup_Exclusive",
                table: "InitialBalances",
                sql: "([AssetType] = 0 AND [AssetGroup] <> 0) OR ([AssetType] <> 0 AND [AssetGroup] = 0)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InitialBalance_AssetType_Or_AssetGroup_Required",
                table: "InitialBalances",
                sql: "[AssetType] <> 0 OR [AssetGroup] <> 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InitialBalance_BaseAssetHolder_AssetGroup",
                table: "InitialBalances");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InitialBalance_AssetType_AssetGroup_Exclusive",
                table: "InitialBalances");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InitialBalance_AssetType_Or_AssetGroup_Required",
                table: "InitialBalances");

            migrationBuilder.DropColumn(
                name: "AssetGroup",
                table: "InitialBalances");

            migrationBuilder.RenameColumn(
                name: "AssetType",
                table: "InitialBalances",
                newName: "BalanceUnit");

            migrationBuilder.RenameIndex(
                name: "IX_InitialBalance_BaseAssetHolder_AssetType",
                table: "InitialBalances",
                newName: "IX_InitialBalance_BaseAssetHolder_BalanceUnit");
        }
    }
}
