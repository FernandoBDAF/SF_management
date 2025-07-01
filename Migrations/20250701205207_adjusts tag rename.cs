using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class adjuststagrename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DigitalAssetTransactions_FinancialBehaviors_TagId",
                table: "DigitalAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FiatAssetTransactions_FinancialBehaviors_TagId",
                table: "FiatAssetTransactions");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "FiatAssetTransactions",
                newName: "FinancialBehaviorId");

            migrationBuilder.RenameIndex(
                name: "IX_FiatAssetTransactions_TagId",
                table: "FiatAssetTransactions",
                newName: "IX_FiatAssetTransactions_FinancialBehaviorId");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "DigitalAssetTransactions",
                newName: "FinancialBehaviorId");

            migrationBuilder.RenameIndex(
                name: "IX_DigitalAssetTransactions_TagId",
                table: "DigitalAssetTransactions",
                newName: "IX_DigitalAssetTransactions_FinancialBehaviorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalAssetTransactions_FinancialBehaviors_FinancialBehaviorId",
                table: "DigitalAssetTransactions",
                column: "FinancialBehaviorId",
                principalTable: "FinancialBehaviors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FiatAssetTransactions_FinancialBehaviors_FinancialBehaviorId",
                table: "FiatAssetTransactions",
                column: "FinancialBehaviorId",
                principalTable: "FinancialBehaviors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DigitalAssetTransactions_FinancialBehaviors_FinancialBehaviorId",
                table: "DigitalAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FiatAssetTransactions_FinancialBehaviors_FinancialBehaviorId",
                table: "FiatAssetTransactions");

            migrationBuilder.RenameColumn(
                name: "FinancialBehaviorId",
                table: "FiatAssetTransactions",
                newName: "TagId");

            migrationBuilder.RenameIndex(
                name: "IX_FiatAssetTransactions_FinancialBehaviorId",
                table: "FiatAssetTransactions",
                newName: "IX_FiatAssetTransactions_TagId");

            migrationBuilder.RenameColumn(
                name: "FinancialBehaviorId",
                table: "DigitalAssetTransactions",
                newName: "TagId");

            migrationBuilder.RenameIndex(
                name: "IX_DigitalAssetTransactions_FinancialBehaviorId",
                table: "DigitalAssetTransactions",
                newName: "IX_DigitalAssetTransactions_TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalAssetTransactions_FinancialBehaviors_TagId",
                table: "DigitalAssetTransactions",
                column: "TagId",
                principalTable: "FinancialBehaviors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FiatAssetTransactions_FinancialBehaviors_TagId",
                table: "FiatAssetTransactions",
                column: "TagId",
                principalTable: "FinancialBehaviors",
                principalColumn: "Id");
        }
    }
}
