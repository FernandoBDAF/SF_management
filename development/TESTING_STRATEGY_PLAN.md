# Testing Strategy Plan

## Executive Summary

This document analyzes the current test implementation and provides a comprehensive plan for implementing a robust testing strategy aligned with Clean Architecture, DDD, and industry best practices.

---

## Current State Analysis

### What Exists

```
SFManagement.Tests/
├── Finance/
│   └── DirectIncomeDetailsTests.cs    # 1 test file, 1 test method
├── SFManagement.Tests.csproj
└── bin/, obj/
```

### Current Test Implementation Review

**File:** `Finance/DirectIncomeDetailsTests.cs`

#### Positive Aspects
- Uses xUnit (good choice)
- Uses InMemoryDatabase for isolation
- Has a clear test name following convention
- Tests a specific behavior

#### Issues Identified

| Issue | Severity | Description |
|-------|----------|-------------|
| **No Mocking Framework** | High | Manual test doubles are tedious and error-prone |
| **No Test Base Classes** | Medium | Duplicate setup code across tests |
| **No Test Data Builders** | Medium | 120+ lines of seed data per test |
| **Tight Coupling** | High | Tests directly instantiate `DataContext` and services |
| **No Test Categories** | Medium | No separation of unit/integration tests |
| **Minimal Coverage** | Critical | Only 1 test for entire codebase |
| **No Architecture Tests** | High | No enforcement of layer dependencies |
| **Manual Service Stubs** | Medium | `TestAvgRateService`, `TestLoggingService` are manually written |

### Current Packages

```xml
<PackageReference Include="coverlet.collector" Version="6.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
```

**Missing Essential Packages:**
- Mocking framework (Moq or NSubstitute)
- FluentAssertions for readable assertions
- Bogus for fake data generation
- Architecture testing (NetArchTest or ArchUnitNET)
- WebApplicationFactory for integration tests
- Respawn for database cleanup

---

## Recommended Testing Architecture

### Testing Pyramid for SF_management

```
                    ┌─────────────┐
                    │   E2E/UI    │  ← Future (Playwright)
                    │    Tests    │
                   ┌┴─────────────┴┐
                   │  Integration  │  ← API + Database Tests
                   │    Tests      │
                  ┌┴───────────────┴┐
                  │   Architecture  │  ← Enforce Layer Rules
                  │     Tests       │
                 ┌┴─────────────────┴┐
                 │    Unit Tests     │  ← Domain, Services, Validators
                 └───────────────────┘
```

### Recommended Test Distribution

| Test Type | Target Coverage | Priority |
|-----------|-----------------|----------|
| Unit Tests (Domain) | 90%+ | Phase 1 |
| Unit Tests (Application) | 80%+ | Phase 1 |
| Architecture Tests | 100% rules | Phase 1 |
| Integration Tests | Critical paths | Phase 2 |
| E2E Tests | Happy paths | Phase 3 |

---

## Project Structure Recommendation

### Option A: Separate Test Projects (RECOMMENDED)

```
SF_management/
├── SFManagement.csproj                    # Main project
├── tests/
│   ├── SFManagement.UnitTests/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── BaseAssetHolderTests.cs
│   │   │   │   ├── MemberTests.cs
│   │   │   │   └── ...
│   │   │   ├── ValueObjects/
│   │   │   │   ├── GovernmentNumberTests.cs
│   │   │   │   └── MoneyTests.cs
│   │   │   └── Services/
│   │   │       └── ProfitCalculatorTests.cs
│   │   ├── Application/
│   │   │   ├── Services/
│   │   │   │   ├── TransferServiceTests.cs
│   │   │   │   ├── ProfitCalculationServiceTests.cs
│   │   │   │   └── ...
│   │   │   └── Validators/
│   │   │       ├── TransferRequestValidatorTests.cs
│   │   │       └── ...
│   │   ├── Common/
│   │   │   ├── Fixtures/
│   │   │   │   ├── DatabaseFixture.cs
│   │   │   │   └── ServiceFixture.cs
│   │   │   ├── Builders/
│   │   │   │   ├── AssetHolderBuilder.cs
│   │   │   │   ├── TransactionBuilder.cs
│   │   │   │   └── WalletIdentifierBuilder.cs
│   │   │   └── Fakes/
│   │   │       └── FakeDataGenerator.cs
│   │   └── SFManagement.UnitTests.csproj
│   │
│   ├── SFManagement.IntegrationTests/
│   │   ├── Api/
│   │   │   ├── Controllers/
│   │   │   │   ├── TransferControllerTests.cs
│   │   │   │   ├── ClientControllerTests.cs
│   │   │   │   └── ...
│   │   │   └── Middleware/
│   │   │       └── ErrorHandlerMiddlewareTests.cs
│   │   ├── Database/
│   │   │   ├── RepositoryTests.cs
│   │   │   └── DataContextTests.cs
│   │   ├── Common/
│   │   │   ├── CustomWebApplicationFactory.cs
│   │   │   ├── IntegrationTestBase.cs
│   │   │   └── DatabaseSeeder.cs
│   │   └── SFManagement.IntegrationTests.csproj
│   │
│   └── SFManagement.ArchitectureTests/
│       ├── LayerDependencyTests.cs
│       ├── NamingConventionTests.cs
│       ├── DddComplianceTests.cs
│       └── SFManagement.ArchitectureTests.csproj
```

