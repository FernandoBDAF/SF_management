# Database Schema

## Overview

The SF Management system uses SQL Server with Entity Framework Core for data persistence. The schema follows a normalized design with a central `BaseAssetHolders` table that serves as the foundation for all asset-holding entities.

---

## Entity Relationship Diagram

```
┌──────────────────────────────────────────────────────────────────────────┐
│                           CORE ENTITIES                                   │
└──────────────────────────────────────────────────────────────────────────┘

┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│     Clients     │     │      Banks      │     │     Members     │
│─────────────────│     │─────────────────│     │─────────────────│
│ BaseAssetHolder │────▶│ BaseAssetHolder │────▶│ BaseAssetHolder │
│     Id (FK)     │     │     Id (FK)     │     │     Id (FK)     │
│   BirthDate     │     │                 │     │     Share       │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 ▼
                    ┌─────────────────────────┐
                    │    BaseAssetHolders     │
                    │─────────────────────────│
                    │ Id (PK)                 │
                    │ Name                    │
                    │ GovernmentNumber        │
                    │ TaxEntityType           │
                    │ ReferrerId (FK, self)   │
                    └─────────────────────────┘
                                 │
         ┌───────────┬───────────┼───────────┬───────────┐
         ▼           ▼           ▼           ▼           ▼
    AssetPools   Addresses  ContactPhones InitialBalances Referrals

┌──────────────────────────────────────────────────────────────────────────┐
│                         ASSET INFRASTRUCTURE                              │
└──────────────────────────────────────────────────────────────────────────┘

    BaseAssetHolders
          │
          ▼
    ┌─────────────────┐         ┌─────────────────────┐
    │   AssetPools    │         │  WalletIdentifiers  │
    │─────────────────│    1:N  │─────────────────────│
    │ Id (PK)         │────────▶│ Id (PK)             │
    │ BaseAssetHolder │         │ AssetPoolId (FK)    │
    │   Id (FK, null) │         │ AssetType           │
    │ AssetGroup      │         │ AssetGroup          │
    │ Description     │         │ AccountClassification│
    └─────────────────┘         │ Metadata (JSON)     │
                                └─────────────────────┘
                                         │
                                         ▼
                                    Referrals
                                    Transactions

┌──────────────────────────────────────────────────────────────────────────┐
│                           TRANSACTIONS                                    │
└──────────────────────────────────────────────────────────────────────────┘

                    ┌─────────────────────────┐
                    │    BaseTransaction      │
                    │ (abstract, not in DB)   │
                    │─────────────────────────│
                    │ SenderWalletIdentifierId│
                    │ ReceiverWalletIdentifier│
                    │ AssetAmount             │
                    │ Date                    │
                    │ CategoryId              │
                    └─────────────────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│FiatAssetTransact│    │DigitalAssetTrans│    │SettlementTransact│
│     ions        │    │    actions      │    │     ions        │
│─────────────────│    │─────────────────│    │─────────────────│
│                 │    │ Rate            │    │ RakeAmount      │
│                 │    │ BalanceAs       │    │ SettlementDate  │
│                 │    │ ConversionRate  │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

---

## Core Tables

### BaseAssetHolders

Central table for all entities that can hold assets.

```sql
CREATE TABLE BaseAssetHolders (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    GovernmentNumber NVARCHAR(20),
    TaxEntityType INT NOT NULL,          -- 1=CPF, 2=CNPJ, 3=CNPJ_Not_Taxable
    ReferrerId UNIQUEIDENTIFIER NULL,    -- Self-referencing FK
    
    -- Audit fields
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    DeletedAt DATETIME2,
    CreatedBy NVARCHAR(255),
    LastModifiedBy NVARCHAR(255),
    DeletedBy NVARCHAR(255),
    
    FOREIGN KEY (ReferrerId) REFERENCES BaseAssetHolders(Id)
);
```

### Clients

```sql
CREATE TABLE Clients (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BaseAssetHolderId UNIQUEIDENTIFIER NOT NULL UNIQUE,
    BirthDate DATE,
    
    -- Audit fields inherited from BaseDomain
    
    FOREIGN KEY (BaseAssetHolderId) REFERENCES BaseAssetHolders(Id)
);
```

### Banks, Members, PokerManagers

Similar structure to Clients, with entity-specific fields:

| Table | Specific Fields |
|-------|-----------------|
| Banks | (none additional) |
| Members | Share (decimal) |
| PokerManagers | (none additional) |

---

## Asset Infrastructure Tables

### AssetPools

```sql
CREATE TABLE AssetPools (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BaseAssetHolderId UNIQUEIDENTIFIER NULL,  -- NULL = company-owned
    AssetGroup INT NOT NULL,                   -- 1=FiatAssets, 2=PokerAssets, etc.
    Description NVARCHAR(500),
    
    -- Audit fields
    
    FOREIGN KEY (BaseAssetHolderId) REFERENCES BaseAssetHolders(Id)
);
```

### WalletIdentifiers

```sql
CREATE TABLE WalletIdentifiers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AssetPoolId UNIQUEIDENTIFIER NOT NULL,
    AssetType INT NOT NULL,                    -- 21=BrazilianReal, 101=PokerStars, etc.
    AssetGroup INT NOT NULL,                   -- Denormalized for queries
    AccountClassification INT NOT NULL,        -- 1=ASSET, 2=LIABILITY, etc.
    Metadata NVARCHAR(MAX),                    -- JSON storage
    
    -- Audit fields
    
    FOREIGN KEY (AssetPoolId) REFERENCES AssetPools(Id)
);
```

---

## Transaction Tables

### FiatAssetTransactions

```sql
CREATE TABLE FiatAssetTransactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SenderWalletIdentifierId UNIQUEIDENTIFIER NOT NULL,
    ReceiverWalletIdentifierId UNIQUEIDENTIFIER NOT NULL,
    AssetAmount DECIMAL(18,2) NOT NULL,
    Date DATETIME2 NOT NULL,
    CategoryId UNIQUEIDENTIFIER,
    
    -- Audit fields
    
    FOREIGN KEY (SenderWalletIdentifierId) REFERENCES WalletIdentifiers(Id),
    FOREIGN KEY (ReceiverWalletIdentifierId) REFERENCES WalletIdentifiers(Id),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
