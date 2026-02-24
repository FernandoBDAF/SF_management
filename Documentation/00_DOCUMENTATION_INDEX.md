# SF Management Documentation Index

Welcome to the SF Management documentation. This index provides a comprehensive guide to understanding the system's architecture, business domain, and implementation details.

---

## Quick Navigation

| Category | Purpose | Key Documents |
|----------|---------|---------------|
| [Business](#01_business) | Business context and domain concepts | Business Domain Overview |
| [Architecture](#02_architecture) | System design and patterns | Service Layer, Controllers, Database, AutoMapper |
| [Core Systems](#03_core_systems) | Primary system functionality | Assets, Entities, Transactions, Finance, Settlements |
| [Supporting Systems](#04_supporting_systems) | Auxiliary features | Categories, Initial Balances, Contact Information |
| [Infrastructure](#05_infrastructure) | Technical infrastructure | Auth, Logging, Error Handling, Validation, Configuration |
| [API](#06_api) | API documentation | Complete API Reference, Specialized Endpoints |
| [Reference](#07_reference) | Technical reference | Enums, Type System |
| [Business Rules](#08_business_rules) | Business rule documentation | Asset Valuation Rules |
| [Development](#09_development) | Developer resources | Development Guide |
| [Deployment](#10_deployment) | CI/CD and cloud infrastructure | CI/CD Pipeline, Azure Infrastructure |
| [Refactoring](#11_refactoring) | Code improvement plans | Architecture, Testing, Contracts, AvgRate Fix |

---

## Documentation Structure

### 01_BUSINESS

Business domain documentation providing context for non-technical stakeholders and new developers.

| Document | Description |
|----------|-------------|
| [BUSINESS_DOMAIN_OVERVIEW.md](01_BUSINESS/BUSINESS_DOMAIN_OVERVIEW.md) | Overview of poker staking management, key business concepts, entity relationships, and common workflows |

---

### 02_ARCHITECTURE

System architecture documentation covering design patterns and structural decisions.

| Document | Description |
|----------|-------------|
| [SERVICE_LAYER_ARCHITECTURE.md](02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) | Service layer patterns including `BaseService`, `BaseAssetHolderService`, and `BaseTransactionService` |
| [CONTROLLER_LAYER_ARCHITECTURE.md](02_ARCHITECTURE/CONTROLLER_LAYER_ARCHITECTURE.md) | API controller patterns, `BaseApiController`, `BaseAssetHolderController`, and route conventions |
| [DATABASE_SCHEMA.md](02_ARCHITECTURE/DATABASE_SCHEMA.md) | Entity Framework configuration, database relationships, indexes, and constraints |
| [AUTOMAPPER_CONFIGURATION.md](02_ARCHITECTURE/AUTOMAPPER_CONFIGURATION.md) | Object mapping profiles, custom resolvers, and mapping strategies |

---

### 03_CORE_SYSTEMS

Documentation for the primary systems that power the application.

| Document | Description |
|----------|-------------|
| [ENTITY_INFRASTRUCTURE.md](03_CORE_SYSTEMS/ENTITY_INFRASTRUCTURE.md) | BaseAssetHolder hierarchy, entity relationships, and asset holder management |
| [ENTITY_BUSINESS_BEHAVIOR.md](03_CORE_SYSTEMS/ENTITY_BUSINESS_BEHAVIOR.md) | Business behavior, balance meanings, and transaction rules per entity type |
| [ASSET_INFRASTRUCTURE.md](03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md) | Asset pools, wallet identifiers, metadata system, and asset management |
| [TRANSACTION_INFRASTRUCTURE.md](03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md) | Transaction models, transaction modes, self-conversion, TransferService, guardrails |
| [TRANSACTION_BALANCE_IMPACT.md](03_CORE_SYSTEMS/TRANSACTION_BALANCE_IMPACT.md) | Single source of truth for how transactions impact balances |
| [TRANSACTION_RESPONSE_VIEWMODELS.md](03_CORE_SYSTEMS/TRANSACTION_RESPONSE_VIEWMODELS.md) | Transaction response DTOs including TransferResponse and WalletMissingError |
| [FINANCE_SYSTEM.md](03_CORE_SYSTEMS/FINANCE_SYSTEM.md) | Company revenue model overview and finance API endpoints |
| [PROFIT_CALCULATION_SYSTEM.md](03_CORE_SYSTEMS/PROFIT_CALCULATION_SYSTEM.md) | **NEW:** Deep-dive into all four profit sources, AvgRate algorithm, caching, and data flows |
| [REFERRAL_SYSTEM.md](03_CORE_SYSTEMS/REFERRAL_SYSTEM.md) | Commission tracking system, referral management, and business rules |
| [SETTLEMENT_WORKFLOW.md](03_CORE_SYSTEMS/SETTLEMENT_WORKFLOW.md) | Poker settlement process, rake calculations, and batch settlements |
| [IMPORTED_TRANSACTIONS.md](03_CORE_SYSTEMS/IMPORTED_TRANSACTIONS.md) | File import system for OFX and Excel, reconciliation workflow |

---

### 04_SUPPORTING_SYSTEMS

Documentation for auxiliary systems that support core functionality.

| Document | Description |
|----------|-------------|
| [CATEGORY_SYSTEM.md](04_SUPPORTING_SYSTEMS/CATEGORY_SYSTEM.md) | Transaction categorization, hierarchical categories, and reporting |
| [INITIAL_BALANCES.md](04_SUPPORTING_SYSTEMS/INITIAL_BALANCES.md) | Starting balance configuration per asset type and group |
| [CONTACT_INFORMATION.md](04_SUPPORTING_SYSTEMS/CONTACT_INFORMATION.md) | Address and phone management for asset holders |

---

### 05_INFRASTRUCTURE

Technical infrastructure documentation covering cross-cutting concerns.

| Document | Description |
|----------|-------------|
| [AUTHENTICATION.md](05_INFRASTRUCTURE/AUTHENTICATION.md) | Auth0 integration, JWT handling, roles, and permissions |
| [LOGGING.md](05_INFRASTRUCTURE/LOGGING.md) | Serilog configuration, log sinks, and structured logging |
| [AUDIT_SYSTEM.md](05_INFRASTRUCTURE/AUDIT_SYSTEM.md) | Audit trail system, automatic timestamps, user tracking, and data change history |
| [ERROR_HANDLING.md](05_INFRASTRUCTURE/ERROR_HANDLING.md) | Exception hierarchy, middleware details, and error response structure |
| [VALIDATION_SYSTEM.md](05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) | FluentValidation setup and service-level validation |
| [SOFT_DELETE_AND_DATA_LIFECYCLE.md](05_INFRASTRUCTURE/SOFT_DELETE_AND_DATA_LIFECYCLE.md) | Soft delete implementation and data retention |
| [RATE_LIMITING_AND_PERFORMANCE.md](05_INFRASTRUCTURE/RATE_LIMITING_AND_PERFORMANCE.md) | Rate limiting, **IMemoryCache caching**, CacheMetricsService, CachedLookupService |
| [CONFIGURATION_MANAGEMENT.md](05_INFRASTRUCTURE/CONFIGURATION_MANAGEMENT.md) | Application settings, environment variables, user secrets, and dependency management |
| [CACHING_STRATEGY.md](05_INFRASTRUCTURE/CACHING_STRATEGY.md) | Future caching extension plan for heavy queries |
| [datetime_standards.md](05_INFRASTRUCTURE/datetime_standards.md) | UTC storage, API date formats, timezone handling, implementation patterns |

---

### 06_API

API documentation and endpoint references.

| Document | Description |
|----------|-------------|
| [API_REFERENCE.md](06_API/API_REFERENCE.md) | Complete endpoint reference for all controllers |
| [TRANSACTION_API_ENDPOINTS.md](06_API/TRANSACTION_API_ENDPOINTS.md) | **Comprehensive transaction API reference** with detailed examples, error codes, and use cases |
| [COMPANY_ASSET_POOL_ENDPOINTS.md](06_API/COMPANY_ASSET_POOL_ENDPOINTS.md) | Specialized endpoints for company-owned asset pools and system wallet pairing |
| [INTERNAL_WALLET_TYPE_IMPLEMENTATION.md](06_API/INTERNAL_WALLET_TYPE_IMPLEMENTATION.md) | Internal and Settlement wallet group implementation |
| [BALANCE_ENDPOINTS.md](06_API/BALANCE_ENDPOINTS.md) | Balance endpoint patterns and frontend alignment notes |

---

### 07_REFERENCE

Technical reference documentation.

| Document | Description |
|----------|-------------|
| [ENUMS_AND_TYPE_SYSTEM.md](07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) | All enumerations with business meanings and values |
| [ASSET_POOL_COMPANY_OWNERSHIP_ANALYSIS.md](07_REFERENCE/ASSET_POOL_COMPANY_OWNERSHIP_ANALYSIS.md) | Analysis of company-owned asset pools |
| [FINANCE_DEFERRED_FEATURES.md](07_REFERENCE/FINANCE_DEFERRED_FEATURES.md) | Deferred finance features for future implementation |

---

### 08_BUSINESS_RULES

Business rule documentation for key domain behaviors.

| Document | Description |
|----------|-------------|
| [ASSET_VALUATION_RULES.md](08_BUSINESS_RULES/ASSET_VALUATION_RULES.md) | Balance modes, InitialBalance configuration, and AvgRate rules |

---

### 09_DEVELOPMENT

Developer resources for onboarding and daily development.

| Document | Description |
|----------|-------------|
| [DEVELOPMENT_GUIDE.md](09_DEVELOPMENT/DEVELOPMENT_GUIDE.md) | Development environment setup, coding conventions, and common tasks |

---

### 10_DEPLOYMENT

Deployment and cloud infrastructure documentation.

| Document | Description |
|----------|-------------|
| [CI_CD_PIPELINE.md](10_DEPLOYMENT/CI_CD_PIPELINE.md) | GitHub Actions workflows, build/deploy stages, OIDC authentication, and rollback procedures |
| [AZURE_INFRASTRUCTURE.md](10_DEPLOYMENT/AZURE_INFRASTRUCTURE.md) | Azure Web App configuration, SQL Database setup, health checks, and monitoring |

---

### 11_REFACTORING

Future implementation plans and roadmaps.

| Document | Status | Description |
|----------|--------|-------------|
| [CLEAN_ARCHITECTURE_IMPROVEMENT_PLAN.md](11_REFACTORING/CLEAN_ARCHITECTURE_IMPROVEMENT_PLAN.md) | 📋 Planning | Comprehensive DDD/Clean Architecture improvement roadmap |
| [TESTING_STRATEGY_PLAN.md](11_REFACTORING/TESTING_STRATEGY_PLAN.md) | 📋 Planning | Complete testing strategy with patterns and implementation roadmap |
| [BALANCE_IMPROVEMENTS_PLAN.md](11_REFACTORING/BALANCE_IMPROVEMENTS_PLAN.md) | 📋 Planning | Balance API improvements and date-filtered endpoints |
| [CONTRACTS_IMPLEMENTATION_PLAN.md](11_REFACTORING/CONTRACTS_IMPLEMENTATION_PLAN.md) | 📋 Planning | Recurring payments for services, employees, and contracts |
| [FINANCE_MODULE_VISION.md](11_REFACTORING/FINANCE_MODULE_VISION.md) | 🔄 Active | Guiding vision for Finance Module (some phases pending) |
| [AVGRATE_CALCULATION_FIX_PLAN.md](11_REFACTORING/AVGRATE_CALCULATION_FIX_PLAN.md) | 📋 Planning | Fix for AvgRate calculation inconsistencies and transaction ordering |
| [ASSETGROUP_FLEXIBLE_RENAME_PLAN.md](11_REFACTORING/ASSETGROUP_FLEXIBLE_RENAME_PLAN.md) | ⏸️ Deferred | Rename `AssetGroup.Internal` → `Flexible` |

---

## Getting Started

### For New Developers

1. **Start with business context**: Read [BUSINESS_DOMAIN_OVERVIEW.md](01_BUSINESS/BUSINESS_DOMAIN_OVERVIEW.md) to understand the poker staking domain
2. **Understand the data model**: Review [ENTITY_INFRASTRUCTURE.md](03_CORE_SYSTEMS/ENTITY_INFRASTRUCTURE.md) and [ASSET_INFRASTRUCTURE.md](03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md)
3. **Learn the architecture**: Study [SERVICE_LAYER_ARCHITECTURE.md](02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) and [CONTROLLER_LAYER_ARCHITECTURE.md](02_ARCHITECTURE/CONTROLLER_LAYER_ARCHITECTURE.md)
4. **Set up your environment**: Follow [DEVELOPMENT_GUIDE.md](09_DEVELOPMENT/DEVELOPMENT_GUIDE.md) and [CONFIGURATION_MANAGEMENT.md](05_INFRASTRUCTURE/CONFIGURATION_MANAGEMENT.md)
5. **Explore the API**: Reference [API_REFERENCE.md](06_API/API_REFERENCE.md) for endpoint details

### For Specific Topics

| If you need to... | Read... |
|-------------------|---------|
| Add a new entity type | [ENTITY_INFRASTRUCTURE.md](03_CORE_SYSTEMS/ENTITY_INFRASTRUCTURE.md) |
| Create new endpoints | [CONTROLLER_LAYER_ARCHITECTURE.md](02_ARCHITECTURE/CONTROLLER_LAYER_ARCHITECTURE.md), [API_REFERENCE.md](06_API/API_REFERENCE.md) |
| Add new asset types | [ASSET_INFRASTRUCTURE.md](03_CORE_SYSTEMS/ASSET_INFRASTRUCTURE.md), [ENUMS_AND_TYPE_SYSTEM.md](07_REFERENCE/ENUMS_AND_TYPE_SYSTEM.md) |
| Handle transactions | [TRANSACTION_INFRASTRUCTURE.md](03_CORE_SYSTEMS/TRANSACTION_INFRASTRUCTURE.md), [TRANSACTION_API_ENDPOINTS.md](06_API/TRANSACTION_API_ENDPOINTS.md) |
| Create transfers (P2P) | [TRANSACTION_API_ENDPOINTS.md](06_API/TRANSACTION_API_ENDPOINTS.md) - Use `/api/v1/transfer` |
| Understand profit calculation | [PROFIT_CALCULATION_SYSTEM.md](03_CORE_SYSTEMS/PROFIT_CALCULATION_SYSTEM.md), [FINANCE_SYSTEM.md](03_CORE_SYSTEMS/FINANCE_SYSTEM.md), [ASSET_VALUATION_RULES.md](08_BUSINESS_RULES/ASSET_VALUATION_RULES.md) |
| Configure authentication | [AUTHENTICATION.md](05_INFRASTRUCTURE/AUTHENTICATION.md) |
| Add validation | [VALIDATION_SYSTEM.md](05_INFRASTRUCTURE/VALIDATION_SYSTEM.md) |
| Handle errors | [ERROR_HANDLING.md](05_INFRASTRUCTURE/ERROR_HANDLING.md) |
| Import external data | [IMPORTED_TRANSACTIONS.md](03_CORE_SYSTEMS/IMPORTED_TRANSACTIONS.md) |
| Configure the application | [CONFIGURATION_MANAGEMENT.md](05_INFRASTRUCTURE/CONFIGURATION_MANAGEMENT.md) |
| Deploy to Azure | [CI_CD_PIPELINE.md](10_DEPLOYMENT/CI_CD_PIPELINE.md), [AZURE_INFRASTRUCTURE.md](10_DEPLOYMENT/AZURE_INFRASTRUCTURE.md) |
| Understand folder structure | [SERVICE_LAYER_ARCHITECTURE.md](02_ARCHITECTURE/SERVICE_LAYER_ARCHITECTURE.md) |

---

## Document Statistics

| Category | Count |
|----------|-------|
| Business | 1 |
| Architecture | 4 |
| Core Systems | 11 |
| Supporting Systems | 3 |
| Infrastructure | 10 |
| API | 5 |
| Reference | 3 |
| Business Rules | 1 |
| Development | 1 |
| Deployment | 2 |
| Refactoring | 7 |
| **Total** | **48** |

---

## Maintenance

### Documentation Standards

- **Keep documentation current**: Update documents when code changes
- **Use consistent formatting**: Follow Markdown conventions
- **Include code examples**: Reference actual codebase code
- **Cross-reference**: Link to related documents
- **Version awareness**: Note significant changes

### Contributing to Documentation

1. Create or update documents in the appropriate category folder
2. Update this index if adding new documents
3. Maintain consistent formatting with existing documents
4. Include practical examples where applicable
5. Review for accuracy against current codebase

---

*Last updated: February 23, 2026*
