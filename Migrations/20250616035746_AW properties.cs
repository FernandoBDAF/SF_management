using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AWproperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DigitalAssetTransactions_AssetWallets_AssetWalletId",
                table: "DigitalAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_DigitalAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "DigitalAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FiatAssetTransactions_AssetWallets_AssetWalletId",
                table: "FiatAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FiatAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "FiatAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalTransactions_AssetWallets_AssetWalletId",
                table: "InternalTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "InternalTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InternalTransactions_AssetWalletId",
                table: "InternalTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InternalTransactions_WalletIdentifierId",
                table: "InternalTransactions");

            migrationBuilder.DropIndex(
                name: "IX_FiatAssetTransactions_AssetWalletId",
                table: "FiatAssetTransactions");

            migrationBuilder.DropIndex(
                name: "IX_DigitalAssetTransactions_AssetWalletId",
                table: "DigitalAssetTransactions");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "DigitalAssetTransactions",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FiatAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "FiatAssetTransactions",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DigitalAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "DigitalAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FiatAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "FiatAssetTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransactions_AssetWalletId",
                table: "InternalTransactions",
                column: "AssetWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransactions_WalletIdentifierId",
                table: "InternalTransactions",
                column: "WalletIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransactions_AssetWalletId",
                table: "FiatAssetTransactions",
                column: "AssetWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransactions_AssetWalletId",
                table: "DigitalAssetTransactions",
                column: "AssetWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalAssetTransactions_AssetWallets_AssetWalletId",
                table: "DigitalAssetTransactions",
                column: "AssetWalletId",
                principalTable: "AssetWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "DigitalAssetTransactions",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FiatAssetTransactions_AssetWallets_AssetWalletId",
                table: "FiatAssetTransactions",
                column: "AssetWalletId",
                principalTable: "AssetWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FiatAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "FiatAssetTransactions",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalTransactions_AssetWallets_AssetWalletId",
                table: "InternalTransactions",
                column: "AssetWalletId",
                principalTable: "AssetWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "InternalTransactions",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
