# LinkedIn Backend Engineering Apprenticeship - Question 3: Your Engineering Talent

> **Project:** SF Management API (financial REST API)  
> **Repository:** https://github.com/FernandoBDAF/SF_management

---

This project evolved with me. I started as product owner because my company needed better financial management. The first version was a MVC app I co-built while learning to code. About two years ago, I took over solo and refactored it into a modern REST API. What began as a tool to modernize my company's operations became my primary vehicle for self-improvement as a software engineer.

---

## 1) Why this project matters + contributions

I kept building SF Management because I wanted a "real" backend problem, not a toy CRUD app. Financial systems are strict: if a balance is wrong, trust is gone. The core of my work was translating business rules into a clean architecture and a database model that can evolve.

I implemented:
- Multi-entity domain model with wallet-based ledger (clients, banks, managers, transfers, balance tracking)
- Service layer with explicit business rules and validations
- Auth0 JWT auth + authorization policies (roles + permissions)
- Performance tuning in SQL Server/EF Core (indexes, query patterns)
- Reliability + observability (structured logs, health checks, caching diagnostics)
- CI/CD to Azure with GitHub Actions and OIDC

---

## 2) Concrete code examples

**Separation of concerns:** `Api/Controllers/v1/Transactions/TransferController.cs` stays thin — it logs request context and delegates to the service layer. HTTP concerns (status codes, validation responses) are separate from business logic so the service can be tested and reused.

**Audit trail enforced centrally:** `Infrastructure/Data/DataContext.cs` intercepts SaveChanges to set CreatedAt, UpdatedAt, LastModifiedBy automatically. This prevents a common bug: forgetting to set audit info in random services.

**Authorization that is explicit and inspectable:** `Infrastructure/Authorization/Auth0AuthorizationHandlers.cs` checks Auth0 permission claims and logs allow/deny decisions. Custom handlers (more work) make rules explicit and easier to audit.

**Scalability decisions:**
1. In `Application/Services/Finance/ProfitCalculationService.cs`, profit calculations query system wallet IDs repeatedly. Without caching, each call is O(n) against the database. I cache the ID list, reducing subsequent lookups to O(1).
2. In `Application/Services/Finance/AvgRateService.cs`, the original recursive algorithm caused stack overflows on accounts with years of history. I refactored to iterative with caching, fixing the issue.

**Database performance by design:** `Infrastructure/Data/DataContext.cs` (OnModelCreating) defines composite indexes matching query patterns (wallet + date range). I learned that "add indexes everywhere" is wrong — I select indexes that protect real reads, accepting the write cost.

---

## 3) Overcoming challenges

My biggest challenge was translating messy business rules into clean code and a database plan. I had gaps over time (EF Core, auth policies, indexing strategy). My approach was disciplined: I talked with more experienced engineers, went back to fundamentals (books, official docs), and iterated in phases.

**Concrete example:** The profit calculation query was timing out (12+ seconds). I profiled it, found N+1 queries, rewrote it using explicit joins in `ProfitCalculationService.cs`, and added caching. Result: 0.3s response time.

---

## 4) Leveraging AI

I use AI (Cursor + ChatGPT) heavily, but with structure. I start by planning and documenting decisions, then implement, then clean up.

**Specific example:** When building `Application/Services/Finance/AvgRateService.cs`, the AI suggested a recursive algorithm that caused stack overflows on large histories. I caught this during testing, refactored to iterative, and added caching. Lesson: AI accelerates scaffolding, but edge cases need manual review.

---

## 5) Best practices + weakness

The biggest improvement was error handling + logging. `Api/Middleware/ErrorHandlerMiddleware.cs` provides consistent error mapping that makes debugging faster. I also adopted strict naming conventions (`*Service`, `*Validator`, `*Request`).

**Weakness:** This repo currently lacks persisted tests. My plan is to add integration tests soon. Meanwhile, I'm building a side project using TDD from the start.

---

*Last updated: January 23, 2026*
