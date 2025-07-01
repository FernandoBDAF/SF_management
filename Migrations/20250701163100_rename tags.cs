using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class renametags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DigitalAssetTransactions_Tags_TagId",
                table: "DigitalAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FiatAssetTransactions_Tags_TagId",
                table: "FiatAssetTransactions");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.CreateTable(
                name: "FinancialBehaviors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EditorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialBehaviors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialBehaviors_FinancialBehaviors_ParentId",
                        column: x => x.ParentId,
                        principalTable: "FinancialBehaviors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialBehaviors_ParentId",
                table: "FinancialBehaviors",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalAssetTransactions_FinancialBehaviors_TagId",
                table: "DigitalAssetTransactions",
                column: "TagId",
                principalTable: "FinancialBehaviors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FiatAssetTransactions_FinancialBehaviors_TagId",
                table: "FiatAssetTransactions",
                column: "TagId",
                principalTable: "FinancialBehaviors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DigitalAssetTransactions_FinancialBehaviors_TagId",
                table: "DigitalAssetTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FiatAssetTransactions_FinancialBehaviors_TagId",
                table: "FiatAssetTransactions");

            migrationBuilder.DropTable(
                name: "FinancialBehaviors");

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Tags_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Tags",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ParentId",
                table: "Tags",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalAssetTransactions_Tags_TagId",
                table: "DigitalAssetTransactions",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FiatAssetTransactions_Tags_TagId",
                table: "FiatAssetTransactions",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");
        }
    }
}
