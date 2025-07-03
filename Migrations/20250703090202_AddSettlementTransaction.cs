using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlementTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransaction_AssetWallets_AssetWalletId",
                table: "SettlementTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransaction_FinancialBehaviors_FinancialBehaviorId",
                table: "SettlementTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransaction_WalletIdentifiers_WalletIdentifierId",
                table: "SettlementTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementTransaction",
                table: "SettlementTransaction");

            migrationBuilder.RenameTable(
                name: "SettlementTransaction",
                newName: "SettlementTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementTransaction_WalletIdentifierId",
                table: "SettlementTransactions",
                newName: "IX_SettlementTransactions_WalletIdentifierId");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementTransaction_FinancialBehaviorId",
                table: "SettlementTransactions",
                newName: "IX_SettlementTransactions_FinancialBehaviorId");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementTransaction_AssetWalletId",
                table: "SettlementTransactions",
                newName: "IX_SettlementTransactions_AssetWalletId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementTransactions",
                table: "SettlementTransactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransactions_AssetWallets_AssetWalletId",
                table: "SettlementTransactions",
                column: "AssetWalletId",
                principalTable: "AssetWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransactions_FinancialBehaviors_FinancialBehaviorId",
                table: "SettlementTransactions",
                column: "FinancialBehaviorId",
                principalTable: "FinancialBehaviors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "SettlementTransactions",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransactions_AssetWallets_AssetWalletId",
                table: "SettlementTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransactions_FinancialBehaviors_FinancialBehaviorId",
                table: "SettlementTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "SettlementTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementTransactions",
                table: "SettlementTransactions");

            migrationBuilder.RenameTable(
                name: "SettlementTransactions",
                newName: "SettlementTransaction");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementTransactions_WalletIdentifierId",
                table: "SettlementTransaction",
                newName: "IX_SettlementTransaction_WalletIdentifierId");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementTransactions_FinancialBehaviorId",
                table: "SettlementTransaction",
                newName: "IX_SettlementTransaction_FinancialBehaviorId");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementTransactions_AssetWalletId",
                table: "SettlementTransaction",
                newName: "IX_SettlementTransaction_AssetWalletId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementTransaction",
                table: "SettlementTransaction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransaction_AssetWallets_AssetWalletId",
                table: "SettlementTransaction",
                column: "AssetWalletId",
                principalTable: "AssetWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransaction_FinancialBehaviors_FinancialBehaviorId",
                table: "SettlementTransaction",
                column: "FinancialBehaviorId",
                principalTable: "FinancialBehaviors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransaction_WalletIdentifiers_WalletIdentifierId",
                table: "SettlementTransaction",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id");
        }
    }
}
