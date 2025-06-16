using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class adjustWImodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletIdentifiers_AssetWallets_AssetWalletId",
                table: "WalletIdentifiers");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetWalletId",
                table: "WalletIdentifiers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "AssetType",
                table: "WalletIdentifiers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "BankId",
                table: "WalletIdentifiers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletIdentifiers_BankId",
                table: "WalletIdentifiers",
                column: "BankId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletIdentifiers_AssetWallets_AssetWalletId",
                table: "WalletIdentifiers",
                column: "AssetWalletId",
                principalTable: "AssetWallets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletIdentifiers_Banks_BankId",
                table: "WalletIdentifiers",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletIdentifiers_AssetWallets_AssetWalletId",
                table: "WalletIdentifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletIdentifiers_Banks_BankId",
                table: "WalletIdentifiers");

            migrationBuilder.DropIndex(
                name: "IX_WalletIdentifiers_BankId",
                table: "WalletIdentifiers");

            migrationBuilder.DropColumn(
                name: "AssetType",
                table: "WalletIdentifiers");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "WalletIdentifiers");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssetWalletId",
                table: "WalletIdentifiers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletIdentifiers_AssetWallets_AssetWalletId",
                table: "WalletIdentifiers",
                column: "AssetWalletId",
                principalTable: "AssetWallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
