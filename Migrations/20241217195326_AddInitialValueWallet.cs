using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialValueWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IntialCredits",
                table: "Wallets",
                newName: "IntialCoins");

            migrationBuilder.RenameColumn(
                name: "IntialBalance",
                table: "Wallets",
                newName: "InitialValue");

            migrationBuilder.RenameColumn(
                name: "InitialRate",
                table: "Wallets",
                newName: "InitialExchangeRate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IntialCoins",
                table: "Wallets",
                newName: "IntialCredits");

            migrationBuilder.RenameColumn(
                name: "InitialValue",
                table: "Wallets",
                newName: "IntialBalance");

            migrationBuilder.RenameColumn(
                name: "InitialExchangeRate",
                table: "Wallets",
                newName: "InitialRate");
        }
    }
}
