using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddTagIntoWalletAndBankTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tag",
                table: "BankTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "TagId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TagId",
                table: "BankTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_TagId",
                table: "WalletTransactions",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_TagId",
                table: "BankTransactions",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_Tags_TagId",
                table: "BankTransactions",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_Tags_TagId",
                table: "WalletTransactions",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Tags_TagId",
                table: "BankTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_Tags_TagId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_TagId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_TagId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "TagId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "TagId",
                table: "BankTransactions");

            migrationBuilder.AddColumn<int>(
                name: "Tag",
                table: "BankTransactions",
                type: "int",
                nullable: true);
        }
    }
}
