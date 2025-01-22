using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerIntoAvgRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverateRate",
                table: "WalletTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "AvgRates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AvgRates_ManagerId",
                table: "AvgRates",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AvgRates_Managers_ManagerId",
                table: "AvgRates",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AvgRates_Managers_ManagerId",
                table: "AvgRates");

            migrationBuilder.DropIndex(
                name: "IX_AvgRates_ManagerId",
                table: "AvgRates");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "AvgRates");

            migrationBuilder.AddColumn<decimal>(
                name: "AverateRate",
                table: "WalletTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
