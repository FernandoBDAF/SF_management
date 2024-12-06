using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorIntoBaseDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Wallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Tags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Ofxs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Nicknames",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Managers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Excels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "ClosingWallets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "ClosingNicknames",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "ClosingManagers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "BankTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Banks",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Ofxs");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Nicknames");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Excels");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "ClosingWallets");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "ClosingNicknames");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "ClosingManagers");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Banks");
        }
    }
}
