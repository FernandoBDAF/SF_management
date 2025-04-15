using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class SwapVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCoinBalance",
                table: "WalletTransactions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "WalletTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCoinBalance",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "WalletTransactions");
        }
    }
}
