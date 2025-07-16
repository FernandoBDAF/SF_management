using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaseAssetHolders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Cnpj = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReferrerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseAssetHolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseAssetHolders_BaseAssetHolders_ReferrerId",
                        column: x => x.ReferrerId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Postcode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetPools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssetGroup = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetPools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetPools_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Banks_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.CheckConstraint("CK_Client_Birthday_NotFuture", "[Birthday] IS NULL OR [Birthday] <= GETDATE()");
                    table.ForeignKey(
                        name: "FK_Clients_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactPhone",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryCode = table.Column<int>(type: "int", nullable: true),
                    AreaCode = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<int>(type: "int", nullable: true),
                    InputPhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SearchFor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactPhone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactPhone_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    ReconciledTransactionType = table.Column<int>(type: "int", nullable: true),
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
                    table.CheckConstraint("CK_ImportedTransaction_ReconciledAt_Logic", "([ReconciledTransactionId] IS NOT NULL AND [ReconciledAt] IS NOT NULL AND [ReconciledTransactionType] IS NOT NULL) OR ([ReconciledTransactionId] IS NULL AND [ReconciledAt] IS NULL AND [ReconciledTransactionType] IS NULL)");
                    table.ForeignKey(
                        name: "FK_ImportedTransactions_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InitialBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AssetType = table.Column<int>(type: "int", nullable: false),
                    ConversionRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    BalanceAs = table.Column<int>(type: "int", nullable: true),
                    AssetGroup = table.Column<int>(type: "int", nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitialBalances", x => x.Id);
                    table.CheckConstraint("CK_InitialBalance_AssetType_AssetGroup_Exclusive", "([AssetType] = 0 AND [AssetGroup] <> 0) OR ([AssetType] <> 0 AND [AssetGroup] = 0)");
                    table.CheckConstraint("CK_InitialBalance_AssetType_Or_AssetGroup_Required", "[AssetType] <> 0 OR [AssetGroup] <> 0");
                    table.CheckConstraint("CK_InitialBalance_ConversionRate_Positive", "[ConversionRate] IS NULL OR [ConversionRate] > 0");
                    table.ForeignKey(
                        name: "FK_InitialBalances_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Share = table.Column<double>(type: "float(5)", precision: 5, scale: 4, nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.CheckConstraint("CK_Member_Birthday_NotFuture", "[Birthday] IS NULL OR [Birthday] <= GETDATE()");
                    table.CheckConstraint("CK_Member_Share_Range", "[Share] >= 0 AND [Share] <= 1");
                    table.ForeignKey(
                        name: "FK_Members_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PokerManagers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManagerType = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PokerManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PokerManagers_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletIdentifiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetPoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountClassification = table.Column<int>(type: "int", nullable: false),
                    AssetType = table.Column<int>(type: "int", nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(2000)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletIdentifiers_AssetPools_AssetPoolId",
                        column: x => x.AssetPoolId,
                        principalTable: "AssetPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DigitalAssetTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BalanceAs = table.Column<int>(type: "int", nullable: true),
                    ConversionRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SenderWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "FiatAssetTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SenderWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentCommission = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ActiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActiveUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.CheckConstraint("CK_Referral_ActiveDates_Logical", "[ActiveFrom] IS NULL OR [ActiveUntil] IS NULL OR [ActiveFrom] <= [ActiveUntil]");
                    table.CheckConstraint("CK_Referral_ParentCommission_Range", "[ParentCommission] IS NULL OR ([ParentCommission] >= 0 AND [ParentCommission] <= 100)");
                    table.ForeignKey(
                        name: "FK_Referrals_BaseAssetHolders_AssetHolderId",
                        column: x => x.AssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Referrals_WalletIdentifiers_WalletIdentifierId",
                        column: x => x.WalletIdentifierId,
                        principalTable: "WalletIdentifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rake = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RakeCommission = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RakeBack = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SenderWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementTransactions", x => x.Id);
                    table.CheckConstraint("CK_SettlementTransaction_AssetAmount_Positive", "[AssetAmount] > 0");
                    table.CheckConstraint("CK_SettlementTransaction_Date_NotFuture", "[Date] <= GETDATE()");
                    table.CheckConstraint("CK_SettlementTransaction_Different_Sender_Receiver", "[SenderWalletIdentifierId] <> [ReceiverWalletIdentifierId]");
                    table.ForeignKey(
                        name: "FK_SettlementTransactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettlementTransactions_WalletIdentifiers_ReceiverWalletIdentifierId",
                        column: x => x.ReceiverWalletIdentifierId,
                        principalTable: "WalletIdentifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementTransactions_WalletIdentifiers_SenderWalletIdentifierId",
                        column: x => x.SenderWalletIdentifierId,
                        principalTable: "WalletIdentifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BaseAssetHolderId",
                table: "Addresses",
                column: "BaseAssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetPool_AssetGroup",
                table: "AssetPools",
                column: "AssetGroup");

            migrationBuilder.CreateIndex(
                name: "IX_AssetPool_BaseAssetHolder_AssetGroup",
                table: "AssetPools",
                columns: new[] { "BaseAssetHolderId", "AssetGroup" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetPool_BaseAssetHolderId",
                table: "AssetPools",
                column: "BaseAssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetPool_DeletedAt",
                table: "AssetPools",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Bank_BaseAssetHolderId",
                table: "Banks",
                column: "BaseAssetHolderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bank_DeletedAt",
                table: "Banks",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "UQ_Bank_Code_Active",
                table: "Banks",
                column: "Code",
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BaseAssetHolder_DeletedAt",
                table: "BaseAssetHolders",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BaseAssetHolder_Name",
                table: "BaseAssetHolders",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BaseAssetHolder_ReferrerId",
                table: "BaseAssetHolders",
                column: "ReferrerId");

            migrationBuilder.CreateIndex(
                name: "UQ_BaseAssetHolder_Cnpj",
                table: "BaseAssetHolders",
                column: "Cnpj",
                unique: true,
                filter: "[Cnpj] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_BaseAssetHolder_Cpf",
                table: "BaseAssetHolders",
                column: "Cpf",
                unique: true,
                filter: "[Cpf] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryId",
                table: "Categories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Client_BaseAssetHolderId",
                table: "Clients",
                column: "BaseAssetHolderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client_DeletedAt",
                table: "Clients",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhone_BaseAssetHolderId",
                table: "ContactPhone",
                column: "BaseAssetHolderId");

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

            migrationBuilder.CreateIndex(
                name: "IX_InitialBalance_BaseAssetHolder_AssetGroup",
                table: "InitialBalances",
                columns: new[] { "BaseAssetHolderId", "AssetGroup" });

            migrationBuilder.CreateIndex(
                name: "IX_InitialBalance_BaseAssetHolder_AssetType",
                table: "InitialBalances",
                columns: new[] { "BaseAssetHolderId", "AssetType" });

            migrationBuilder.CreateIndex(
                name: "IX_InitialBalance_BaseAssetHolderId",
                table: "InitialBalances",
                column: "BaseAssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_InitialBalance_DeletedAt",
                table: "InitialBalances",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Member_BaseAssetHolderId",
                table: "Members",
                column: "BaseAssetHolderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Member_DeletedAt",
                table: "Members",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PokerManager_BaseAssetHolderId",
                table: "PokerManagers",
                column: "BaseAssetHolderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PokerManager_DeletedAt",
                table: "PokerManagers",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_AssetHolderId",
                table: "Referrals",
                column: "AssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_DeletedAt",
                table: "Referrals",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_Wallet_ActivePeriod",
                table: "Referrals",
                columns: new[] { "WalletIdentifierId", "ActiveFrom", "ActiveUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_Referral_WalletIdentifierId",
                table: "Referrals",
                column: "WalletIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_BaseAssetHolderId",
                table: "Referrals",
                column: "BaseAssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementTransaction_Date",
                table: "SettlementTransactions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementTransaction_DeletedAt",
                table: "SettlementTransactions",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementTransaction_Receiver_Date",
                table: "SettlementTransactions",
                columns: new[] { "ReceiverWalletIdentifierId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementTransaction_Sender_Date",
                table: "SettlementTransactions",
                columns: new[] { "SenderWalletIdentifierId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementTransactions_CategoryId",
                table: "SettlementTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletIdentifier_AssetPoolId",
                table: "WalletIdentifiers",
                column: "AssetPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletIdentifier_AssetType",
                table: "WalletIdentifiers",
                column: "AssetType");

            migrationBuilder.CreateIndex(
                name: "IX_WalletIdentifier_DeletedAt",
                table: "WalletIdentifiers",
                column: "DeletedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "ContactPhone");

            migrationBuilder.DropTable(
                name: "DigitalAssetTransactions");

            migrationBuilder.DropTable(
                name: "FiatAssetTransactions");

            migrationBuilder.DropTable(
                name: "ImportedTransactions");

            migrationBuilder.DropTable(
                name: "InitialBalances");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "PokerManagers");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "SettlementTransactions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "WalletIdentifiers");

            migrationBuilder.DropTable(
                name: "AssetPools");

            migrationBuilder.DropTable(
                name: "BaseAssetHolders");
        }
    }
}
