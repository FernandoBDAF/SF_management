# Configuration Management

This document describes how configuration is managed in the SF Management API, including application settings, environment variables, user secrets, and dependency management.

---

## Table of Contents

- [Overview](#overview)
- [Configuration Hierarchy](#configuration-hierarchy)
- [Configuration Files](#configuration-files)
- [Configuration Sections](#configuration-sections)
- [User Secrets](#user-secrets)
- [Environment Variables](#environment-variables)
- [SDK Configuration](#sdk-configuration)
- [Dependencies](#dependencies)
- [Development Profiles](#development-profiles)
- [Culture and Localization](#culture-and-localization)
- [Best Practices](#best-practices)

---

## Overview

The SF Management API uses ASP.NET Core's configuration system, which supports multiple configuration sources that can be layered and overridden. This provides flexibility for different environments while keeping sensitive data secure.

---

## Configuration Hierarchy

Configuration sources are loaded in order, with later sources overriding earlier ones:

```
┌─────────────────────────────────────────────────────────────┐
│                    5. Environment Variables                  │  ◄── Highest Priority
│                       (Azure App Settings)                   │
├─────────────────────────────────────────────────────────────┤
│                    4. User Secrets                           │
│                    (Development only)                        │
├─────────────────────────────────────────────────────────────┤
│               3. appsettings.{Environment}.json              │
│                    (Environment-specific)                    │
├─────────────────────────────────────────────────────────────┤
│                    2. appsettings.json                       │
│                       (Base settings)                        │
├─────────────────────────────────────────────────────────────┤
│                    1. Default Values                         │  ◄── Lowest Priority
│                       (Code defaults)                        │
└─────────────────────────────────────────────────────────────┘
```

---

## Configuration Files

### appsettings.json

**Location:** Project root

**Purpose:** Base configuration for all environments

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.Authentication": "Debug",
      "SFManagement.Authorization": "Debug",
      "SFManagement.Services": "Information",
      "SFManagement.Middleware.RequestResponseLoggingMiddleware": "Information",
      "SFManagement.ErrorHandlerMiddleware": "Information"
    }
  },
  "AllowedHosts": "*",
  "EnableDetailedLogging": true,
  "Auth0": {
    "Domain": "your-domain.auth0.com",
    "Audience": "https://your-api-identifier",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  },
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File",
      "Serilog.Sinks.Seq"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "SFManagement.Middleware.RequestResponseLoggingMiddleware": "Information",
        "SFManagement.ErrorHandlerMiddleware": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/sf-management-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341",
          "apiKey": "your-seq-api-key",
          "restrictedToMinimumLevel": "Information"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "SF Management API",
      "Environment": "Development"
    }
  },
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": true,
    "RealIpHeader": "X-Real-IP",
    "HttpStatusCode": 429,
    "IpWhitelist": ["127.0.0.1"],
    "EndpointWhitelist": [],
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "60s",
        "Limit": 20
      }
    ]
  }
}
```

### global.json

**Location:** Project root

**Purpose:** SDK version configuration

```json
{
  "sdk": {
    "version": "9.0.0",
    "rollForward": "latestMajor",
    "allowPrerelease": true
  }
}
```

| Setting | Value | Description |
|---------|-------|-------------|
| `version` | `9.0.0` | Target SDK version |
| `rollForward` | `latestMajor` | Use latest major version if exact not found |
| `allowPrerelease` | `true` | Allow prerelease SDK versions |

### launchSettings.json

**Location:** `Properties/launchSettings.json`

**Purpose:** Development launch configuration

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "dotnetRunMessages": true,
      "applicationUrl": "https://localhost:7078;http://localhost:5242"
    },
    "https": {
      "commandName": "Project",
      "launchBrowser": true,
      "launchUrl": "",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "dotnetRunMessages": true,
      "applicationUrl": "https://localhost:7078;http://localhost:5242"
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## Configuration Sections

### ConnectionStrings

Database connection configuration.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SFManagement;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Usage in code:**
```csharp
builder.Configuration.GetConnectionString("DefaultConnection")
```

### Auth0

Authentication provider configuration.

```json
{
  "Auth0": {
    "Domain": "your-tenant.auth0.com",
    "Audience": "https://your-api-identifier",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

| Setting | Description |
|---------|-------------|
| `Domain` | Auth0 tenant domain (e.g., `tenant.auth0.com`) |
| `Audience` | API identifier registered in Auth0 |
| `ClientId` | Application client ID |
| `ClientSecret` | Application client secret (keep secure!) |

**Strongly-typed access:**
```csharp
// Settings/Auth0Settings.cs
public class Auth0Settings
{
    public string? Domain { get; set; }
    public string? Audience { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}

// Registration
builder.Services.Configure<Auth0Settings>(builder.Configuration.GetSection("Auth0"));
```

### Logging

Built-in .NET logging configuration.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.Authentication": "Debug",
      "SFManagement.Authorization": "Debug",
      "SFManagement.Services": "Information"
    }
  }
}
```

**Log levels (least to most verbose):**
- `None` - No logging
- `Critical` - Failures requiring immediate attention
- `Error` - Errors and exceptions
- `Warning` - Abnormal or unexpected events
- `Information` - General flow information
- `Debug` - Detailed debugging information
- `Trace` - Most detailed information

### Serilog

Structured logging with multiple sinks.

**Sinks configured:**
| Sink | Purpose |
|------|---------|
| `Console` | Development output |
| `File` | Daily rolling log files |
| `Seq` | Log aggregation server |

**Console output template:**
```
[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}
```

**File configuration:**
| Setting | Value | Description |
|---------|-------|-------------|
| `path` | `logs/sf-management-.log` | Log file path |
| `rollingInterval` | `Day` | New file each day |
| `retainedFileCountLimit` | `30` | Keep last 30 files |

**Enrichers:**
- `FromLogContext` - Include log context properties
- `WithMachineName` - Include server name
- `WithThreadId` - Include thread ID

### IpRateLimiting

Request rate limiting configuration.

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": true,
    "RealIpHeader": "X-Real-IP",
    "HttpStatusCode": 429,
    "IpWhitelist": ["127.0.0.1"],
    "EndpointWhitelist": [],
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "60s",
        "Limit": 20
      }
    ]
  }
}
```

| Setting | Value | Description |
|---------|-------|-------------|
| `EnableEndpointRateLimiting` | `true` | Enable per-endpoint limiting |
| `StackBlockedRequests` | `true` | Count blocked requests |
| `RealIpHeader` | `X-Real-IP` | Header for real client IP |
| `HttpStatusCode` | `429` | Response code when limited |
| `IpWhitelist` | `["127.0.0.1"]` | IPs exempt from limiting |
| `GeneralRules` | 20 per 60s | Default rate limit |

### Application Settings

Other application-specific settings.

| Setting | Type | Description |
|---------|------|-------------|
| `AllowedHosts` | `string` | Allowed host headers (`*` for all) |
| `EnableDetailedLogging` | `bool` | Enable request/response logging |

---

## User Secrets

For local development, use .NET User Secrets to avoid committing sensitive data.

### Setup

```bash
# Initialize user secrets (one-time)
dotnet user-secrets init

# User Secrets ID is stored in .csproj
# <UserSecretsId>ed746e9e-1446-47fe-a708-fc3380b65b06</UserSecretsId>
```

### Setting Secrets

```bash
# Set individual secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=SFManagement;..."
dotnet user-secrets set "Auth0:Domain" "your-tenant.auth0.com"
dotnet user-secrets set "Auth0:ClientId" "your-client-id"
dotnet user-secrets set "Auth0:ClientSecret" "your-client-secret"
dotnet user-secrets set "Auth0:Audience" "https://your-api-identifier"
```

### Viewing Secrets

```bash
# List all secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "Auth0:ClientSecret"

# Clear all secrets
dotnet user-secrets clear
```

### Storage Location

Secrets are stored outside the project:
- **Windows:** `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- **macOS/Linux:** `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

---

## Environment Variables

For production deployments, use environment variables.

### Naming Convention

Use double underscore (`__`) to represent nested configuration:

```bash
# Connection string
ConnectionStrings__DefaultConnection="Server=..."

# Auth0 settings
Auth0__Domain="tenant.auth0.com"
Auth0__Audience="https://api.example.com"
Auth0__ClientId="abc123"
Auth0__ClientSecret="xyz789"

# Application settings
ASPNETCORE_ENVIRONMENT="Production"
EnableDetailedLogging="false"
```

### Azure App Settings

In Azure Web App, add application settings:

| Name | Value |
|------|-------|
| `ConnectionStrings__DefaultConnection` | SQL connection string |
| `Auth0__Domain` | `your-tenant.auth0.com` |
| `Auth0__Audience` | `https://your-api-identifier` |
| `Auth0__ClientId` | `your-client-id` |
| `Auth0__ClientSecret` | `your-client-secret` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

### Required Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Yes | Database connection |
| `Auth0__Domain` | Yes | Auth0 tenant |
| `Auth0__Audience` | Yes | API identifier |
| `Auth0__ClientId` | Yes | Client ID |
| `Auth0__ClientSecret` | Yes | Client secret |
| `ASPNETCORE_ENVIRONMENT` | Yes | Environment name |
| `EnableDetailedLogging` | No | Enable verbose logging |

---

## SDK Configuration

### global.json

Controls which .NET SDK version is used:

```json
{
  "sdk": {
    "version": "9.0.0",
    "rollForward": "latestMajor",
    "allowPrerelease": true
  }
}
```

### Roll Forward Policies

| Policy | Description |
|--------|-------------|
| `patch` | Use latest patch version |
| `feature` | Use latest feature band |
| `minor` | Use latest minor version |
| `major` | Use latest major version |
| `latestPatch` | Use latest available patch |
| `latestFeature` | Use latest available feature |
| `latestMinor` | Use latest available minor |
| `latestMajor` | Use latest available major |
| `disable` | Exact version only |

---

## Dependencies

### Package References

All NuGet packages are defined in `SFManagement.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
    <PropertyGroup>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <TargetFramework>net9.0</TargetFramework>
        <UserSecretsId>ed746e9e-1446-47fe-a708-fc3380b65b06</UserSecretsId>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="AspNetCore.HealthChecks.SqlServer" Version="9.0.0" />
        <PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />
        <PackageReference Include="Auth0.AspNetCore.Authentication" Version="1.4.1" />
        <PackageReference Include="AutoMapper" Version="13.0.1" />
        <PackageReference Include="EntityFramework" Version="6.5.1" />
        <PackageReference Include="EPPlus" Version="7.3.0" />
        <PackageReference Include="FluentValidation" Version="11.9.2" />
        <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
        <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.7" />
        <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.7" />
        <PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning" Version="5.1.0" />
        <PackageReference Include="Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer" Version="5.1.0" />
        <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.7" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.7" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.7" />
        <PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
        <PackageReference Include="Serilog.Sinks.Seq" Version="6.0.0" />
        <PackageReference Include="Swashbuckle.AspNetCore" Version="6.3.1" />
        <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.1" />
    </ItemGroup>
</Project>
```

### Package Categories

| Category | Packages |
|----------|----------|
| **Framework** | `net9.0` target |
| **ORM** | `EntityFramework`, `Microsoft.EntityFrameworkCore.*` |
| **Authentication** | `Auth0.*`, `Microsoft.AspNetCore.Authentication.JwtBearer` |
| **Validation** | `FluentValidation.*` |
| **Mapping** | `AutoMapper` |
| **Logging** | `Serilog.*` |
| **API Documentation** | `Swashbuckle.AspNetCore` |
| **Rate Limiting** | `AspNetCoreRateLimit` |
| **Excel Processing** | `EPPlus` |
| **Health Checks** | `AspNetCore.HealthChecks.SqlServer` |

### Updating Dependencies

```bash
# Check for outdated packages
dotnet list package --outdated

# Update specific package
dotnet add package PackageName --version X.Y.Z

# Restore packages
dotnet restore
```

---

## Development Profiles

### Available Profiles

| Profile | Command | URL | Environment |
|---------|---------|-----|-------------|
| `http` | `dotnet run` | `https://localhost:7078` | Development |
| `https` | `dotnet run --launch-profile https` | `https://localhost:7078` | Development |
| `IIS Express` | Visual Studio | `http://localhost:5108` | Development |

### Running with Specific Profile

```bash
# Default profile
dotnet run

# Specific profile
dotnet run --launch-profile https

# Specific environment
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

### Development URLs

| Protocol | Port | URL |
|----------|------|-----|
| HTTPS | 7078 | `https://localhost:7078` |
| HTTP | 5242 | `http://localhost:5242` |
| IIS Express HTTP | 5108 | `http://localhost:5108` |
| IIS Express HTTPS | 44375 | `https://localhost:44375` |

---

## Culture and Localization

The application is configured for Brazilian Portuguese (pt-BR).

### Configuration in Program.cs

```csharp
// Request localization options
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = new List<CultureInfo> { new("pt-BR") };
    options.RequestCultureProviders.Clear();
});

// Thread culture settings
var cultureInfo = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
CultureInfo.CurrentCulture = cultureInfo;
CultureInfo.CurrentUICulture = cultureInfo;

// Request localization middleware
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultureInfo),
    SupportedCultures = new List<CultureInfo> { cultureInfo },
    SupportedUICultures = new List<CultureInfo> { cultureInfo }
});
```

### Impact

- **Number formatting:** Uses Brazilian format (1.234,56 instead of 1,234.56)
- **Date formatting:** Uses Brazilian format (dd/MM/yyyy)
- **Currency:** Brazilian Real (R$)

---

## Best Practices

### Security

1. **Never commit secrets** - Use User Secrets or environment variables
2. **Rotate credentials** - Regularly update passwords and API keys
3. **Use Key Vault** - Store production secrets in Azure Key Vault
4. **Audit access** - Review who has access to configuration

### Environment Management

1. **Environment-specific settings** - Use `appsettings.{Environment}.json`
2. **Override in production** - Use environment variables in Azure
3. **Validate configuration** - Check required settings at startup
4. **Document requirements** - Keep this documentation updated

### Configuration Loading

```csharp
// Validate required configuration at startup
var auth0Config = builder.Configuration.GetSection("Auth0");
if (string.IsNullOrEmpty(auth0Config["Domain"]))
{
    throw new InvalidOperationException("Auth0:Domain is required");
}
```

### Dependency Management

1. **Pin versions** - Specify exact package versions
2. **Regular updates** - Check for security updates
3. **Test upgrades** - Test in staging before production
4. **Document changes** - Track dependency updates

---

## References

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [User Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Environment Variables](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments)
- [Serilog Configuration](https://github.com/serilog/serilog-settings-configuration)
- [AspNetCoreRateLimit](https://github.com/stefanprodan/AspNetCoreRateLimit)

