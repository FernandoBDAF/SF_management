using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddOfx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransaction_Banks_BankId",
                table: "BankTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankTransaction",
                table: "BankTransaction");

            migrationBuilder.RenameTable(
                name: "BankTransaction",
                newName: "BankTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_BankTransaction_BankId",
                table: "BankTransactions",
                newName: "IX_BankTransactions_BankId");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "BankTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FitId",
                table: "BankTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfxId",
                table: "BankTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankTransactions",
                table: "BankTransactions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Ofxs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ofxs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ofxs_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankTransactions_OfxId",
                table: "BankTransactions",
                column: "OfxId");

            migrationBuilder.CreateIndex(
                name: "IX_Ofxs_BankId",
                table: "Ofxs",
                column: "BankId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_Banks_BankId",
                table: "BankTransactions",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransactions_Ofxs_OfxId",
                table: "BankTransactions",
                column: "OfxId",
                principalTable: "Ofxs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Banks_BankId",
                table: "BankTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_BankTransactions_Ofxs_OfxId",
                table: "BankTransactions");

            migrationBuilder.DropTable(
                name: "Ofxs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BankTransactions",
                table: "BankTransactions");

            migrationBuilder.DropIndex(
                name: "IX_BankTransactions_OfxId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "FitId",
                table: "BankTransactions");

            migrationBuilder.DropColumn(
                name: "OfxId",
                table: "BankTransactions");

            migrationBuilder.RenameTable(
                name: "BankTransactions",
                newName: "BankTransaction");

            migrationBuilder.RenameIndex(
                name: "IX_BankTransactions_BankId",
                table: "BankTransaction",
                newName: "IX_BankTransaction_BankId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BankTransaction",
                table: "BankTransaction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankTransaction_Banks_BankId",
                table: "BankTransaction",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
