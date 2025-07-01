using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class makeWIoptionalintransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DigitalAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "DigitalAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FiatAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "FiatAssetTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "WalletIdentifierId",
                table: "FiatAssetTransactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "WalletIdentifierId",
                table: "DigitalAssetTransactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "DigitalAssetTransactions",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FiatAssetTransactions_WalletIdentifiers_WalletIdentifierId",
                table: "FiatAssetTransactions",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id");
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

            migrationBuilder.AlterColumn<Guid>(
                name: "WalletIdentifierId",
                table: "FiatAssetTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "WalletIdentifierId",
                table: "DigitalAssetTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

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
    }
}
