using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class implementImportedTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransactions_Categories_CategoryId",
                table: "SettlementTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransactions_WalletIdentifiers_ReceiverWalletIdentifierId",
                table: "SettlementTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SettlementTransactions_WalletIdentifiers_SenderWalletIdentifierId",
                table: "SettlementTransactions");

            migrationBuilder.DropTable(
                name: "ExcelTransactions");

            migrationBuilder.DropTable(
                name: "FiatAssetTransactions");

            migrationBuilder.DropTable(
                name: "DigitalAssetTransactions");

            migrationBuilder.DropTable(
                name: "OfxTransactions");

            migrationBuilder.DropTable(
                name: "Excels");

            migrationBuilder.DropTable(
                name: "Ofxs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SettlementTransactions",
                table: "SettlementTransactions");

            migrationBuilder.RenameTable(
                name: "SettlementTransactions",
                newName: "BaseTransaction");

            migrationBuilder.RenameIndex(
                name: "IX_SettlementTransactions_CategoryId",
                table: "BaseTransaction",
                newName: "IX_BaseTransaction_CategoryId");

            migrationBuilder.AlterColumn<decimal>(
                name: "RakeCommission",
                table: "BaseTransaction",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Rake",
                table: "BaseTransaction",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "BalanceAs",
                table: "BaseTransaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionRate",
                table: "BaseTransaction",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "BaseTransaction",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "BaseTransaction",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BaseTransaction",
                table: "BaseTransaction",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ImportedTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExternalReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileType = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    FileMetadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReconciledTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReconciledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReconciliationNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedTransactions", x => x.Id);
                    table.CheckConstraint("CK_ImportedTransaction_Amount_Positive", "[Amount] >= 0");
                    table.CheckConstraint("CK_ImportedTransaction_Date_NotFuture", "[Date] <= GETDATE()");
                    table.CheckConstraint("CK_ImportedTransaction_FileSize_Positive", "[FileSizeBytes] IS NULL OR [FileSizeBytes] > 0");
                    table.CheckConstraint("CK_ImportedTransaction_ProcessedAt_Logic", "([Status] = 3 AND [ProcessedAt] IS NOT NULL) OR ([Status] <> 3 AND [ProcessedAt] IS NULL) OR [Status] = 3");
                    table.CheckConstraint("CK_ImportedTransaction_ReconciledAt_Logic", "([ReconciledTransactionId] IS NOT NULL AND [ReconciledAt] IS NOT NULL) OR ([ReconciledTransactionId] IS NULL AND [ReconciledAt] IS NULL)");
                    table.ForeignKey(
                        name: "FK_ImportedTransactions_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ImportedTransactions_BaseTransaction_ReconciledTransactionId",
                        column: x => x.ReconciledTransactionId,
                        principalTable: "BaseTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaseTransaction_ReceiverWalletIdentifierId",
                table: "BaseTransaction",
                column: "ReceiverWalletIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseTransaction_SenderWalletIdentifierId",
                table: "BaseTransaction",
                column: "SenderWalletIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransaction_Date",
                table: "BaseTransaction",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransaction_DeletedAt",
                table: "BaseTransaction",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransaction_Receiver_Date",
                table: "BaseTransaction",
                columns: new[] { "ReceiverWalletIdentifierId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransaction_Sender_Date",
                table: "BaseTransaction",
                columns: new[] { "SenderWalletIdentifierId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransaction_Date",
                table: "BaseTransaction",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransaction_DeletedAt",
                table: "BaseTransaction",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransaction_Receiver_Date",
                table: "BaseTransaction",
                columns: new[] { "ReceiverWalletIdentifierId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransaction_Sender_Date",
                table: "BaseTransaction",
                columns: new[] { "SenderWalletIdentifierId", "Date" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_DigitalAssetTransaction_AssetAmount_Positive",
                table: "BaseTransaction",
                sql: "[AssetAmount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DigitalAssetTransaction_Date_NotFuture",
                table: "BaseTransaction",
                sql: "[Date] <= GETDATE()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DigitalAssetTransaction_Different_Sender_Receiver",
                table: "BaseTransaction",
                sql: "[SenderWalletIdentifierId] <> [ReceiverWalletIdentifierId]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FiatAssetTransaction_AssetAmount_Positive",
                table: "BaseTransaction",
                sql: "[AssetAmount] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FiatAssetTransaction_Date_NotFuture",
                table: "BaseTransaction",
                sql: "[Date] <= GETDATE()");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FiatAssetTransaction_Different_Sender_Receiver",
                table: "BaseTransaction",
                sql: "[SenderWalletIdentifierId] <> [ReceiverWalletIdentifierId]");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransaction_BaseAssetHolder_FileType_Status",
                table: "ImportedTransactions",
                columns: new[] { "BaseAssetHolderId", "FileType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransaction_BaseAssetHolderId",
                table: "ImportedTransactions",
                column: "BaseAssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransaction_Date_Amount",
                table: "ImportedTransactions",
                columns: new[] { "Date", "Amount" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransaction_DeletedAt",
                table: "ImportedTransactions",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransaction_ExternalReferenceId",
                table: "ImportedTransactions",
                column: "ExternalReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransaction_FileName",
                table: "ImportedTransactions",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransaction_FileType",
                table: "ImportedTransactions",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransaction_ReconciledTransactionId",
                table: "ImportedTransactions",
                column: "ReconciledTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedTransaction_Status",
                table: "ImportedTransactions",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTransaction_Categories_CategoryId",
                table: "BaseTransaction",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTransaction_WalletIdentifiers_ReceiverWalletIdentifierId",
                table: "BaseTransaction",
                column: "ReceiverWalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTransaction_WalletIdentifiers_SenderWalletIdentifierId",
                table: "BaseTransaction",
                column: "SenderWalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseTransaction_Categories_CategoryId",
                table: "BaseTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseTransaction_WalletIdentifiers_ReceiverWalletIdentifierId",
                table: "BaseTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseTransaction_WalletIdentifiers_SenderWalletIdentifierId",
                table: "BaseTransaction");

            migrationBuilder.DropTable(
                name: "ImportedTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BaseTransaction",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BaseTransaction_ReceiverWalletIdentifierId",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_BaseTransaction_SenderWalletIdentifierId",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_DigitalAssetTransaction_Date",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_DigitalAssetTransaction_DeletedAt",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_DigitalAssetTransaction_Receiver_Date",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_DigitalAssetTransaction_Sender_Date",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_FiatAssetTransaction_Date",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_FiatAssetTransaction_DeletedAt",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_FiatAssetTransaction_Receiver_Date",
                table: "BaseTransaction");

            migrationBuilder.DropIndex(
                name: "IX_FiatAssetTransaction_Sender_Date",
                table: "BaseTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DigitalAssetTransaction_AssetAmount_Positive",
                table: "BaseTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DigitalAssetTransaction_Date_NotFuture",
                table: "BaseTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DigitalAssetTransaction_Different_Sender_Receiver",
                table: "BaseTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FiatAssetTransaction_AssetAmount_Positive",
                table: "BaseTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FiatAssetTransaction_Date_NotFuture",
                table: "BaseTransaction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FiatAssetTransaction_Different_Sender_Receiver",
                table: "BaseTransaction");

            migrationBuilder.DropColumn(
                name: "BalanceAs",
                table: "BaseTransaction");

            migrationBuilder.DropColumn(
                name: "ConversionRate",
                table: "BaseTransaction");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "BaseTransaction");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "BaseTransaction");

            migrationBuilder.RenameTable(
                name: "BaseTransaction",
                newName: "SettlementTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_BaseTransaction_CategoryId",
                table: "SettlementTransactions",
                newName: "IX_SettlementTransactions_CategoryId");

            migrationBuilder.AlterColumn<decimal>(
                name: "RakeCommission",
                table: "SettlementTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Rake",
                table: "SettlementTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SettlementTransactions",
                table: "SettlementTransactions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Excels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PokerManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Excels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Excels_PokerManagers_PokerManagerId",
                        column: x => x.PokerManagerId,
                        principalTable: "PokerManagers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ofxs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "DigitalAssetTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiverWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAs = table.Column<int>(type: "int", nullable: true),
                    ConversionRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalAssetTransactions", x => x.Id);
                    table.CheckConstraint("CK_DigitalAssetTransaction_AssetAmount_Positive", "[AssetAmount] > 0");
                    table.CheckConstraint("CK_DigitalAssetTransaction_Date_NotFuture", "[Date] <= GETDATE()");
                    table.CheckConstraint("CK_DigitalAssetTransaction_Different_Sender_Receiver", "[SenderWalletIdentifierId] <> [ReceiverWalletIdentifierId]");
                    table.ForeignKey(
                        name: "FK_DigitalAssetTransactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DigitalAssetTransactions_Excels_ExcelId",
                        column: x => x.ExcelId,
                        principalTable: "Excels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DigitalAssetTransactions_WalletIdentifiers_ReceiverWalletIdentifierId",
                        column: x => x.ReceiverWalletIdentifierId,
                        principalTable: "WalletIdentifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DigitalAssetTransactions_WalletIdentifiers_SenderWalletIdentifierId",
                        column: x => x.SenderWalletIdentifierId,
                        principalTable: "WalletIdentifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfxTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    FitId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfxTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfxTransactions_Ofxs_OfxId",
                        column: x => x.OfxId,
                        principalTable: "Ofxs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExcelTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DigitalAssetTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Coins = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExcelNickname = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ExcelWallet = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcelTransactions_DigitalAssetTransactions_DigitalAssetTransactionId",
                        column: x => x.DigitalAssetTransactionId,
                        principalTable: "DigitalAssetTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExcelTransactions_Excels_ExcelId",
                        column: x => x.ExcelId,
                        principalTable: "Excels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FiatAssetTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OfxTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiverWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiatAssetTransactions", x => x.Id);
                    table.CheckConstraint("CK_FiatAssetTransaction_AssetAmount_Positive", "[AssetAmount] > 0");
                    table.CheckConstraint("CK_FiatAssetTransaction_Date_NotFuture", "[Date] <= GETDATE()");
                    table.CheckConstraint("CK_FiatAssetTransaction_Different_Sender_Receiver", "[SenderWalletIdentifierId] <> [ReceiverWalletIdentifierId]");
                    table.ForeignKey(
                        name: "FK_FiatAssetTransactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FiatAssetTransactions_OfxTransactions_OfxTransactionId",
                        column: x => x.OfxTransactionId,
                        principalTable: "OfxTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FiatAssetTransactions_WalletIdentifiers_ReceiverWalletIdentifierId",
                        column: x => x.ReceiverWalletIdentifierId,
                        principalTable: "WalletIdentifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FiatAssetTransactions_WalletIdentifiers_SenderWalletIdentifierId",
                        column: x => x.SenderWalletIdentifierId,
                        principalTable: "WalletIdentifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransaction_Date",
                table: "DigitalAssetTransactions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransaction_DeletedAt",
                table: "DigitalAssetTransactions",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransaction_Receiver_Date",
                table: "DigitalAssetTransactions",
                columns: new[] { "ReceiverWalletIdentifierId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransaction_Sender_Date",
                table: "DigitalAssetTransactions",
                columns: new[] { "SenderWalletIdentifierId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransactions_CategoryId",
                table: "DigitalAssetTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransactions_ExcelId",
                table: "DigitalAssetTransactions",
                column: "ExcelId");

            migrationBuilder.CreateIndex(
                name: "IX_Excels_PokerManagerId",
                table: "Excels",
                column: "PokerManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelTransactions_DigitalAssetTransactionId",
                table: "ExcelTransactions",
                column: "DigitalAssetTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ExcelTransactions_ExcelId",
                table: "ExcelTransactions",
                column: "ExcelId");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransaction_Date",
                table: "FiatAssetTransactions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransaction_DeletedAt",
                table: "FiatAssetTransactions",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransaction_Receiver_Date",
                table: "FiatAssetTransactions",
                columns: new[] { "ReceiverWalletIdentifierId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransaction_Sender_Date",
                table: "FiatAssetTransactions",
                columns: new[] { "SenderWalletIdentifierId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransactions_CategoryId",
                table: "FiatAssetTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransactions_OfxTransactionId",
                table: "FiatAssetTransactions",
                column: "OfxTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Ofxs_BankId",
                table: "Ofxs",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_OfxTransactions_OfxId",
                table: "OfxTransactions",
                column: "OfxId");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransactions_Categories_CategoryId",
                table: "SettlementTransactions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransactions_WalletIdentifiers_ReceiverWalletIdentifierId",
                table: "SettlementTransactions",
                column: "ReceiverWalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SettlementTransactions_WalletIdentifiers_SenderWalletIdentifierId",
                table: "SettlementTransactions",
                column: "SenderWalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
