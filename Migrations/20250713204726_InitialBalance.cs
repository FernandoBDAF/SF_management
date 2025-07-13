using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InitialBalances_BaseAssetHolders_BaseAssetHolderId",
                table: "InitialBalances");

            migrationBuilder.RenameIndex(
                name: "IX_InitialBalances_BaseAssetHolderId",
                table: "InitialBalances",
                newName: "IX_InitialBalance_BaseAssetHolderId");

            migrationBuilder.AlterColumn<decimal>(
                name: "ConversionRate",
                table: "InitialBalances",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InitialBalances",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "DigitalAssetTransactions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ConversionRate",
                table: "DigitalAssetTransactions",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InitialBalance_BaseAssetHolder_BalanceUnit",
                table: "InitialBalances",
                columns: new[] { "BaseAssetHolderId", "BalanceUnit" });

            migrationBuilder.CreateIndex(
                name: "IX_InitialBalance_DeletedAt",
                table: "InitialBalances",
                column: "DeletedAt");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InitialBalance_Balance_NotNegative",
                table: "InitialBalances",
                sql: "[Balance] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InitialBalance_ConversionRate_Positive",
                table: "InitialBalances",
                sql: "[ConversionRate] IS NULL OR [ConversionRate] > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_InitialBalances_BaseAssetHolders_BaseAssetHolderId",
                table: "InitialBalances",
                column: "BaseAssetHolderId",
                principalTable: "BaseAssetHolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InitialBalances_BaseAssetHolders_BaseAssetHolderId",
                table: "InitialBalances");

            migrationBuilder.DropIndex(
                name: "IX_InitialBalance_BaseAssetHolder_BalanceUnit",
                table: "InitialBalances");

            migrationBuilder.DropIndex(
                name: "IX_InitialBalance_DeletedAt",
                table: "InitialBalances");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InitialBalance_Balance_NotNegative",
                table: "InitialBalances");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InitialBalance_ConversionRate_Positive",
                table: "InitialBalances");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "InitialBalances");

            migrationBuilder.RenameIndex(
                name: "IX_InitialBalance_BaseAssetHolderId",
                table: "InitialBalances",
                newName: "IX_InitialBalances_BaseAssetHolderId");

            migrationBuilder.AlterColumn<decimal>(
                name: "ConversionRate",
                table: "InitialBalances",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "DigitalAssetTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ConversionRate",
                table: "DigitalAssetTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InitialBalances_BaseAssetHolders_BaseAssetHolderId",
                table: "InitialBalances",
                column: "BaseAssetHolderId",
                principalTable: "BaseAssetHolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
