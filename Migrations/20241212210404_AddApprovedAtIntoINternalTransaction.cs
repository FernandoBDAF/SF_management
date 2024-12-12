using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovedAtIntoINternalTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "InternalTransactions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "InternalTransactions");
        }
    }
}
