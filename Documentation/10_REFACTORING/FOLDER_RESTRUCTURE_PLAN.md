# SF Management - Folder Restructure Implementation Plan

> **✅ STATUS: COMPLETED (January 2026)**
> 
> This plan has been successfully executed. The codebase has been reorganized into a Clean Architecture folder structure. This document is preserved as a historical reference of the migration process.

---

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Phase 0: Cleanup Dead Code](#phase-0-cleanup-dead-code)
- [Phase 1: Create New Folder Structure](#phase-1-create-new-folder-structure)
- [Phase 2: Move Domain Layer Files](#phase-2-move-domain-layer-files)
- [Phase 3: Move Application Layer Files](#phase-3-move-application-layer-files)
- [Phase 4: Move Infrastructure Layer Files](#phase-4-move-infrastructure-layer-files)
- [Phase 5: Move API Layer Files](#phase-5-move-api-layer-files)
- [Phase 6: Update Namespaces](#phase-6-update-namespaces)
- [Phase 7: Update References and Imports](#phase-7-update-references-and-imports)
- [Phase 8: Cleanup Old Folders](#phase-8-cleanup-old-folders)
- [Phase 9: Verification](#phase-9-verification)
- [Rollback Plan](#rollback-plan)

---

## Overview

This document provides step-by-step instructions to reorganize the SF Management codebase into a Clean Architecture folder structure within a single project.

**Estimated Time:** 2-4 hours  
**Risk Level:** Medium (requires careful namespace updates)

### What This Plan Covers

| Task | Description |
|------|-------------|
| File Movement | Moving ~120 files to new folder locations |
| Namespace Updates | Updating `namespace` declarations in moved files |
| Import Updates | Updating `using` statements in ALL files that reference moved files |
| Build Verification | Ensuring project compiles after changes |

> **Important:** The import updates (Phase 6.4) are critical. Every file that uses entities, services, or DTOs from the old locations will need its `using` statements updated.

---

## Prerequisites

### 1. Ensure Clean Working Directory

```bash
cd "/Users/fernandobarroso/Local Repo/Sempre Fichas/SF_management"
git status  # Ensure clean working directory
```

### 2. Build Verification

```bash
dotnet build
dotnet test  # If tests exist
```

> **Note:** Branch management is handled externally. This plan assumes you are already on the appropriate branch.

---

## Phase 0: Cleanup Dead Code

### 0.1 Delete Commented-Out Files

```bash
# Delete files that are 100% commented out or unused
rm "Services/TransactionService.cs"
rm "Services/InternalTransactionService.cs"
rm "Controllers/v1/InternalTransactionController.cs"
rm "Controllers/v1/AvgRateController.cs"

# Delete example files (not needed in production)
rm -rf "Examples/"

# Delete empty placeholder
rm "Models/AccountingClosure/README.txt"
rmdir "Models/AccountingClosure"

# Delete redundant exception (BusinessException covers this)
rm "AppException.cs"
```

### 0.2 Commit Cleanup

```bash
git add -A
git commit -m "chore: remove dead code and unused files"
```

---

## Phase 1: Create New Folder Structure

### 1.1 Create Domain Layer Folders

```bash
# Domain root
mkdir -p "Domain/Common"
mkdir -p "Domain/Entities/AssetHolders"
mkdir -p "Domain/Entities/Assets"
mkdir -p "Domain/Entities/Transactions"
mkdir -p "Domain/Entities/Support"
mkdir -p "Domain/Enums/Assets"
mkdir -p "Domain/Enums/ImportedFiles"
mkdir -p "Domain/Enums/Metadata"
mkdir -p "Domain/Exceptions"
mkdir -p "Domain/Interfaces"
```

### 1.2 Create Application Layer Folders

```bash
# Application root
mkdir -p "Application/Common/Extensions"
mkdir -p "Application/Common/Interfaces"
mkdir -p "Application/DTOs/Common"
mkdir -p "Application/DTOs/AssetHolders"
mkdir -p "Application/DTOs/Assets"
mkdir -p "Application/DTOs/CompanyAssets"
mkdir -p "Application/DTOs/Transactions"
mkdir -p "Application/DTOs/ImportedTransactions"
mkdir -p "Application/DTOs/Support"
mkdir -p "Application/DTOs/Statements"
mkdir -p "Application/Mappings"
mkdir -p "Application/Services/Base"
mkdir -p "Application/Services/AssetHolders"
mkdir -p "Application/Services/Assets"
mkdir -p "Application/Services/Transactions"
mkdir -p "Application/Services/Support"
mkdir -p "Application/Services/Domain"
mkdir -p "Application/Services/Validation"
mkdir -p "Application/Validators/AssetHolders"
mkdir -p "Application/Validators/Assets"
mkdir -p "Application/Validators/Transactions"
```

### 1.3 Create Infrastructure Layer Folders

```bash
# Infrastructure root
mkdir -p "Infrastructure/Data/Configurations"
mkdir -p "Infrastructure/Authorization"
mkdir -p "Infrastructure/Logging"
mkdir -p "Infrastructure/Settings"
```

### 1.4 Create API Layer Folders

```bash
# API root
mkdir -p "Api/Controllers/Base"
mkdir -p "Api/Controllers/v1/AssetHolders"
mkdir -p "Api/Controllers/v1/Assets"
mkdir -p "Api/Controllers/v1/Transactions"
mkdir -p "Api/Controllers/v1/Support"
mkdir -p "Api/Middleware"
mkdir -p "Api/Configuration"
```

### 1.5 Commit Structure

```bash
git add -A
git commit -m "chore: create clean architecture folder structure"
```

---

## Phase 2: Move Domain Layer Files

### 2.1 Move Common Domain Files

```bash
# BaseDomain
mv "Models/BaseDomain.cs" "Domain/Common/BaseDomain.cs"
```

### 2.2 Move Entity Files - AssetHolders

```bash
mv "Models/Entities/BaseAssetHolder.cs" "Domain/Entities/AssetHolders/BaseAssetHolder.cs"
mv "Models/Entities/Bank.cs" "Domain/Entities/AssetHolders/Bank.cs"
mv "Models/Entities/Client.cs" "Domain/Entities/AssetHolders/Client.cs"
mv "Models/Entities/Member.cs" "Domain/Entities/AssetHolders/Member.cs"
mv "Models/Entities/PokerManager.cs" "Domain/Entities/AssetHolders/PokerManager.cs"
```

### 2.3 Move Entity Files - Assets

```bash
mv "Models/AssetInfrastructure/AssetPool.cs" "Domain/Entities/Assets/AssetPool.cs"
mv "Models/AssetInfrastructure/WalletIdentifier.cs" "Domain/Entities/Assets/WalletIdentifier.cs"
```

### 2.4 Move Entity Files - Transactions

```bash
mv "Models/Transactions/BaseTransaction.cs" "Domain/Entities/Transactions/BaseTransaction.cs"
mv "Models/Transactions/FiatAssetTransaction.cs" "Domain/Entities/Transactions/FiatAssetTransaction.cs"
mv "Models/Transactions/DigitalAssetTransaction.cs" "Domain/Entities/Transactions/DigitalAssetTransaction.cs"
mv "Models/Transactions/SettlementTransaction.cs" "Domain/Entities/Transactions/SettlementTransaction.cs"
mv "Models/Transactions/ImportedTransaction.cs" "Domain/Entities/Transactions/ImportedTransaction.cs"
```

### 2.5 Move Entity Files - Support

```bash
mv "Models/Support/Address.cs" "Domain/Entities/Support/Address.cs"
mv "Models/Support/Category.cs" "Domain/Entities/Support/Category.cs"
mv "Models/Support/ContactPhone.cs" "Domain/Entities/Support/ContactPhone.cs"
mv "Models/Support/InitialBalance.cs" "Domain/Entities/Support/InitialBalance.cs"
mv "Models/Support/Referral.cs" "Domain/Entities/Support/Referral.cs"
```

### 2.6 Move Enum Files

```bash
# Root-level enums
mv "Enums/AssetHolderType.cs" "Domain/Enums/AssetHolderType.cs"
mv "Enums/TaxEntityType.cs" "Domain/Enums/TaxEntityType.cs"
mv "Enums/ManagerProfitType.cs" "Domain/Enums/ManagerProfitType.cs"

# Asset enums
mv "Enums/AssetInfrastructure/AccountClassification.cs" "Domain/Enums/Assets/AccountClassification.cs"
mv "Enums/AssetInfrastructure/AssetGroup.cs" "Domain/Enums/Assets/AssetGroup.cs"
mv "Enums/AssetInfrastructure/AssetType.cs" "Domain/Enums/Assets/AssetType.cs"

# Imported files enums
mv "Enums/ImportedFiles/ExcelImportType.cs" "Domain/Enums/ImportedFiles/ExcelImportType.cs"
mv "Enums/ImportedFiles/ImportFileType.cs" "Domain/Enums/ImportedFiles/ImportFileType.cs"
mv "Enums/ImportedFiles/ImportedTransactionStatus.cs" "Domain/Enums/ImportedFiles/ImportedTransactionStatus.cs"
mv "Enums/ImportedFiles/ReconciledTransactionType.cs" "Domain/Enums/ImportedFiles/ReconciledTransactionType.cs"

# Metadata enums
mv "Enums/WalletsMetadata/BankWalletMetadata.cs" "Domain/Enums/Metadata/BankWalletMetadata.cs"
mv "Enums/WalletsMetadata/CryptoWalletMetadata.cs" "Domain/Enums/Metadata/CryptoWalletMetadata.cs"
mv "Enums/WalletsMetadata/PokerWalletMetadata.cs" "Domain/Enums/Metadata/PokerWalletMetadata.cs"
```

### 2.7 Move Exception Files

```bash
mv "Exceptions/BusinessException.cs" "Domain/Exceptions/BusinessException.cs"
```

### 2.8 Move Interface Files

```bash
mv "Interfaces/IAssetHolder.cs" "Domain/Interfaces/IAssetHolder.cs"
mv "Interfaces/IAssetHolderDomainService.cs" "Domain/Interfaces/IAssetHolderDomainService.cs"
```

### 2.9 Commit Domain Layer

```bash
git add -A
git commit -m "refactor: move domain layer files to Domain/"
```

---

## Phase 3: Move Application Layer Files

### 3.1 Move Common/Extensions

```bash
# Rename and move EnumHelper
mv "EnumHelper.cs" "Application/Common/Extensions/EnumExtensions.cs"
```

### 3.2 Move DTOs - Common

```bash
mv "ViewModels/BaseResponse.cs" "Application/DTOs/Common/BaseResponse.cs"
mv "ViewModels/TableResponse.cs" "Application/DTOs/Common/TableResponse.cs"
```

### 3.3 Move DTOs - AssetHolders

```bash
mv "ViewModels/BaseAssetHolderRequest.cs" "Application/DTOs/AssetHolders/BaseAssetHolderRequest.cs"
mv "ViewModels/BaseAssetHolderResponse.cs" "Application/DTOs/AssetHolders/BaseAssetHolderResponse.cs"
mv "ViewModels/BankRequest.cs" "Application/DTOs/AssetHolders/BankRequest.cs"
mv "ViewModels/BankResponse.cs" "Application/DTOs/AssetHolders/BankResponse.cs"
mv "ViewModels/ClientRequest.cs" "Application/DTOs/AssetHolders/ClientRequest.cs"
mv "ViewModels/ClientResponse.cs" "Application/DTOs/AssetHolders/ClientResponse.cs"
mv "ViewModels/MemberRequest.cs" "Application/DTOs/AssetHolders/MemberRequest.cs"
mv "ViewModels/MemberResponse.cs" "Application/DTOs/AssetHolders/MemberResponse.cs"
mv "ViewModels/PokerManagerRequest.cs" "Application/DTOs/AssetHolders/PokerManagerRequest.cs"
mv "ViewModels/PokerManagerResponse.cs" "Application/DTOs/AssetHolders/PokerManagerResponse.cs"
```

### 3.4 Move DTOs - Assets

```bash
mv "ViewModels/AssetPoolRequest.cs" "Application/DTOs/Assets/AssetPoolRequest.cs"
mv "ViewModels/AssetPoolResponse.cs" "Application/DTOs/Assets/AssetPoolResponse.cs"
mv "ViewModels/WalletIdentifierRequest.cs" "Application/DTOs/Assets/WalletIdentifierRequest.cs"
mv "ViewModels/WalletIdentifierResponse.cs" "Application/DTOs/Assets/WalletIdentifierResponse.cs"
mv "ViewModels/WalletIdentifiersConnectedResponse.cs" "Application/DTOs/Assets/WalletIdentifiersConnectedResponse.cs"
mv "ViewModels/InitialBalanceRequest.cs" "Application/DTOs/Assets/InitialBalanceRequest.cs"
mv "ViewModels/InitialBalanceResponse.cs" "Application/DTOs/Assets/InitialBalanceResponse.cs"
mv "ViewModels/BalanceRequest.cs" "Application/DTOs/Assets/BalanceRequest.cs"
mv "ViewModels/BalanceResponse.cs" "Application/DTOs/Assets/BalanceResponse.cs"
```

### 3.5 Move DTOs - CompanyAssets

```bash
mv "ViewModels/CompanyAssetPoolRequest.cs" "Application/DTOs/CompanyAssets/CompanyAssetPoolRequest.cs"
mv "ViewModels/CompanyAssetPoolResponse.cs" "Application/DTOs/CompanyAssets/CompanyAssetPoolResponse.cs"
mv "ViewModels/CompanyAssetPoolSummaryResponse.cs" "Application/DTOs/CompanyAssets/CompanyAssetPoolSummaryResponse.cs"
mv "ViewModels/CompanyAssetPoolAnalyticsRequest.cs" "Application/DTOs/CompanyAssets/CompanyAssetPoolAnalyticsRequest.cs"
mv "ViewModels/CompanyAssetPoolAnalyticsResponse.cs" "Application/DTOs/CompanyAssets/CompanyAssetPoolAnalyticsResponse.cs"
```

### 3.6 Move DTOs - Transactions

```bash
mv "ViewModels/BaseTransactionRequest.cs" "Application/DTOs/Transactions/BaseTransactionRequest.cs"
mv "ViewModels/BaseTransactionResponse.cs" "Application/DTOs/Transactions/BaseTransactionResponse.cs"
mv "ViewModels/FiatAssetTransactionRequest.cs" "Application/DTOs/Transactions/FiatAssetTransactionRequest.cs"
mv "ViewModels/FiatAssetTransactionResponse.cs" "Application/DTOs/Transactions/FiatAssetTransactionResponse.cs"
mv "ViewModels/DigitalAssetTransactionRequest.cs" "Application/DTOs/Transactions/DigitalAssetTransactionRequest.cs"
mv "ViewModels/DigitalAssetTransactionResponse.cs" "Application/DTOs/Transactions/DigitalAssetTransactionResponse.cs"
mv "ViewModels/SettlementTransactionRequest.cs" "Application/DTOs/Transactions/SettlementTransactionRequest.cs"
mv "ViewModels/SettlementTransactionResponse.cs" "Application/DTOs/Transactions/SettlementTransactionResponse.cs"
mv "ViewModels/SettlementClosingsResponse.cs" "Application/DTOs/Transactions/SettlementClosingsResponse.cs"
mv "ViewModels/TransactionResponse.cs" "Application/DTOs/Transactions/TransactionResponse.cs"
mv "ViewModels/ProfitResponse.cs" "Application/DTOs/Transactions/ProfitResponse.cs"
mv "ViewModels/BankTransactionApproveRequest.cs" "Application/DTOs/Transactions/BankTransactionApproveRequest.cs"
mv "ViewModels/WalletTransactionApproveRequest.cs" "Application/DTOs/Transactions/WalletTransactionApproveRequest.cs"
mv "ViewModels/StatementTransactionResponse.cs" "Application/DTOs/Transactions/StatementTransactionResponse.cs"
```

### 3.7 Move DTOs - ImportedTransactions

```bash
mv "ViewModels/ImportedTransactionRequest.cs" "Application/DTOs/ImportedTransactions/ImportedTransactionRequest.cs"
mv "ViewModels/ImportedTransactionResponse.cs" "Application/DTOs/ImportedTransactions/ImportedTransactionResponse.cs"
mv "ViewModels/ImportBuySellTransactionsRequest.cs" "Application/DTOs/ImportedTransactions/ImportBuySellTransactionsRequest.cs"
mv "ViewModels/ImportTransferTransactionRequest.cs" "Application/DTOs/ImportedTransactions/ImportTransferTransactionRequest.cs"
```

### 3.8 Move DTOs - Support

```bash
mv "ViewModels/AddressRequest.cs" "Application/DTOs/Support/AddressRequest.cs"
mv "ViewModels/AddressResponse.cs" "Application/DTOs/Support/AddressResponse.cs"
mv "ViewModels/ContactPhoneRequest.cs" "Application/DTOs/Support/ContactPhoneRequest.cs"
mv "ViewModels/ContactPhoneResponse.cs" "Application/DTOs/Support/ContactPhoneResponse.cs"
mv "ViewModels/CategoryRequest.cs" "Application/DTOs/Support/CategoryRequest.cs"
mv "ViewModels/CategoryResponse.cs" "Application/DTOs/Support/CategoryResponse.cs"
```

### 3.9 Move DTOs - Statements

```bash
mv "ViewModels/StatementAssetHolderWithTransactions.cs" "Application/DTOs/Statements/StatementAssetHolderWithTransactions.cs"
```

### 3.10 Move Mappings

```bash
mv "AutoMapperProfile.cs" "Application/Mappings/AutoMapperProfile.cs"
```

### 3.11 Move Services - Base

```bash
mv "Services/BaseService.cs" "Application/Services/Base/BaseService.cs"
mv "Services/BaseAssetHolderService.cs" "Application/Services/Base/BaseAssetHolderService.cs"
mv "Services/BaseTransactionService.cs" "Application/Services/Base/BaseTransactionService.cs"
```

### 3.12 Move Services - AssetHolders

```bash
mv "Services/BankService.cs" "Application/Services/AssetHolders/BankService.cs"
mv "Services/ClientService.cs" "Application/Services/AssetHolders/ClientService.cs"
mv "Services/MemberService.cs" "Application/Services/AssetHolders/MemberService.cs"
mv "Services/PokerManagerService.cs" "Application/Services/AssetHolders/PokerManagerService.cs"
```

### 3.13 Move Services - Assets

```bash
mv "Services/AssetPoolService.cs" "Application/Services/Assets/AssetPoolService.cs"
mv "Services/WalletIdentifierService.cs" "Application/Services/Assets/WalletIdentifierService.cs"
mv "Services/InitialBalanceService.cs" "Application/Services/Assets/InitialBalanceService.cs"
```

### 3.14 Move Services - Transactions

```bash
mv "Services/FiatAssetTransactionService.cs" "Application/Services/Transactions/FiatAssetTransactionService.cs"
mv "Services/DigitalAssetTransactionService.cs" "Application/Services/Transactions/DigitalAssetTransactionService.cs"
mv "Services/SettlementTransactionService.cs" "Application/Services/Transactions/SettlementTransactionService.cs"
mv "Services/ImportedTransactionService.cs" "Application/Services/Transactions/ImportedTransactionService.cs"
```

### 3.15 Move Services - Support

```bash
mv "Services/AddressService.cs" "Application/Services/Support/AddressService.cs"
mv "Services/ContactPhoneService.cs" "Application/Services/Support/ContactPhoneService.cs"
mv "Services/CategoryService.cs" "Application/Services/Support/CategoryService.cs"
mv "Services/ReferralService.cs" "Application/Services/Support/ReferralService.cs"
mv "Services/ClientReferralService.cs" "Application/Services/Support/ClientReferralService.cs"
```

### 3.16 Move Services - Domain

```bash
mv "Services/AssetHolderDomainService.cs" "Application/Services/Domain/AssetHolderDomainService.cs"
```

### 3.17 Move Services - Validation

```bash
mv "Services/AssetPoolValidationService.cs" "Application/Services/Validation/AssetPoolValidationService.cs"
mv "Services/WalletIdentifierValidationService.cs" "Application/Services/Validation/WalletIdentifierValidationService.cs"
```

### 3.18 Move Validators - AssetHolders

```bash
mv "ViewModels/Validators/BankRequestValidator.cs" "Application/Validators/AssetHolders/BankRequestValidator.cs"
mv "ViewModels/Validators/ClientRequestValidator.cs" "Application/Validators/AssetHolders/ClientRequestValidator.cs"
mv "ViewModels/Validators/ManagerValidator.cs" "Application/Validators/AssetHolders/ManagerValidator.cs"
```

### 3.19 Move Validators - Assets

```bash
mv "ViewModels/Validators/WalletValidator.cs" "Application/Validators/Assets/WalletValidator.cs"
```

### 3.20 Move Validators - Transactions

```bash
mv "ViewModels/Validators/BankTransactionRequestValidator.cs" "Application/Validators/Transactions/BankTransactionRequestValidator.cs"
mv "ViewModels/Validators/BankTransactionApproveRequestValidator.cs" "Application/Validators/Transactions/BankTransactionApproveRequestValidator.cs"
mv "ViewModels/Validators/WalletTransactionValidator.cs" "Application/Validators/Transactions/WalletTransactionValidator.cs"
mv "ViewModels/Validators/WalletTransactionApproveRequestValidator.cs" "Application/Validators/Transactions/WalletTransactionApproveRequestValidator.cs"
mv "ViewModels/Validators/FinancialBehaviorRequestValidator.cs" "Application/Validators/Transactions/FinancialBehaviorRequestValidator.cs"
mv "ViewModels/Validators/ImportBuyTransactionsRequestValidator.cs" "Application/Validators/Transactions/ImportBuyTransactionsRequestValidator.cs"
mv "ViewModels/Validators/ClosingWalletRequest.cs" "Application/Validators/Transactions/ClosingWalletRequest.cs"
```

### 3.21 Commit Application Layer

```bash
git add -A
git commit -m "refactor: move application layer files to Application/"
```

---

## Phase 4: Move Infrastructure Layer Files

### 4.1 Move Data Files

```bash
mv "Data/DataContext.cs" "Infrastructure/Data/DataContext.cs"
```

### 4.2 Move Migrations (Keep in Infrastructure/Data)

```bash
mv "Migrations" "Infrastructure/Data/Migrations"
```

### 4.3 Move Authorization Files

```bash
mv "Authorization/Auth0AuthorizationAttributes.cs" "Infrastructure/Authorization/Auth0AuthorizationAttributes.cs"
mv "Authorization/Auth0AuthorizationHandlers.cs" "Infrastructure/Authorization/Auth0AuthorizationHandlers.cs"
mv "Authorization/Auth0UserService.cs" "Infrastructure/Authorization/Auth0UserService.cs"
```

### 4.4 Move Logging Files

```bash
mv "Services/LoggingService.cs" "Infrastructure/Logging/LoggingService.cs"
```

### 4.5 Move Settings Files

```bash
mv "Settings/Auth0Settings.cs" "Infrastructure/Settings/Auth0Settings.cs"
```

### 4.6 Commit Infrastructure Layer

```bash
git add -A
git commit -m "refactor: move infrastructure layer files to Infrastructure/"
```

---

## Phase 5: Move API Layer Files

### 5.1 Move Base Controllers

```bash
mv "Controllers/BaseApiController.cs" "Api/Controllers/Base/BaseApiController.cs"
mv "Controllers/BaseAssetHolderController.cs" "Api/Controllers/Base/BaseAssetHolderController.cs"
```

### 5.2 Move Controllers - AssetHolders

```bash
mv "Controllers/v1/BankController.cs" "Api/Controllers/v1/AssetHolders/BankController.cs"
mv "Controllers/v1/ClientController.cs" "Api/Controllers/v1/AssetHolders/ClientController.cs"
mv "Controllers/v1/MemberController.cs" "Api/Controllers/v1/AssetHolders/MemberController.cs"
mv "Controllers/v1/PokerManagerController.cs" "Api/Controllers/v1/AssetHolders/PokerManagerController.cs"
```

### 5.3 Move Controllers - Assets

```bash
mv "Controllers/v1/AssetPoolController.cs" "Api/Controllers/v1/Assets/AssetPoolController.cs"
mv "Controllers/v1/CompanyAssetPoolController.cs" "Api/Controllers/v1/Assets/CompanyAssetPoolController.cs"
mv "Controllers/v1/WalletIdentifierController.cs" "Api/Controllers/v1/Assets/WalletIdentifierController.cs"
mv "Controllers/v1/InitialBalanceController.cs" "Api/Controllers/v1/Assets/InitialBalanceController.cs"
```

### 5.4 Move Controllers - Transactions

```bash
mv "Controllers/v1/FiatAssetTransactionController.cs" "Api/Controllers/v1/Transactions/FiatAssetTransactionController.cs"
mv "Controllers/v1/DigitalAssetTransactionController.cs" "Api/Controllers/v1/Transactions/DigitalAssetTransactionController.cs"
mv "Controllers/v1/SettlementTransactionController.cs" "Api/Controllers/v1/Transactions/SettlementTransactionController.cs"
mv "Controllers/v1/ImportedTransactionController.cs" "Api/Controllers/v1/Transactions/ImportedTransactionController.cs"
```

### 5.5 Move Controllers - Support

```bash
mv "Controllers/v1/AddressController.cs" "Api/Controllers/v1/Support/AddressController.cs"
mv "Controllers/v1/ContactPhoneController.cs" "Api/Controllers/v1/Support/ContactPhoneController.cs"
mv "Controllers/v1/CategoryController.cs" "Api/Controllers/v1/Support/CategoryController.cs"
```

### 5.6 Move Middleware Files

```bash
mv "ErrorHandlerMiddleware.cs" "Api/Middleware/ErrorHandlerMiddleware.cs"
mv "Middleware/AuthenticationLoggingMiddleware.cs" "Api/Middleware/AuthenticationLoggingMiddleware.cs"
mv "Middleware/RequestResponseLoggingMiddleware.cs" "Api/Middleware/RequestResponseLoggingMiddleware.cs"
```

### 5.7 Move Configuration Files

```bash
mv "StartupConfig/DependencyInjectionExtensions.cs" "Api/Configuration/DependencyInjectionExtensions.cs"
```

### 5.8 Commit API Layer

```bash
git add -A
git commit -m "refactor: move API layer files to Api/"
```

---

## Phase 6: Update Namespaces

### 6.1 Namespace Mapping Table

| Old Namespace | New Namespace |
|---------------|---------------|
| `SFManagement.Models` | `SFManagement.Domain.Common` |
| `SFManagement.Models.Entities` | `SFManagement.Domain.Entities.AssetHolders` |
| `SFManagement.Models.AssetInfrastructure` | `SFManagement.Domain.Entities.Assets` |
| `SFManagement.Models.Transactions` | `SFManagement.Domain.Entities.Transactions` |
| `SFManagement.Models.Support` | `SFManagement.Domain.Entities.Support` |
| `SFManagement.Enums` | `SFManagement.Domain.Enums` |
| `SFManagement.Enums.AssetInfrastructure` | `SFManagement.Domain.Enums.Assets` |
| `SFManagement.Enums.ImportedFiles` | `SFManagement.Domain.Enums.ImportedFiles` |
| `SFManagement.Enums.WalletsMetadata` | `SFManagement.Domain.Enums.Metadata` |
| `SFManagement.Exceptions` | `SFManagement.Domain.Exceptions` |
| `SFManagement.Interfaces` | `SFManagement.Domain.Interfaces` |
| `SFManagement.ViewModels` | `SFManagement.Application.DTOs.*` |
| `SFManagement.ViewModels.Validators` | `SFManagement.Application.Validators.*` |
| `SFManagement.Services` | `SFManagement.Application.Services.*` |
| `SFManagement` (AutoMapperProfile) | `SFManagement.Application.Mappings` |
| `SFManagement` (EnumHelper) | `SFManagement.Application.Common.Extensions` |
| `SFManagement.Data` | `SFManagement.Infrastructure.Data` |
| `SFManagement.Authorization` | `SFManagement.Infrastructure.Authorization` |
| `SFManagement.Settings` | `SFManagement.Infrastructure.Settings` |
| `SFManagement.Controllers` | `SFManagement.Api.Controllers.Base` |
| `SFManagement.Controllers.v1` | `SFManagement.Api.Controllers.v1.*` |
| `SFManagement.Middleware` | `SFManagement.Api.Middleware` |
| `SFManagement.StartupConfig` | `SFManagement.Api.Configuration` |
| `SFManagement` (ErrorHandlerMiddleware) | `SFManagement.Api.Middleware` |

### 6.2 Automated Namespace Update Script

Create a script file `update-namespaces.sh`:

```bash
#!/bin/bash

# Function to update namespace in a file
update_namespace() {
    local file=$1
    local old_ns=$2
    local new_ns=$3
    
    if [[ -f "$file" ]]; then
        sed -i '' "s/namespace $old_ns/namespace $new_ns/g" "$file"
        sed -i '' "s/using $old_ns/using $new_ns/g" "$file"
    fi
}

# Domain Layer
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Models\.Entities;/namespace SFManagement.Domain.Entities.AssetHolders;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Models\.AssetInfrastructure;/namespace SFManagement.Domain.Entities.Assets;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Models\.Transactions;/namespace SFManagement.Domain.Entities.Transactions;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Models\.Support;/namespace SFManagement.Domain.Entities.Support;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Models;/namespace SFManagement.Domain.Common;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Enums\.AssetInfrastructure;/namespace SFManagement.Domain.Enums.Assets;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Enums\.ImportedFiles;/namespace SFManagement.Domain.Enums.ImportedFiles;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Enums\.WalletsMetadata;/namespace SFManagement.Domain.Enums.Metadata;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Enums;/namespace SFManagement.Domain.Enums;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Exceptions;/namespace SFManagement.Domain.Exceptions;/g' {} \;
find "Domain" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Interfaces;/namespace SFManagement.Domain.Interfaces;/g' {} \;

# Application Layer - DTOs
find "Application/DTOs" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels\.Validators;/namespace SFManagement.Application.Validators;/g' {} \;
find "Application/DTOs/Common" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels;/namespace SFManagement.Application.DTOs.Common;/g' {} \;
find "Application/DTOs/AssetHolders" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels;/namespace SFManagement.Application.DTOs.AssetHolders;/g' {} \;
find "Application/DTOs/Assets" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels;/namespace SFManagement.Application.DTOs.Assets;/g' {} \;
find "Application/DTOs/CompanyAssets" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels;/namespace SFManagement.Application.DTOs.CompanyAssets;/g' {} \;
find "Application/DTOs/Transactions" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels;/namespace SFManagement.Application.DTOs.Transactions;/g' {} \;
find "Application/DTOs/ImportedTransactions" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels;/namespace SFManagement.Application.DTOs.ImportedTransactions;/g' {} \;
find "Application/DTOs/Support" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels;/namespace SFManagement.Application.DTOs.Support;/g' {} \;
find "Application/DTOs/Statements" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels;/namespace SFManagement.Application.DTOs.Statements;/g' {} \;

# Application Layer - Services
find "Application/Services" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Services;/namespace SFManagement.Application.Services;/g' {} \;

# Application Layer - Validators
find "Application/Validators/AssetHolders" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels\.Validators;/namespace SFManagement.Application.Validators.AssetHolders;/g' {} \;
find "Application/Validators/Assets" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels\.Validators;/namespace SFManagement.Application.Validators.Assets;/g' {} \;
find "Application/Validators/Transactions" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.ViewModels\.Validators;/namespace SFManagement.Application.Validators.Transactions;/g' {} \;

# Application Layer - Mappings
sed -i '' 's/namespace SFManagement;/namespace SFManagement.Application.Mappings;/g' "Application/Mappings/AutoMapperProfile.cs"

# Application Layer - Extensions
sed -i '' 's/namespace SFManagement;/namespace SFManagement.Application.Common.Extensions;/g' "Application/Common/Extensions/EnumExtensions.cs"

# Infrastructure Layer
find "Infrastructure/Data" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Data;/namespace SFManagement.Infrastructure.Data;/g' {} \;
find "Infrastructure/Authorization" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Authorization;/namespace SFManagement.Infrastructure.Authorization;/g' {} \;
find "Infrastructure/Logging" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Services;/namespace SFManagement.Infrastructure.Logging;/g' {} \;
find "Infrastructure/Settings" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Settings;/namespace SFManagement.Infrastructure.Settings;/g' {} \;

# API Layer
find "Api/Controllers/Base" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Controllers;/namespace SFManagement.Api.Controllers.Base;/g' {} \;
find "Api/Controllers/v1/AssetHolders" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Controllers\.v1;/namespace SFManagement.Api.Controllers.v1.AssetHolders;/g' {} \;
find "Api/Controllers/v1/Assets" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Controllers\.v1;/namespace SFManagement.Api.Controllers.v1.Assets;/g' {} \;
find "Api/Controllers/v1/Transactions" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Controllers\.v1;/namespace SFManagement.Api.Controllers.v1.Transactions;/g' {} \;
find "Api/Controllers/v1/Support" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Controllers\.v1;/namespace SFManagement.Api.Controllers.v1.Support;/g' {} \;
find "Api/Middleware" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.Middleware;/namespace SFManagement.Api.Middleware;/g' {} \;
find "Api/Middleware" -name "*.cs" -exec sed -i '' 's/namespace SFManagement;/namespace SFManagement.Api.Middleware;/g' {} \;
find "Api/Configuration" -name "*.cs" -exec sed -i '' 's/namespace SFManagement\.StartupConfig;/namespace SFManagement.Api.Configuration;/g' {} \;

echo "Namespace updates complete!"
```

### 6.3 Execute Namespace Updates (Moved Files)

```bash
chmod +x update-namespaces.sh
./update-namespaces.sh
```

### 6.4 Update Using Statements Across ALL Files

After updating namespace declarations, we need to update `using` statements in **every file** that references the old namespaces. Run these commands from the project root:

```bash
# ============================================
# DOMAIN LAYER - Update all using statements
# ============================================

# Models.Entities → Domain.Entities.AssetHolders
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Models\.Entities;/using SFManagement.Domain.Entities.AssetHolders;/g' {} \;

# Models.AssetInfrastructure → Domain.Entities.Assets
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Models\.AssetInfrastructure;/using SFManagement.Domain.Entities.Assets;/g' {} \;

# Models.Transactions → Domain.Entities.Transactions
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Models\.Transactions;/using SFManagement.Domain.Entities.Transactions;/g' {} \;

# Models.Support → Domain.Entities.Support
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Models\.Support;/using SFManagement.Domain.Entities.Support;/g' {} \;

# Models → Domain.Common
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Models;/using SFManagement.Domain.Common;/g' {} \;

# Enums.AssetInfrastructure → Domain.Enums.Assets
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Enums\.AssetInfrastructure;/using SFManagement.Domain.Enums.Assets;/g' {} \;

# Enums.ImportedFiles → Domain.Enums.ImportedFiles
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Enums\.ImportedFiles;/using SFManagement.Domain.Enums.ImportedFiles;/g' {} \;

# Enums.WalletsMetadata → Domain.Enums.Metadata
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Enums\.WalletsMetadata;/using SFManagement.Domain.Enums.Metadata;/g' {} \;

# Enums → Domain.Enums
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Enums;/using SFManagement.Domain.Enums;/g' {} \;

# Exceptions → Domain.Exceptions
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Exceptions;/using SFManagement.Domain.Exceptions;/g' {} \;

# Interfaces → Domain.Interfaces
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Interfaces;/using SFManagement.Domain.Interfaces;/g' {} \;

# ============================================
# APPLICATION LAYER - Update all using statements
# ============================================

# ViewModels.Validators → Application.Validators (general)
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.ViewModels\.Validators;/using SFManagement.Application.Validators;/g' {} \;

# ViewModels → Application.DTOs (will need manual refinement for specific subfolders)
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.ViewModels;/using SFManagement.Application.DTOs;/g' {} \;

# Services → Application.Services (general)
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Services;/using SFManagement.Application.Services;/g' {} \;

# ============================================
# INFRASTRUCTURE LAYER - Update all using statements
# ============================================

# Data → Infrastructure.Data
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Data;/using SFManagement.Infrastructure.Data;/g' {} \;

# Authorization → Infrastructure.Authorization
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Authorization;/using SFManagement.Infrastructure.Authorization;/g' {} \;

# Settings → Infrastructure.Settings
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Settings;/using SFManagement.Infrastructure.Settings;/g' {} \;

# ============================================
# API LAYER - Update all using statements
# ============================================

# Controllers → Api.Controllers.Base
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Controllers;/using SFManagement.Api.Controllers.Base;/g' {} \;

# Controllers.v1 → Api.Controllers.v1 (general - will need subfolders added manually)
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Controllers\.v1;/using SFManagement.Api.Controllers.v1;/g' {} \;

# Middleware → Api.Middleware
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.Middleware;/using SFManagement.Api.Middleware;/g' {} \;

# StartupConfig → Api.Configuration
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -exec sed -i '' 's/using SFManagement\.StartupConfig;/using SFManagement.Api.Configuration;/g' {} \;

echo "All using statements updated!"
```

### 6.5 Add Missing Using Statements

Due to the DTOs and Services being split into subfolders, some files may need additional using statements. Run after build to identify missing:

```bash
dotnet build 2>&1 | grep "error CS0246" | sort | uniq
```

Common additions needed (add these at the top of files that need them):

```csharp
// For files using AssetHolder DTOs
using SFManagement.Application.DTOs.AssetHolders;

// For files using Asset DTOs
using SFManagement.Application.DTOs.Assets;

// For files using Transaction DTOs
using SFManagement.Application.DTOs.Transactions;

// For files using Common DTOs (BaseResponse, TableResponse)
using SFManagement.Application.DTOs.Common;

// For files using Support DTOs
using SFManagement.Application.DTOs.Support;

// For AssetHolder services
using SFManagement.Application.Services.AssetHolders;

// For Asset services
using SFManagement.Application.Services.Assets;

// For Transaction services
using SFManagement.Application.Services.Transactions;

// For Base services
using SFManagement.Application.Services.Base;

// For Support services
using SFManagement.Application.Services.Support;

// For Validation services
using SFManagement.Application.Services.Validation;

// For Domain services
using SFManagement.Application.Services.Domain;

// For Logging service
using SFManagement.Infrastructure.Logging;

// For specific validator folders
using SFManagement.Application.Validators.AssetHolders;
using SFManagement.Application.Validators.Assets;
using SFManagement.Application.Validators.Transactions;

// For specific controller folders
using SFManagement.Api.Controllers.v1.AssetHolders;
using SFManagement.Api.Controllers.v1.Assets;
using SFManagement.Api.Controllers.v1.Transactions;
using SFManagement.Api.Controllers.v1.Support;
```

### 6.6 Verify No Old Namespaces Remain

Check that all old namespace references have been replaced:

```bash
echo "=== Checking for remaining old namespaces ==="

# Should return 0 matches for each
echo "Checking SFManagement.Models..."
grep -r "using SFManagement\.Models" --include="*.cs" . | grep -v "/bin/" | grep -v "/obj/" | wc -l

echo "Checking SFManagement.Enums..."
grep -r "using SFManagement\.Enums" --include="*.cs" . | grep -v "/bin/" | grep -v "/obj/" | grep -v "Domain/Enums" | wc -l

echo "Checking SFManagement.ViewModels..."
grep -r "using SFManagement\.ViewModels" --include="*.cs" . | grep -v "/bin/" | grep -v "/obj/" | wc -l

echo "Checking SFManagement.Services..."
grep -r "using SFManagement\.Services" --include="*.cs" . | grep -v "/bin/" | grep -v "/obj/" | grep -v "Application/Services" | wc -l

echo "Checking SFManagement.Data..."
grep -r "using SFManagement\.Data" --include="*.cs" . | grep -v "/bin/" | grep -v "/obj/" | grep -v "Infrastructure/Data" | wc -l

echo "Checking SFManagement.Controllers..."
grep -r "using SFManagement\.Controllers" --include="*.cs" . | grep -v "/bin/" | grep -v "/obj/" | grep -v "Api/Controllers" | wc -l

echo "Checking SFManagement.Authorization..."
grep -r "using SFManagement\.Authorization" --include="*.cs" . | grep -v "/bin/" | grep -v "/obj/" | grep -v "Infrastructure/Authorization" | wc -l

echo "=== All counts should be 0 ==="
```

If any counts are > 0, find and fix those files:

```bash
# Example: find remaining SFManagement.Models references
grep -r "using SFManagement\.Models" --include="*.cs" . | grep -v "/bin/" | grep -v "/obj/"
```

### 6.7 Commit Namespace Changes

```bash
git add -A
git commit -m "refactor: update namespaces to match new folder structure"
```

---

## Phase 7: Fix Remaining Build Errors

After Phase 6, the automated replacements should handle most cases. This phase handles manual fixes for edge cases.

### Key Files Requiring Manual Import Updates

These files have many dependencies and will likely need manual attention:

| File | Why It Needs Updates |
|------|---------------------|
| `Program.cs` | References DI, Data, Middleware, Validators |
| `Api/Configuration/DependencyInjectionExtensions.cs` | References ALL services and entities |
| `Application/Mappings/AutoMapperProfile.cs` | References ALL entities and DTOs |
| `Infrastructure/Data/DataContext.cs` | References ALL entities |
| `Application/Services/Base/BaseService.cs` | References entities, Data |
| `Application/Services/Base/BaseAssetHolderService.cs` | References entities, other services |
| `Application/Services/Base/BaseTransactionService.cs` | References transaction entities |
| All controllers in `Api/Controllers/v1/*/` | Reference base controllers and services |

### 7.1 Build and Identify Errors

```bash
dotnet build 2>&1 | tee build-errors.log
```

### 7.2 Common Issues and Fixes

#### Issue: `Program.cs` needs updated using statements

```csharp
// Program.cs - Required using statements
using SFManagement.Infrastructure.Data;
using SFManagement.Api.Configuration;
using SFManagement.Application.Validators.Transactions;
using SFManagement.Api.Middleware;
using SFManagement.Application.Mappings;
```

#### Issue: `DependencyInjectionExtensions.cs` needs many imports

```csharp
// Api/Configuration/DependencyInjectionExtensions.cs - Required using statements
using SFManagement.Domain.Interfaces;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Support;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Common;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.AssetHolders;
using SFManagement.Application.Services.Assets;
using SFManagement.Application.Services.Transactions;
using SFManagement.Application.Services.Support;
using SFManagement.Application.Services.Domain;
using SFManagement.Application.Services.Validation;
using SFManagement.Infrastructure.Settings;
using SFManagement.Infrastructure.Authorization;
using SFManagement.Infrastructure.Logging;
```

#### Issue: `AutoMapperProfile.cs` needs entity and DTO imports

```csharp
// Application/Mappings/AutoMapperProfile.cs - Required using statements
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Support;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Domain.Enums.Metadata;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.CompanyAssets;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.DTOs.Support;
using SFManagement.Application.DTOs.Common;
```

#### Issue: `DataContext.cs` needs entity imports

```csharp
// Infrastructure/Data/DataContext.cs - Required using statements
using SFManagement.Domain.Common;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Support;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Infrastructure.Logging;
```

#### Issue: Controllers need base controller and service imports

```csharp
// For each controller in Api/Controllers/v1/*/
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.Services.Base;
// Plus specific service imports for that controller's domain
```

### 7.3 Iterative Fix Process

Repeat until build succeeds:

```bash
# Build and capture errors
dotnet build 2>&1 | grep "error CS" | head -20

# Fix the identified issues

# Rebuild
dotnet build
```

### 7.4 Commit Fixes

```bash
git add -A
git commit -m "refactor: fix remaining namespace references"
```

---

## Phase 8: Cleanup Old Folders

### 8.1 Remove Empty Old Folders

```bash
# Remove old folders (only if empty)
rmdir "Models/Entities" 2>/dev/null
rmdir "Models/AssetInfrastructure" 2>/dev/null
rmdir "Models/Transactions" 2>/dev/null
rmdir "Models/Support" 2>/dev/null
rmdir "Models" 2>/dev/null
rmdir "Enums/AssetInfrastructure" 2>/dev/null
rmdir "Enums/ImportedFiles" 2>/dev/null
rmdir "Enums/WalletsMetadata" 2>/dev/null
rmdir "Enums" 2>/dev/null
rmdir "Exceptions" 2>/dev/null
rmdir "Interfaces" 2>/dev/null
rmdir "ViewModels/Validators" 2>/dev/null
rmdir "ViewModels" 2>/dev/null
rmdir "Services" 2>/dev/null
rmdir "Controllers/v1" 2>/dev/null
rmdir "Controllers" 2>/dev/null
rmdir "Middleware" 2>/dev/null
rmdir "Data" 2>/dev/null
rmdir "Authorization" 2>/dev/null
rmdir "Settings" 2>/dev/null
rmdir "StartupConfig" 2>/dev/null
```

### 8.2 Commit Cleanup

```bash
git add -A
git commit -m "chore: remove empty old folders"
```

---

## Phase 9: Verification

### 9.1 Build Verification

```bash
dotnet clean
dotnet restore
dotnet build
```

### 9.2 Check for Missing Files

```bash
# List all .cs files to verify structure
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" | sort
```

### 9.3 Run Application

```bash
dotnet run
```

### 9.4 Test API Endpoints

```bash
# Test health endpoint
curl -X GET http://localhost:5000/health

# Test Swagger
open http://localhost:5000
```

### 9.5 Final Commit

```bash
git add -A
git commit -m "refactor: complete clean architecture folder restructure"
```

> **Note:** Branch merging is handled externally.

---

## Rollback Plan

If issues arise, rollback using standard Git operations:

```bash
# Option 1: Reset to a specific commit before restructure
git reset --hard <commit-hash-before-restructure>

# Option 2: Revert specific commits
git revert <commit-hash>

# Option 3: Discard uncommitted changes
git checkout -- .
```

> **Note:** Manage rollback according to your Git workflow.

---

## Final Folder Structure Summary

```
SFManagement/
├── Domain/                          # Core business logic
│   ├── Common/                      # BaseDomain, IEntity
│   ├── Entities/                    # All entity classes
│   │   ├── AssetHolders/           # Bank, Client, Member, PokerManager
│   │   ├── Assets/                  # AssetPool, WalletIdentifier
│   │   ├── Transactions/           # All transaction types
│   │   └── Support/                 # Address, Category, etc.
│   ├── Enums/                       # All enumerations
│   ├── Exceptions/                  # Business exceptions
│   └── Interfaces/                  # Domain interfaces
│
├── Application/                     # Application business logic
│   ├── Common/                      # Shared utilities
│   │   └── Extensions/             # EnumExtensions
│   ├── DTOs/                        # Data Transfer Objects
│   │   ├── Common/                  # BaseResponse, TableResponse
│   │   ├── AssetHolders/           # Client/Bank/Member DTOs
│   │   ├── Assets/                  # AssetPool/Wallet DTOs
│   │   ├── CompanyAssets/          # Company pool DTOs
│   │   ├── Transactions/           # Transaction DTOs
│   │   ├── ImportedTransactions/   # Import DTOs
│   │   ├── Support/                 # Address/Contact DTOs
│   │   └── Statements/             # Statement DTOs
│   ├── Mappings/                    # AutoMapper profiles
│   ├── Services/                    # Application services
│   │   ├── Base/                    # Base services
│   │   ├── AssetHolders/           # Entity services
│   │   ├── Assets/                  # Asset services
│   │   ├── Transactions/           # Transaction services
│   │   ├── Support/                 # Support services
│   │   ├── Domain/                  # Domain services
│   │   └── Validation/             # Validation services
│   └── Validators/                  # FluentValidation
│
├── Infrastructure/                  # External concerns
│   ├── Data/                        # EF Core
│   │   ├── DataContext.cs
│   │   ├── Configurations/         # Entity configs
│   │   └── Migrations/             # EF migrations
│   ├── Authorization/               # Auth0
│   ├── Logging/                     # Logging service
│   └── Settings/                    # Config classes
│
├── Api/                             # Presentation layer
│   ├── Controllers/
│   │   ├── Base/                    # Base controllers
│   │   └── v1/                      # Versioned controllers
│   │       ├── AssetHolders/
│   │       ├── Assets/
│   │       ├── Transactions/
│   │       └── Support/
│   ├── Middleware/                  # All middleware
│   └── Configuration/               # DI extensions
│
├── Documentation/                   # Keep as-is
├── wwwroot/                         # Keep as-is
├── sql/                             # Keep as-is
├── logs/                            # Keep as-is
├── Program.cs
├── appsettings.json
└── SFManagement.csproj
```

---

## Next Steps After Restructure

1. **Add Service Interfaces** - Create `I*Service.cs` interfaces for dependency injection
2. **Extract Entity Configurations** - Move EF configs from DataContext to separate files
3. **Update Documentation** - Update architecture docs to reflect new structure
4. **Consider Repository Pattern** - Add repository layer if needed

---

## Related Documentation

- [SERVICE_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md)
- [CONTROLLER_LAYER_ARCHITECTURE.md](../02_ARCHITECTURE/CONTROLLER_LAYER_ARCHITECTURE.md)
- [DATABASE_SCHEMA.md](../02_ARCHITECTURE/DATABASE_SCHEMA.md)

