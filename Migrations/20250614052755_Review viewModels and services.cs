using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class ReviewviewModelsandservices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletIdentifiers_Banks_BankId",
                table: "WalletIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_WalletIdentifiers_BankId",
                table: "WalletIdentifiers");

            migrationBuilder.DropColumn(
                name: "InitialBalanceId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "WalletIdentifiers");

            migrationBuilder.DropColumn(
                name: "InitialCoins",
                table: "PokerManagers");

            migrationBuilder.DropColumn(
                name: "InitialExchangeRate",
                table: "PokerManagers");

            migrationBuilder.AddColumn<decimal>(
                name: "InitialAssetAmount",
                table: "Wallets",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SearchFor",
                table: "ContactPhone",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CountryCode",
                table: "ContactPhone",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialAssetAmount",
                table: "Wallets");

            migrationBuilder.AddColumn<Guid>(
                name: "InitialBalanceId",
                table: "Wallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Wallets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BankId",
                table: "WalletIdentifiers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialCoins",
                table: "PokerManagers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialExchangeRate",
                table: "PokerManagers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SearchFor",
                table: "ContactPhone",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CountryCode",
                table: "ContactPhone",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletIdentifiers_BankId",
                table: "WalletIdentifiers",
                column: "BankId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletIdentifiers_Banks_BankId",
                table: "WalletIdentifiers",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id");
        }
    }
}
