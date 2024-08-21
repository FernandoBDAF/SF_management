using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRequiredNicknameIdInWalletTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_Nicknames_NicknameId",
                table: "WalletTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "NicknameId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_Nicknames_NicknameId",
                table: "WalletTransactions",
                column: "NicknameId",
                principalTable: "Nicknames",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_Nicknames_NicknameId",
                table: "WalletTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "NicknameId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_Nicknames_NicknameId",
                table: "WalletTransactions",
                column: "NicknameId",
                principalTable: "Nicknames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
