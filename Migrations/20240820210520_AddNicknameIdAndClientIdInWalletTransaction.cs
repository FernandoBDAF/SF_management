using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddNicknameIdAndClientIdInWalletTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_Wallets_WalletId",
                table: "WalletTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WalletTransaction",
                table: "WalletTransaction");

            migrationBuilder.RenameTable(
                name: "WalletTransaction",
                newName: "WalletTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_WalletTransaction_WalletId",
                table: "WalletTransactions",
                newName: "IX_WalletTransactions_WalletId");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NicknameId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_WalletTransactions",
                table: "WalletTransactions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ClientId",
                table: "WalletTransactions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_NicknameId",
                table: "WalletTransactions",
                column: "NicknameId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_Clients_ClientId",
                table: "WalletTransactions",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_Nicknames_NicknameId",
                table: "WalletTransactions",
                column: "NicknameId",
                principalTable: "Nicknames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_Wallets_WalletId",
                table: "WalletTransactions",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_Clients_ClientId",
                table: "WalletTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_Nicknames_NicknameId",
                table: "WalletTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_Wallets_WalletId",
                table: "WalletTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WalletTransactions",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_ClientId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_NicknameId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "NicknameId",
                table: "WalletTransactions");

            migrationBuilder.RenameTable(
                name: "WalletTransactions",
                newName: "WalletTransaction");

            migrationBuilder.RenameIndex(
                name: "IX_WalletTransactions_WalletId",
                table: "WalletTransaction",
                newName: "IX_WalletTransaction_WalletId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WalletTransaction",
                table: "WalletTransaction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_Wallets_WalletId",
                table: "WalletTransaction",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
