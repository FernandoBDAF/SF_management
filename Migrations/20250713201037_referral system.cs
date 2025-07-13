using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFManagement.Migrations
{
    /// <inheritdoc />
    public partial class referralsystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Referral_BaseAssetHolders_AssetHolderId",
                table: "Referral");

            migrationBuilder.DropForeignKey(
                name: "FK_Referral_WalletIdentifiers_WalletIdentifierId",
                table: "Referral");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Referral",
                table: "Referral");

            migrationBuilder.DropIndex(
                name: "IX_Referral_WalletIdentifierId",
                table: "Referral");

            migrationBuilder.RenameTable(
                name: "Referral",
                newName: "Referrals");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SettlementTransactions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FiatAssetTransactions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DigitalAssetTransactions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferrerId",
                table: "BaseAssetHolders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActiveUntil",
                table: "Referrals",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveFrom",
                table: "Referrals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BaseAssetHolderId",
                table: "Referrals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Referrals",
                table: "Referrals",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BaseAssetHolder_ReferrerId",
                table: "BaseAssetHolders",
                column: "ReferrerId");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_Referral_ActiveDates_Logical",
                table: "Referrals",
                sql: "[ActiveFrom] IS NULL OR [ActiveUntil] IS NULL OR [ActiveFrom] <= [ActiveUntil]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Referral_ParentCommission_Range",
                table: "Referrals",
                sql: "[ParentCommission] IS NULL OR ([ParentCommission] >= 0 AND [ParentCommission] <= 100)");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseAssetHolders_BaseAssetHolders_ReferrerId",
                table: "BaseAssetHolders",
                column: "ReferrerId",
                principalTable: "BaseAssetHolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_BaseAssetHolders_AssetHolderId",
                table: "Referrals",
                column: "AssetHolderId",
                principalTable: "BaseAssetHolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_BaseAssetHolders_BaseAssetHolderId",
                table: "Referrals",
                column: "BaseAssetHolderId",
                principalTable: "BaseAssetHolders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_WalletIdentifiers_WalletIdentifierId",
                table: "Referrals",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseAssetHolders_BaseAssetHolders_ReferrerId",
                table: "BaseAssetHolders");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_BaseAssetHolders_AssetHolderId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_BaseAssetHolders_BaseAssetHolderId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_WalletIdentifiers_WalletIdentifierId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_BaseAssetHolder_ReferrerId",
                table: "BaseAssetHolders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Referrals",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referral_DeletedAt",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referral_Wallet_ActivePeriod",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referral_WalletIdentifierId",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_BaseAssetHolderId",
                table: "Referrals");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Referral_ActiveDates_Logical",
                table: "Referrals");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Referral_ParentCommission_Range",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "ReferrerId",
                table: "BaseAssetHolders");

            migrationBuilder.DropColumn(
                name: "ActiveFrom",
                table: "Referrals");

            migrationBuilder.DropColumn(
                name: "BaseAssetHolderId",
                table: "Referrals");

            migrationBuilder.RenameTable(
                name: "Referrals",
                newName: "Referral");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SettlementTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "FiatAssetTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DigitalAssetTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActiveUntil",
                table: "Referral",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Referral",
                table: "Referral",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_WalletIdentifierId",
                table: "Referral",
                column: "WalletIdentifierId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Referral_BaseAssetHolders_AssetHolderId",
                table: "Referral",
                column: "AssetHolderId",
                principalTable: "BaseAssetHolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referral_WalletIdentifiers_WalletIdentifierId",
                table: "Referral",
                column: "WalletIdentifierId",
                principalTable: "WalletIdentifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
