using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletIdIntoInternalTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WalletId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransactions_WalletId",
                table: "InternalTransactions",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternalTransactions_Wallets_WalletId",
                table: "InternalTransactions",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternalTransactions_Wallets_WalletId",
                table: "InternalTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InternalTransactions_WalletId",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "WalletId",
                table: "InternalTransactions");
        }
    }
}
