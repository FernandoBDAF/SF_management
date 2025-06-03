using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddClosingManager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClosingManagers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosingManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClosingManagers_Managers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Managers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClosingNicknames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NicknameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClosingManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Rake = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Rakeback = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FatherNicknameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FatherPercentual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosingNicknames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClosingNicknames_ClosingManagers_ClosingManagerId",
                        column: x => x.ClosingManagerId,
                        principalTable: "ClosingManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClosingNicknames_Nicknames_NicknameId",
                        column: x => x.NicknameId,
                        principalTable: "Nicknames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ClosingWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClosingManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnRake = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosingWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClosingWallets_ClosingManagers_ClosingManagerId",
                        column: x => x.ClosingManagerId,
                        principalTable: "ClosingManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClosingWallets_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClosingManagers_ManagerId",
                table: "ClosingManagers",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosingNicknames_ClosingManagerId",
                table: "ClosingNicknames",
                column: "ClosingManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosingNicknames_NicknameId",
                table: "ClosingNicknames",
                column: "NicknameId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosingWallets_ClosingManagerId",
                table: "ClosingWallets",
                column: "ClosingManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosingWallets_WalletId",
                table: "ClosingWallets",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClosingNicknames");

            migrationBuilder.DropTable(
                name: "ClosingWallets");

            migrationBuilder.DropTable(
                name: "ClosingManagers");
        }
    }
}
