using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddBankIdIntoInternalTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BankId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransactions_BankId",
                table: "InternalTransactions",
                column: "BankId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternalTransactions_Banks_BankId",
                table: "InternalTransactions",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternalTransactions_Banks_BankId",
                table: "InternalTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InternalTransactions_BankId",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "InternalTransactions");
        }
    }
}
