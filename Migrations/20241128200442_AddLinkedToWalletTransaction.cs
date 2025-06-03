using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkedToWalletTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedToId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_LinkedToId",
                table: "WalletTransactions",
                column: "LinkedToId",
                unique: true,
                filter: "[LinkedToId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_WalletTransactions_LinkedToId",
                table: "WalletTransactions",
                column: "LinkedToId",
                principalTable: "WalletTransactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_WalletTransactions_LinkedToId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_LinkedToId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "LinkedToId",
                table: "WalletTransactions");
        }
    }
}
