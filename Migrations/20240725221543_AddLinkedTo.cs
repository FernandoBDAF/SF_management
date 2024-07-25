using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkedTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LinkedToId",
                table: "BankTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_LinkedToId",
                table: "BankTransactions",
                column: "LinkedToId",
                unique: true,
                filter: "[LinkedToId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_BankTransactions_LinkedToId",
                table: "BankTransactions",
                column: "LinkedToId",
                principalTable: "BankTransactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_BankTransactions_LinkedToId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_LinkedToId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "LinkedToId",
                table: "BankTransactions");
        }
    }
}
