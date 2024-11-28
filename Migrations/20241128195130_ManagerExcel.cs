using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class ManagerExcel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Excels_Wallets_WalletId",
                table: "Excels");

            migrationBuilder.RenameColumn(
                name: "WalletId",
                table: "Excels",
                newName: "ManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_Excels_WalletId",
                table: "Excels",
                newName: "IX_Excels_ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Excels_Managers_ManagerId",
                table: "Excels",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Excels_Managers_ManagerId",
                table: "Excels");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Excels",
                newName: "WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Excels_ManagerId",
                table: "Excels",
                newName: "IX_Excels_WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_Excels_Wallets_WalletId",
                table: "Excels",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