```

### DigitalAssetTransactions

```sql
CREATE TABLE DigitalAssetTransactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SenderWalletIdentifierId UNIQUEIDENTIFIER NOT NULL,
    ReceiverWalletIdentifierId UNIQUEIDENTIFIER NOT NULL,
    AssetAmount DECIMAL(18,8) NOT NULL,        -- Higher precision for crypto
    Date DATETIME2 NOT NULL,
    CategoryId UNIQUEIDENTIFIER,
    Rate DECIMAL(18,4),                        -- Transaction rate
    BalanceAs INT,                             -- Convert to AssetType
    ConversionRate DECIMAL(18,8),              -- Conversion factor
    
    -- Audit fields
    
    FOREIGN KEY (SenderWalletIdentifierId) REFERENCES WalletIdentifiers(Id),
    FOREIGN KEY (ReceiverWalletIdentifierId) REFERENCES WalletIdentifiers(Id),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
```

### SettlementTransactions

```sql
CREATE TABLE SettlementTransactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SenderWalletIdentifierId UNIQUEIDENTIFIER NOT NULL,
    ReceiverWalletIdentifierId UNIQUEIDENTIFIER NOT NULL,
    AssetAmount DECIMAL(18,2) NOT NULL,
    Date DATETIME2 NOT NULL,
    CategoryId UNIQUEIDENTIFIER,
    RakeAmount DECIMAL(18,2),
    SettlementDate DATETIME2,
    
    -- Audit fields
    
    FOREIGN KEY (SenderWalletIdentifierId) REFERENCES WalletIdentifiers(Id),
    FOREIGN KEY (ReceiverWalletIdentifierId) REFERENCES WalletIdentifiers(Id),
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
```

---

## Supporting Tables

### Categories

```sql
CREATE TABLE Categories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Description NVARCHAR(255) NOT NULL,
    ParentId UNIQUEIDENTIFIER NULL,            -- Hierarchical categories
    
    FOREIGN KEY (ParentId) REFERENCES Categories(Id)
);
```

### Referrals

```sql
CREATE TABLE Referrals (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AssetHolderId UNIQUEIDENTIFIER NOT NULL,   -- Referrer
    WalletIdentifierId UNIQUEIDENTIFIER NOT NULL,
    ParentCommission DECIMAL(18,4),            -- 0-100%
    ActiveFrom DATETIME2,
    ActiveUntil DATETIME2,
    
    FOREIGN KEY (AssetHolderId) REFERENCES BaseAssetHolders(Id),
    FOREIGN KEY (WalletIdentifierId) REFERENCES WalletIdentifiers(Id)
);
```

### ImportedTransactions

```sql
CREATE TABLE ImportedTransactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BaseAssetHolderId UNIQUEIDENTIFIER NOT NULL,
    Date DATETIME2 NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(MAX),
    ExternalReferenceId NVARCHAR(255),         -- FitId for OFX
    FileType INT NOT NULL,                      -- 1=OFX, 2=Excel
    FileName NVARCHAR(255) NOT NULL,
    FileHash NVARCHAR(64),
    FileSizeBytes BIGINT,
    FileMetadata NVARCHAR(MAX),                -- JSON
    Status INT NOT NULL,                        -- ImportedTransactionStatus
    ProcessedAt DATETIME2,
    ReconciledTransactionType INT,
    ReconciledTransactionId UNIQUEIDENTIFIER,
    ReconciledAt DATETIME2,
    
    FOREIGN KEY (BaseAssetHolderId) REFERENCES BaseAssetHolders(Id)
);
```

---

## Audit Fields (BaseDomain)

All tables inherit these audit fields:

```sql
CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
UpdatedAt DATETIME2,
DeletedAt DATETIME2,                -- Soft delete timestamp
CreatedBy NVARCHAR(255),
LastModifiedBy NVARCHAR(255),
DeletedBy NVARCHAR(255)
```

---

## Key Relationships

| Relationship | Type | Description |
|--------------|------|-------------|
| BaseAssetHolder → Client/Bank/Member/PokerManager | 1:1 | Entity type extension |
| BaseAssetHolder → AssetPool | 1:N | Asset pools owned |
| AssetPool → WalletIdentifier | 1:N | Wallets in pool |
| WalletIdentifier → Transaction | 1:N | As sender or receiver |
| WalletIdentifier → Referral | 1:N | Commission relationships |
| Category → Transaction | 1:N | Transaction classification |
| BaseAssetHolder → BaseAssetHolder | N:1 | Self-referential (referrer) |

---

## Indexes (Recommended)

```sql
-- Performance indexes
CREATE INDEX IX_WalletIdentifiers_AssetPoolId ON WalletIdentifiers(AssetPoolId);
CREATE INDEX IX_WalletIdentifiers_AssetType ON WalletIdentifiers(AssetType);
CREATE INDEX IX_AssetPools_BaseAssetHolderId ON AssetPools(BaseAssetHolderId);

-- Transaction query indexes
CREATE INDEX IX_FiatTransactions_Date ON FiatAssetTransactions(Date);
CREATE INDEX IX_FiatTransactions_Sender ON FiatAssetTransactions(SenderWalletIdentifierId);
CREATE INDEX IX_FiatTransactions_Receiver ON FiatAssetTransactions(ReceiverWalletIdentifierId);

-- Soft delete filtering
CREATE INDEX IX_BaseAssetHolders_DeletedAt ON BaseAssetHolders(DeletedAt);
```

---

## Related Documentation

- [ASSET_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) - Asset model details
- [TRANSACTION_INFRASTRUCTURE.md](../03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) - Transaction details
- [SOFT_DELETE_AND_DATA_LIFECYCLE.md](../05_INFRASTRUCTURE/SOFT_DELETE_AND_DATA_LIFECYCLE.md) - Deletion handling

