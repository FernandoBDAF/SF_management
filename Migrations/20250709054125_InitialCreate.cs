using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    Email = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Cpf = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Cnpj = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseAssetHolders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinancialBehavior = table.Column<int>(type: "int", nullable: true),
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
                    StreetAddress = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    City = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    State = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Complement = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
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
                name: "AssetWallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetWallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetWallets_BaseAssetHolders_BaseAssetHolderId",
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
                    Code = table.Column<int>(type: "int", nullable: false),
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
                    LocalCode = table.Column<int>(type: "int", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                name: "InitialBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceUnit = table.Column<int>(type: "int", nullable: false),
                    ConversionRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BalanceAs = table.Column<int>(type: "int", nullable: true),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitialBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InitialBalances_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Share = table.Column<double>(type: "float", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
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
                    AssetWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteInfo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IdentifierInfo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    DescriptiveInfo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ExtraInfo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InputForTransactions = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BaseAssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletIdentifiers_AssetWallets_AssetWalletId",
                        column: x => x.AssetWalletId,
                        principalTable: "AssetWallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletIdentifiers_BaseAssetHolders_BaseAssetHolderId",
                        column: x => x.BaseAssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ofxs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                name: "Excels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PokerManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                name: "Referral",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActiveUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentCommission = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referral", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referral_BaseAssetHolders_AssetHolderId",
                        column: x => x.AssetHolderId,
                        principalTable: "BaseAssetHolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Referral_WalletIdentifiers_WalletIdentifierId",
                        column: x => x.WalletIdentifierId,
                        principalTable: "WalletIdentifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementTransactions", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "OfxTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    TransactionDirection = table.Column<int>(type: "int", nullable: false),
                    FitId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OfxId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                name: "DigitalAssetTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BalanceAs = table.Column<int>(type: "int", nullable: true),
                    ConversionRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ExcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SenderWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalAssetTransactions", x => x.Id);
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
                name: "FiatAssetTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfxTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SenderWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverWalletIdentifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiatAssetTransactions", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "ExcelTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Coins = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionDirection = table.Column<int>(type: "int", nullable: false),
                    ExcelNickname = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ExcelWallet = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ExcelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DigitalAssetTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_BaseAssetHolderId",
                table: "Addresses",
                column: "BaseAssetHolderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetWallets_BaseAssetHolderId",
                table: "AssetWallets",
                column: "BaseAssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_BaseAssetHolderId",
                table: "Banks",
                column: "BaseAssetHolderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryId",
                table: "Categories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_BaseAssetHolderId",
                table: "Clients",
                column: "BaseAssetHolderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactPhone_BaseAssetHolderId",
                table: "ContactPhone",
                column: "BaseAssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransactions_CategoryId",
                table: "DigitalAssetTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransactions_ExcelId",
                table: "DigitalAssetTransactions",
                column: "ExcelId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransactions_ReceiverWalletIdentifierId",
                table: "DigitalAssetTransactions",
                column: "ReceiverWalletIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAssetTransactions_SenderWalletIdentifierId",
                table: "DigitalAssetTransactions",
                column: "SenderWalletIdentifierId");

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
                name: "IX_FiatAssetTransactions_CategoryId",
                table: "FiatAssetTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransactions_OfxTransactionId",
                table: "FiatAssetTransactions",
                column: "OfxTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransactions_ReceiverWalletIdentifierId",
                table: "FiatAssetTransactions",
                column: "ReceiverWalletIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_FiatAssetTransactions_SenderWalletIdentifierId",
                table: "FiatAssetTransactions",
                column: "SenderWalletIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_InitialBalances_BaseAssetHolderId",
                table: "InitialBalances",
                column: "BaseAssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_BaseAssetHolderId",
                table: "Members",
                column: "BaseAssetHolderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ofxs_BankId",
                table: "Ofxs",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_OfxTransactions_OfxId",
                table: "OfxTransactions",
                column: "OfxId");

            migrationBuilder.CreateIndex(
                name: "IX_PokerManagers_BaseAssetHolderId",
                table: "PokerManagers",
                column: "BaseAssetHolderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Referral_AssetHolderId",
                table: "Referral",
                column: "AssetHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_WalletIdentifierId",
                table: "Referral",
                column: "WalletIdentifierId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementTransactions_CategoryId",
                table: "SettlementTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementTransactions_ReceiverWalletIdentifierId",
                table: "SettlementTransactions",
                column: "ReceiverWalletIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementTransactions_SenderWalletIdentifierId",
                table: "SettlementTransactions",
                column: "SenderWalletIdentifierId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletIdentifiers_AssetWalletId",
                table: "WalletIdentifiers",
                column: "AssetWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletIdentifiers_BaseAssetHolderId",
                table: "WalletIdentifiers",
                column: "BaseAssetHolderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "ContactPhone");

            migrationBuilder.DropTable(
                name: "ExcelTransactions");

            migrationBuilder.DropTable(
                name: "FiatAssetTransactions");

            migrationBuilder.DropTable(
                name: "InitialBalances");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Referral");

            migrationBuilder.DropTable(
                name: "SettlementTransactions");

            migrationBuilder.DropTable(
                name: "DigitalAssetTransactions");

            migrationBuilder.DropTable(
                name: "OfxTransactions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Excels");

            migrationBuilder.DropTable(
                name: "WalletIdentifiers");

            migrationBuilder.DropTable(
                name: "Ofxs");

            migrationBuilder.DropTable(
                name: "PokerManagers");

            migrationBuilder.DropTable(
                name: "AssetWallets");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "BaseAssetHolders");
        }
    }
}