### Option B: Single Test Project with Folders (Current - NOT RECOMMENDED)

The current single-project approach doesn't scale well and mixes concerns.

---

## Recommended Packages

### Unit Tests Project

```xml
<ItemGroup>
  <!-- Test Framework -->
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  <PackageReference Include="xunit" Version="2.9.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  
  <!-- Mocking -->
  <PackageReference Include="Moq" Version="4.20.72" />
  <!-- OR -->
  <PackageReference Include="NSubstitute" Version="5.1.0" />
  
  <!-- Assertions -->
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  
  <!-- Fake Data -->
  <PackageReference Include="Bogus" Version="35.6.1" />
  
  <!-- Database (for repository tests) -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
  
  <!-- Coverage -->
  <PackageReference Include="coverlet.collector" Version="6.0.2" />
</ItemGroup>
```

### Integration Tests Project

```xml
<ItemGroup>
  <!-- All Unit Test packages plus: -->
  
  <!-- API Testing -->
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
  
  <!-- Database Cleanup -->
  <PackageReference Include="Respawn" Version="6.2.1" />
  
  <!-- Test Containers (optional - for real DB) -->
  <PackageReference Include="Testcontainers.MsSql" Version="3.10.0" />
</ItemGroup>
```

### Architecture Tests Project

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  <PackageReference Include="xunit" Version="2.9.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  
  <!-- Architecture Testing -->
  <PackageReference Include="NetArchTest.Rules" Version="1.3.2" />
  <!-- OR -->
  <PackageReference Include="ArchUnitNET.xUnit" Version="0.10.6" />
  
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>
```

---

## Implementation Patterns

### 1. Test Data Builders (Builder Pattern)

**Current Problem:** 120+ lines of manual entity creation

**Solution:**

```csharp
// tests/SFManagement.UnitTests/Common/Builders/AssetHolderBuilder.cs
public class AssetHolderBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Manager";
    private TaxEntityType _taxEntityType = TaxEntityType.CPF;
    private string _governmentNumber = "12345678901";
    
    public AssetHolderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }
    
    public AssetHolderBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public AssetHolderBuilder AsCompany()
    {
        _taxEntityType = TaxEntityType.CNPJ;
        _governmentNumber = "12345678901234";
        return this;
    }
    
    public AssetHolderBuilder AsIndividual()
    {
        _taxEntityType = TaxEntityType.CPF;
        _governmentNumber = "12345678901";
        return this;
    }
    
    public BaseAssetHolder Build()
    {
        return new BaseAssetHolder
        {
            Id = _id,
            Name = _name,
            TaxEntityType = _taxEntityType,
            GovernmentNumber = _governmentNumber,
            LastModifiedBy = Guid.NewGuid()
        };
    }
    
    // Implicit conversion for cleaner tests
    public static implicit operator BaseAssetHolder(AssetHolderBuilder builder) 
        => builder.Build();
}

// Usage in tests:
var manager = new AssetHolderBuilder()
    .WithName("Manager A")
    .AsCompany()
    .Build();
```

### 2. Fake Data Generator (using Bogus)

```csharp
// tests/SFManagement.UnitTests/Common/Fakes/FakeDataGenerator.cs
public static class FakeDataGenerator
{
    private static readonly Faker _faker = new("pt_BR");
    
