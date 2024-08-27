using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddExcel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExcelId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Excels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Excels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Excels_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ExcelId",
                table: "WalletTransactions",
                column: "ExcelId");

            migrationBuilder.CreateIndex(
                name: "IX_Excels_WalletId",
                table: "Excels",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_Excels_ExcelId",
                table: "WalletTransactions",
                column: "ExcelId",
                principalTable: "Excels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_Excels_ExcelId",
                table: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "Excels");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_ExcelId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "ExcelId",
                table: "WalletTransactions");
        }
    }
}
