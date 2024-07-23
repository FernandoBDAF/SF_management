using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBankTransactionWithClientId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "BankTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportedFromOfxFileAt",
                table: "BankTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tag",
                table: "BankTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TagDescription",
                table: "BankTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_ClientId",
                table: "BankTransactions",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_Clients_ClientId",
                table: "BankTransactions",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Clients_ClientId",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_ClientId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "ImportedFromOfxFileAt",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "Tag",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "TagDescription",
                table: "BankTransactions");
        }
    }
}
