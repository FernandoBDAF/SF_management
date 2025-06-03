using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddIsProfitIntoInternalTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClosingManagerId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfit",
                table: "InternalTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransactions_ClosingManagerId",
                table: "InternalTransactions",
                column: "ClosingManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternalTransactions_ClosingManagers_ClosingManagerId",
                table: "InternalTransactions",
                column: "ClosingManagerId",
                principalTable: "ClosingManagers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternalTransactions_ClosingManagers_ClosingManagerId",
                table: "InternalTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InternalTransactions_ClosingManagerId",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "ClosingManagerId",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "IsProfit",
                table: "InternalTransactions");
        }
    }
}
