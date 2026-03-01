# Azure Infrastructure

This document describes the Azure cloud infrastructure that hosts the SF Management API. The application is deployed to Azure Web Apps with Azure SQL Database for data persistence.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Azure Resources](#azure-resources)
- [Web App Configuration](#web-app-configuration)
- [Database Configuration](#database-configuration)
- [Health Checks](#health-checks)
- [Environment Configuration](#environment-configuration)
- [Monitoring](#monitoring)
- [Security](#security)
- [Scaling](#scaling)

---

## Overview

The SF Management API runs on Microsoft Azure using a Platform-as-a-Service (PaaS) architecture. This approach provides:

- **Managed infrastructure** - Azure handles OS updates, patching, and server maintenance
- **Scalability** - Easy vertical and horizontal scaling options
- **High availability** - Built-in redundancy and failover
- **Security** - Enterprise-grade security features and compliance

---

## Architecture

### Infrastructure Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Internet                                    │
└─────────────────────────────────┬───────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                           Azure Front Door                              │
│                        (Optional CDN/WAF Layer)                         │
└─────────────────────────────────┬───────────────────────────────────────┘
                                  │
                    ┌─────────────┴─────────────┐
                    │                           │
                    ▼                           ▼
┌───────────────────────────┐   ┌───────────────────────────┐
│    Azure Web App          │   │    Azure Web App          │
│  sfmanagement-api-hmg     │   │    sfmanagement-api       │
│       (Staging)           │   │     (Production)          │
│                           │   │                           │
│  ┌─────────────────────┐  │   │  ┌─────────────────────┐  │
│  │  .NET 9 Runtime     │  │   │  │  .NET 9 Runtime     │  │
│  │  SF Management API  │  │   │  │  SF Management API  │  │
│  └─────────────────────┘  │   │  └─────────────────────┘  │
└─────────────┬─────────────┘   └─────────────┬─────────────┘
              │                               │
              │         ┌─────────────────────┘
              │         │
              ▼         ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         Azure SQL Database                              │
│                        (Production/Staging DBs)                         │
└─────────────────────────────────────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                          Azure Key Vault                                │
│                     (Secrets and Certificates)                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Azure Resources

### Resource Summary

| Resource Type | Production | Staging |
|---------------|------------|---------|
| **Web App** | `sfmanagement-api` | `sfmanagement-api-hmg` |
| **App Service Plan** | Shared/Dedicated | Shared/Dedicated |
| **SQL Database** | Production DB | Staging DB |
| **Key Vault** | Shared | Shared |
| **Application Insights** | Production instance | Staging instance |

### Resource Details

#### Azure Web App

- **Runtime:** Windows with .NET 9
- **Deployment Slot:** Production (single slot per environment)
- **HTTPS:** Enforced
- **Platform:** 64-bit

#### Azure SQL Database

- **Engine:** SQL Server
- **Connectivity:** Public endpoint with firewall rules
- **Backup:** Automatic backups enabled

#### Azure Key Vault

- **Purpose:** Secure storage for:
  - Connection strings
  - Auth0 secrets
  - API keys
- **Access:** Managed Identity from Web App

#### Application Insights

- **Integration:** Via Serilog sink
- **Features:** Request tracing, performance monitoring, error tracking

---

## Web App Configuration

### App Service Plan

The Web App runs on an App Service Plan that determines:
- Available compute resources (CPU, memory)
- Scaling options
- Features available (custom domains, SSL, slots)

### Runtime Configuration

- **Stack:** .NET
- **Version:** .NET 9
- **Platform:** 64-bit
- **Managed Pipeline:** Integrated

### Deployment Configuration

Deployments use the `Production` slot directly. For zero-downtime deployments, consider:
- Adding a staging slot
- Implementing slot swapping

---

## Database Configuration

### Connection Configuration

The database connection is configured in `Program.cs` with resilience features:

```csharp
builder.Services.AddDbContext<DataContext>(p =>
    p.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            .EnableRetryOnFailure(6, TimeSpan.FromSeconds(15), null)
        )
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));
```

### Configuration Options

| Option | Value | Description |
|--------|-------|-------------|
| **QuerySplittingBehavior** | `SplitQuery` | Splits queries with multiple includes to avoid cartesian explosion |
| **EnableRetryOnFailure** | 6 retries, 15s delay | Automatic retry on transient failures |
| **EnableSensitiveDataLogging** | `Development only` | Disabled in production for security |

### Connection String Format

```
Server=tcp:your-server.database.windows.net,1433;
Initial Catalog=SFManagement;
Persist Security Info=False;
User ID=your-user;
Password=your-password;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

### Retry on Failure

The retry policy handles transient Azure SQL failures:

```csharp
.EnableRetryOnFailure(
    maxRetryCount: 6,              // Maximum retry attempts
    maxRetryDelay: TimeSpan.FromSeconds(15),  // Max delay between retries
    errorNumbersToAdd: null        // Use default transient errors
)
```

**Transient errors handled:**
- Network connectivity issues
- SQL Server throttling
- Temporary server unavailability

---

## Health Checks

### Health Check Endpoint

The application exposes a health check endpoint for monitoring:

```
GET /health
```

This endpoint:
- Is **publicly accessible** (AllowAnonymous)
- Returns HTTP 200 when healthy
- Returns HTTP 503 when unhealthy

### Health Check Configuration

Health checks are configured in `DependencyInjectionExtensions.cs`:

```csharp
public static void AddHealthCheckServices(this WebApplicationBuilder builder)
{
    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}
```

### What's Checked

| Check | Description |
|-------|-------------|
| **SQL Server** | Verifies database connectivity |

### Registration in Pipeline

```csharp
// Program.cs
app.MapHealthChecks("/health").AllowAnonymous();
```

### Azure Integration

Configure Azure Web App health checks to use this endpoint:
1. Go to Web App > **Health check** in Azure Portal
2. Enable health check
3. Set path to `/health`
4. Configure check interval and failure threshold

---

## Environment Configuration

### Required App Settings

Configure these settings in Azure Web App > **Configuration** > **Application settings**:

| Setting | Description | Example |
|---------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | `Server=...` |
| `Auth0__Domain` | Auth0 tenant domain | `your-tenant.auth0.com` |
| `Auth0__Audience` | Auth0 API identifier | `https://api.semprefichas.com.br` |
| `Auth0__ClientId` | Auth0 application client ID | `abc123...` |
| `Auth0__ClientSecret` | Auth0 application secret | `xyz789...` |
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Production` or `Staging` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Application Insights telemetry | `InstrumentationKey=...` |

### Environment-Specific Settings

| Setting | Production | Staging |
|---------|------------|---------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | `Staging` |
| `EnableDetailedLogging` | `false` | `true` |
| Serilog MinimumLevel | `Warning` | `Information` |

### Configuration Hierarchy

Azure Web App settings override `appsettings.json`:

```
1. appsettings.json (base)
2. appsettings.{Environment}.json (environment-specific)
3. Environment variables (Azure App Settings)
4. User Secrets (development only)
```

---

## Monitoring

### Application Insights Integration

The application uses Serilog and should send production telemetry to Application Insights:

```csharp
// In appsettings.json or via NuGet package
"WriteTo": [
  {
    "Name": "ApplicationInsights",
    "Args": {
      "connectionString": "InstrumentationKey=..."
    }
  }
]
```

### Metrics to Monitor

| Metric | Description | Alert Threshold |
|--------|-------------|-----------------|
| **Response Time** | Average request duration | > 2 seconds |
| **Failed Requests** | HTTP 5xx errors | > 1% |
| **CPU Usage** | App Service CPU | > 80% |
| **Memory Usage** | App Service memory | > 80% |
| **Database DTU** | SQL Database utilization | > 80% |

### Log Analytics

Production logging writes to:
- **Console** - Visible in Azure Log Stream
- **Application Insights** - Queryable telemetry and alerts

### Azure Log Stream

View real-time logs:
1. Go to Web App in Azure Portal
2. Select **Log stream** under Monitoring
3. Logs appear in real-time

---

## Security

### Managed Identity

Use Managed Identity for secure access to Azure resources without credentials:

```csharp
// Connection string with Managed Identity
Server=tcp:your-server.database.windows.net,1433;
Initial Catalog=SFManagement;
Authentication=Active Directory Managed Identity;
```

### Key Vault Integration

Store secrets in Azure Key Vault and reference them:

1. **Create Key Vault reference:**
   ```
   @Microsoft.KeyVault(SecretUri=https://your-vault.vault.azure.net/secrets/SecretName/)
   ```

2. **Add in App Settings:**
   ```
   Auth0__ClientSecret = @Microsoft.KeyVault(SecretUri=...)
   ```

### Network Security

- **HTTPS Only:** Enable "HTTPS Only" in TLS/SSL settings
- **Minimum TLS Version:** Set to TLS 1.2
- **IP Restrictions:** Configure allowed IP ranges if needed
- **Virtual Network:** Consider VNet integration for private connectivity

### Authentication

- **Auth0 Integration:** All API endpoints require authentication
- **CORS:** Configure allowed origins
- **Rate Limiting:** IP-based rate limiting enabled

---

## Scaling

### Vertical Scaling (Scale Up)

Increase resources by changing App Service Plan tier:

| Tier | vCPUs | Memory | Use Case |
|------|-------|--------|----------|
| Basic | 1 | 1.75 GB | Development/Testing |
| Standard | 1-4 | 1.75-7 GB | Production (small) |
| Premium | 1-8 | 3.5-14 GB | Production (medium) |
| Premium V3 | 2-32 | 8-128 GB | Production (large) |

### Horizontal Scaling (Scale Out)

Add more instances to handle increased load:

1. **Manual scaling:** Set instance count in Azure Portal
2. **Auto-scaling:** Configure rules based on metrics

**Example auto-scale rule:**
- Scale out when CPU > 70% for 10 minutes
- Scale in when CPU < 30% for 10 minutes
- Minimum instances: 2
- Maximum instances: 10

### Database Scaling

**SQL Database tiers:**

| Tier | DTUs/vCores | Use Case |
|------|-------------|----------|
| Basic | 5 DTUs | Development |
| Standard | 10-3000 DTUs | Production (small-medium) |
| Premium | 125-4000 DTUs | Production (high performance) |
| Serverless | Auto-scale | Variable workloads |

---

## Deployment Considerations

### Pre-Deployment Checklist

- [ ] Verify connection string is correct
- [ ] Confirm all environment variables are set
- [ ] Check Auth0 configuration matches environment
- [ ] Verify health check endpoint works locally
- [ ] Review database migrations

### Post-Deployment Verification

1. **Check health endpoint:**
   ```bash
   curl https://your-app.azurewebsites.net/health
   ```

2. **Verify Swagger UI loads:**
   ```
   https://your-app.azurewebsites.net/
   ```

3. **Test authentication flow:**
   - Obtain token from Auth0
   - Make authenticated API request
   - Verify response

4. **Check logs:**
   - Review Azure Log Stream
   - Check for any startup errors
   - Verify database connectivity

---

## Cost Optimization

### Recommendations

1. **Right-size resources** - Start small and scale based on actual usage
2. **Use reserved instances** - 1-3 year reservations for predictable workloads
3. **Auto-scaling** - Scale down during off-peak hours
4. **Staging environment** - Use smaller tier for non-production
5. **SQL elastic pools** - Share resources across multiple databases

### Cost Monitoring

- Set up Azure Cost Management alerts
- Review monthly cost reports
- Tag resources for cost allocation

---

## References

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [Azure Key Vault Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [Health Checks in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

