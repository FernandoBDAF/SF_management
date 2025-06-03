using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddTagIdINtoInternalTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TagId",
                table: "InternalTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternalTransactions_TagId",
                table: "InternalTransactions",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternalTransactions_Tags_TagId",
                table: "InternalTransactions",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternalTransactions_Tags_TagId",
                table: "InternalTransactions");

            migrationBuilder.DropIndex(
                name: "IX_InternalTransactions_TagId",
                table: "InternalTransactions");

            migrationBuilder.DropColumn(
                name: "TagId",
                table: "InternalTransactions");
        }
    }
}