    public static Faker<BaseAssetHolder> AssetHolder => new Faker<BaseAssetHolder>()
        .RuleFor(x => x.Id, f => Guid.NewGuid())
        .RuleFor(x => x.Name, f => f.Company.CompanyName())
        .RuleFor(x => x.TaxEntityType, f => f.PickRandom<TaxEntityType>())
        .RuleFor(x => x.GovernmentNumber, f => f.Random.String2(11, "0123456789"))
        .RuleFor(x => x.LastModifiedBy, f => Guid.NewGuid());
    
    public static Faker<WalletIdentifier> WalletIdentifier => new Faker<WalletIdentifier>()
        .RuleFor(x => x.Id, f => Guid.NewGuid())
        .RuleFor(x => x.AssetType, f => f.PickRandom<AssetType>())
        .RuleFor(x => x.AccountClassification, f => f.PickRandom<AccountClassification>())
        .RuleFor(x => x.LastModifiedBy, f => Guid.NewGuid());
    
    public static Faker<FiatAssetTransaction> FiatTransaction => new Faker<FiatAssetTransaction>()
        .RuleFor(x => x.Id, f => Guid.NewGuid())
        .RuleFor(x => x.Date, f => f.Date.Recent(30))
        .RuleFor(x => x.AssetAmount, f => f.Finance.Amount(100, 10000))
        .RuleFor(x => x.LastModifiedBy, f => Guid.NewGuid());
}

// Usage:
var transactions = FakeDataGenerator.FiatTransaction.Generate(10);
```

### 3. Test Fixtures (Shared Context)

```csharp
// tests/SFManagement.UnitTests/Common/Fixtures/DatabaseFixture.cs
public class DatabaseFixture : IDisposable
{
    public DataContext Context { get; }
    
    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        Context = new DataContext(
            options, 
            new NullHttpContextAccessor(), 
            new NullLoggingService());
    }
    
    public void Dispose()
    {
        Context.Dispose();
    }
}

// Collection fixture for sharing across tests
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

// Usage:
[Collection("Database")]
public class TransferServiceTests
{
    private readonly DatabaseFixture _fixture;
    
