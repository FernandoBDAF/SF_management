using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteIdAndEditorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "Wallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "Wallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "Tags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "Tags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "Ofxs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "Ofxs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "Nicknames",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "Nicknames",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "Managers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "Managers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "Excels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "Excels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "ClosingWallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "ClosingWallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "ClosingNicknames",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "ClosingNicknames",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "ClosingManagers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "ClosingManagers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "BankTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "BankTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteId",
                table: "Banks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "Banks",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "Ofxs");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "Ofxs");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "Nicknames");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "Nicknames");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "Excels");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "Excels");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "ClosingWallets");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "ClosingWallets");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "ClosingNicknames");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "ClosingNicknames");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "ClosingManagers");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "ClosingManagers");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "Banks");
        }
    }
}
