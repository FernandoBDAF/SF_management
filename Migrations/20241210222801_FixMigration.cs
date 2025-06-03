using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternalTransactions_Clients_ClientId",
                table: "InternalTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClientId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "InternalTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InternalTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CalculatedAt",
                table: "ClosingManagers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DoneAt",
                table: "ClosingManagers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RakeBruto",
                table: "ClosingManagers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBalance",
                table: "ClosingManagers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRakeDiscounts",
                table: "ClosingManagers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "BankTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ManagerId",
                table: "WalletTransactions",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransactions_ManagerId",
                table: "InternalTransactions",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_ManagerId",
                table: "BankTransactions",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_Managers_ManagerId",
                table: "BankTransactions",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InternalTransactions_Clients_ClientId",
                table: "InternalTransactions",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InternalTransactions_Managers_ManagerId",
                table: "InternalTransactions",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_Managers_ManagerId",
                table: "WalletTransactions",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Managers_ManagerId",
                table: "BankTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalTransactions_Clients_ClientId",
                table: "InternalTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InternalTransactions_Managers_ManagerId",
                table: "InternalTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_Managers_ManagerId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_ManagerId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InternalTransactions_ManagerId",
                table: "InternalTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_ManagerId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "CalculatedAt",
                table: "ClosingManagers");

            migrationBuilder.DropColumn(
                name: "DoneAt",
                table: "ClosingManagers");

            migrationBuilder.DropColumn(
                name: "RakeBruto",
                table: "ClosingManagers");

            migrationBuilder.DropColumn(
                name: "TotalBalance",
                table: "ClosingManagers");

            migrationBuilder.DropColumn(
                name: "TotalRakeDiscounts",
                table: "ClosingManagers");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "BankTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClientId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InternalTransactions_Clients_ClientId",
                table: "InternalTransactions",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