    public TransferServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### 4. Unit Test Pattern (AAA)

```csharp
// tests/SFManagement.UnitTests/Application/Services/TransferServiceTests.cs
public class TransferServiceTests
{
    private readonly Mock<IDataContext> _contextMock;
    private readonly Mock<IAvgRateService> _avgRateServiceMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly TransferService _sut; // System Under Test
    
    public TransferServiceTests()
    {
        _contextMock = new Mock<IDataContext>();
        _avgRateServiceMock = new Mock<IAvgRateService>();
        _cacheMock = new Mock<IMemoryCache>();
        
        _sut = new TransferService(
            _contextMock.Object,
            _avgRateServiceMock.Object,
            _cacheMock.Object);
    }
    
    [Fact]
    public async Task TransferAsync_WithValidRequest_CreatesTransactions()
    {
        // Arrange
        var request = new TransferRequestBuilder()
            .WithAmount(1000m)
            .FromWallet(Guid.NewGuid())
            .ToWallet(Guid.NewGuid())
            .Build();
        
        _contextMock.Setup(x => x.WalletIdentifiers)
            .Returns(MockDbSet(new List<WalletIdentifier> { /* ... */ }));
        
        // Act
        var result = await _sut.TransferAsync(request);
        
        // Assert
        result.Should().NotBeNull();
        result.Transactions.Should().HaveCount(2);
        _contextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task TransferAsync_WithInvalidAmount_ThrowsValidationException(decimal amount)
    {
        // Arrange
        var request = new TransferRequestBuilder()
            .WithAmount(amount)
            .Build();
        
        // Act
        var act = () => _sut.TransferAsync(request);
        
        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*amount*");
    }
}
```

### 5. Architecture Tests

```csharp
// tests/SFManagement.ArchitectureTests/LayerDependencyTests.cs
public class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(BaseDomain).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(BaseService<>).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(DataContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Program).Assembly;
    
    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("SFManagement.Application")
            .GetResult();
        
        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer must not depend on Application layer");
    }
    
    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("SFManagement.Infrastructure")
            .GetResult();
        
        result.IsSuccessful.Should().BeTrue(
            because: "Domain layer must not depend on Infrastructure layer");
    }
    
    [Fact]
    public void Application_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("SFManagement.Api")
            .GetResult();
        
        result.IsSuccessful.Should().BeTrue();
    }
    
    [Fact]
    public void Controllers_Should_Be_In_Api_Namespace()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .ResideInNamespace("SFManagement.Api.Controllers")
            .GetResult();
        
        result.IsSuccessful.Should().BeTrue();
    }
    
    [Fact]
    public void Services_Should_Have_Interface()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Service")
            .And()
            .AreClasses()
            .Should()
            .ImplementInterface(typeof(object)) // Check has at least one interface
            .GetResult();
        
        // This will fail currently - documenting technical debt
    }
}

// tests/SFManagement.ArchitectureTests/NamingConventionTests.cs
public class NamingConventionTests
{
    [Fact]
    public void Interfaces_Should_Start_With_I()
    {
        var result = Types.InAssembly(typeof(BaseDomain).Assembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();
        
        result.IsSuccessful.Should().BeTrue();
    }
    
    [Fact]
    public void DTOs_Should_End_With_Request_Or_Response()
    {
        var result = Types.InNamespace("SFManagement.Application.DTOs")
            .That()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Request")
            .Or()
            .HaveNameEndingWith("Response")
            .Or()
            .HaveNameEndingWith("DTO")
            .GetResult();
        
        // Documenting convention
    }
}
```

### 6. Integration Test Pattern

```csharp
// tests/SFManagement.IntegrationTests/Common/CustomWebApplicationFactory.cs
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DataContext>));
            if (descriptor != null)
                services.Remove(descriptor);
            
            // Add in-memory database
            services.AddDbContext<DataContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
            });
            
            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            context.Database.EnsureCreated();
            SeedTestData(context);
        });
    }
    
    private static void SeedTestData(DataContext context)
    {
        // Minimal seed data for integration tests
    }
}

// tests/SFManagement.IntegrationTests/Api/Controllers/TransferControllerTests.cs
public class TransferControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public TransferControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task Transfer_WithValidRequest_Returns200()
    {
        // Arrange
        var request = new TransferRequest
        {
            Amount = 1000m,
            SenderWalletId = TestData.SenderWalletId,
            ReceiverWalletId = TestData.ReceiverWalletId
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/transfer", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<TransferResponse>();
        result.Should().NotBeNull();
        result!.Transactions.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task Transfer_WithInsufficientBalance_Returns400()
    {
        // Arrange
        var request = new TransferRequest
        {
            Amount = 999999999m, // Excessive amount
            SenderWalletId = TestData.SenderWalletId,
            ReceiverWalletId = TestData.ReceiverWalletId
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/transfer", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

---

## Implementation Roadmap

### Phase 1: Foundation (Week 1-2)

#### Tasks

1. **Create Test Projects Structure**
   - Create `SFManagement.UnitTests` project
   - Create `SFManagement.ArchitectureTests` project
   - Move existing test to new structure
   - Add recommended packages

2. **Create Test Infrastructure**
   - Test data builders for all entities
   - Fake data generator with Bogus
   - Database fixture
   - Null implementations for dependencies

3. **Architecture Tests**
   - Layer dependency tests
   - Naming convention tests
   - DDD compliance tests

#### Deliverables
- 3 test projects configured
- Test infrastructure classes
- 10+ architecture tests passing (some may fail - documenting debt)

### Phase 2: Domain Unit Tests (Week 3-4)

#### Tasks

1. **Entity Tests**
   - `BaseAssetHolder` validation tests
   - `Member` share validation tests
   - `WalletIdentifier` metadata tests
   - `Referral` date range tests

2. **Value Object Tests** (after creating Value Objects)
   - `GovernmentNumber` validation tests
   - `Money` arithmetic tests

3. **Domain Service Tests**
   - `ProfitCalculator` tests (after moving from Application)
   - `BalanceCalculator` tests (after moving from Application)

#### Deliverables
- 50+ domain unit tests
- 90%+ domain layer coverage

### Phase 3: Application Unit Tests (Week 5-6)

#### Tasks

1. **Service Tests**
   - `TransferService` tests
   - `ProfitCalculationService` tests
   - `AvgRateService` tests
   - `BaseAssetHolderService` tests

2. **Validator Tests**
   - All FluentValidation validators
   - Edge cases and boundary conditions

#### Deliverables
- 100+ application unit tests
- 80%+ application layer coverage

### Phase 4: Integration Tests (Week 7-8)

#### Tasks

1. **Setup Integration Test Project**
   - `SFManagement.IntegrationTests` project
   - `CustomWebApplicationFactory`
   - Database seeding
   - Respawn for cleanup

2. **API Controller Tests**
   - Transfer endpoints
   - Client CRUD endpoints
   - Finance endpoints
   - Error handling tests

3. **Database Tests**
   - Repository pattern tests (after implementation)
   - Complex query tests

#### Deliverables
- Integration test project configured
- 30+ integration tests for critical paths

### Phase 5: CI/CD Integration (Week 9-10)

#### Tasks

1. **GitHub Actions Updates**
   - Separate test jobs for unit/integration
   - Code coverage reporting
   - Test result artifacts

2. **Quality Gates**
   - Minimum coverage thresholds
   - Architecture test gates
   - PR checks

#### Deliverables
- Updated CI/CD pipeline
- Coverage reports in PRs
- Quality gates enforced

---

## Test Naming Conventions

### Unit Tests

```
MethodName_StateUnderTest_ExpectedBehavior
```

**Examples:**
- `TransferAsync_WithValidRequest_CreatesTransactions`
- `TransferAsync_WithNegativeAmount_ThrowsValidationException`
- `CalculateBalance_WithNoTransactions_ReturnsInitialBalance`

### Integration Tests

```
Action_Scenario_ExpectedResult
```

**Examples:**
- `Transfer_WithValidRequest_Returns200`
- `GetClient_WithInvalidId_Returns404`
- `CreateTransaction_WithDuplicateId_Returns409`

---

## Coverage Targets

| Layer | Minimum | Target | Current |
|-------|---------|--------|---------|
| Domain | 80% | 90% | ~0% |
| Application | 70% | 80% | ~1% |
| Infrastructure | 50% | 60% | 0% |
| API | 60% | 70% | 0% |
| **Overall** | **65%** | **75%** | **~0%** |

---

## Tooling Setup

### VS Code / Cursor Settings

```json
// .vscode/settings.json
{
  "dotnet-test-explorer.testProjectPath": "tests/**/*.csproj",
  "dotnet-test-explorer.runInParallel": true
}
```

### Coverage Report Generation

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

### GitHub Actions Example

```yaml
- name: Run Unit Tests
  run: dotnet test tests/SFManagement.UnitTests --no-build --verbosity normal

- name: Run Integration Tests
  run: dotnet test tests/SFManagement.IntegrationTests --no-build --verbosity normal

- name: Run Architecture Tests
  run: dotnet test tests/SFManagement.ArchitectureTests --no-build --verbosity normal

- name: Upload Coverage
  uses: codecov/codecov-action@v3
  with:
    files: "**/coverage.cobertura.xml"
```

---

## Quick Wins (Immediate Actions)

### 1. Add Essential Packages to Current Project

```xml
<!-- Add to SFManagement.Tests.csproj -->
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Bogus" Version="35.6.1" />
<PackageReference Include="NetArchTest.Rules" Version="1.3.2" />
```

### 2. Create Architecture Tests Now

Even with minimal unit tests, architecture tests immediately enforce Clean Architecture rules:

```csharp
[Fact]
public void Domain_Should_Not_Reference_Application()
{
    // This test will FAIL currently (documenting the violation)
    // WalletIdentifier.cs references WalletIdentifierValidationService
}
```

### 3. Refactor Existing Test

```csharp
// Before: 234 lines with manual setup
// After: ~50 lines with builders
[Fact]
public async Task GetDirectIncomeDetails_NetDirectIncome_Matches_ProfitSummary()
{
    // Arrange
    using var fixture = new DatabaseFixture();
    var manager = new AssetHolderBuilder().AsCompany().Build();
    var systemPool = new AssetPoolBuilder().AsSystemWallet().Build();
    // ... use builders instead of 120 lines of manual setup
}
```

---

## Summary

The current test implementation needs significant improvement:

| Aspect | Current | Target |
|--------|---------|--------|
| Test Files | 1 | 50+ |
| Test Methods | 1 | 200+ |
| Coverage | ~0% | 75%+ |
| Architecture Tests | 0 | 20+ |
| Integration Tests | 0 | 30+ |
| Test Projects | 1 | 3 |

The recommended approach splits tests into:
1. **Unit Tests** - Fast, isolated, high coverage
2. **Integration Tests** - API and database validation
3. **Architecture Tests** - Enforce Clean Architecture/DDD rules

Priority should be given to Architecture Tests first (quick wins) and Domain Unit Tests (highest value).

---

*Created: January 23, 2026*
