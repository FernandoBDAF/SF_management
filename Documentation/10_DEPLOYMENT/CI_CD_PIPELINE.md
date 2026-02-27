# CI/CD Pipeline

This document describes the Continuous Integration and Continuous Deployment (CI/CD) pipeline for the SF Management API. The system uses GitHub Actions to automate building and deploying to Azure Web Apps.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Environments](#environments)
- [Workflow Configuration](#workflow-configuration)
- [Build Stage](#build-stage)
- [Deploy Stage](#deploy-stage)
- [Azure OIDC Authentication](#azure-oidc-authentication)
- [Database Migrations](#database-migrations)
- [Manual Deployment](#manual-deployment)
- [Rollback Procedures](#rollback-procedures)
- [Best Practices](#best-practices)

---

## Overview

The SF Management API uses a two-environment deployment strategy with separate workflows for staging (HMG) and production environments. Each environment has its own:

- Git branch (`hmg` or `main`)
- GitHub Actions workflow file
- Azure Web App instance
- Azure credentials

```
┌─────────────────────────────────────────────────────────────────┐
│                      GitHub Repository                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────────┐                      ┌─────────────┐         │
│   │  hmg branch │                      │ main branch │         │
│   └──────┬──────┘                      └──────┬──────┘         │
│          │                                    │                 │
│          ▼                                    ▼                 │
│   ┌──────────────┐                    ┌──────────────┐         │
│   │ GitHub Action│                    │ GitHub Action│         │
│   │  (Staging)   │                    │ (Production) │         │
│   └──────┬───────┘                    └──────┬───────┘         │
│          │                                    │                 │
└──────────┼────────────────────────────────────┼─────────────────┘
           │                                    │
           ▼                                    ▼
┌──────────────────────┐            ┌──────────────────────┐
│   Azure Web App      │            │   Azure Web App      │
│ sfmanagement-api-hmg │            │   sfmanagement-api   │
│     (Staging)        │            │    (Production)      │
└──────────────────────┘            └──────────────────────┘
```

---

## Architecture

### Pipeline Stages

Both pipelines follow a two-stage process:

```
┌─────────────────────────────────────────────────────────────────┐
│                         BUILD STAGE                             │
├─────────────────────────────────────────────────────────────────┤
│  1. Checkout code from repository                               │
│  2. Setup .NET 9.x SDK                                          │
│  3. Build with Release configuration                            │
│  4. Publish application to output directory                     │
│  5. Upload build artifacts                                      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        DEPLOY STAGE                             │
├─────────────────────────────────────────────────────────────────┤
│  1. Download build artifacts                                    │
│  2. Login to Azure using OIDC federation                        │
│  3. Deploy to Azure Web App                                     │
└─────────────────────────────────────────────────────────────────┘
```

### Workflow Files

```
.github/
└── workflows/
    ├── hmg_sfmanagement-api-hmg.yml   # Staging deployment
    └── main_sfmanagement-api.yml       # Production deployment
```

---

## Environments

| Environment | Branch | Azure Resource | Purpose |
|-------------|--------|----------------|---------|
| **Staging (HMG)** | `hmg` | `sfmanagement-api-hmg` | Testing and validation |
| **Production** | `main` | `sfmanagement-api` | Live production system |

### Environment Differences

| Aspect | Staging | Production |
|--------|---------|------------|
| Workflow File | `hmg_sfmanagement-api-hmg.yml` | `main_sfmanagement-api.yml` |
| Trigger Branch | `hmg` | `main` |
| App Name | `sfmanagement-api-hmg` | `sfmanagement-api` |
| Artifact Retention | 1 day | Default (90 days) |
| Prerelease SDK | Yes | No |

---

## Workflow Configuration

### Production Workflow

**File:** `.github/workflows/main_sfmanagement-api.yml`

```yaml
name: Build and deploy ASP.Net Core app to Azure Web App - sfmanagement-api

on:
  push:
    branches:
      - main
  workflow_dispatch:

jobs:
  build:
    runs-on: windows-latest
    permissions:
      contents: read

    steps:
      - uses: actions/checkout@v4

      - name: Set up .NET Core
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.x'

      - name: Build with dotnet
        run: dotnet build SFManagement.csproj --configuration Release

      - name: dotnet publish
        run: dotnet publish SFManagement.csproj -c Release -o "${{env.DOTNET_ROOT}}/myapp"

      - name: Upload artifact for deployment job
        uses: actions/upload-artifact@v4
        with:
          name: .net-app
          path: ${{env.DOTNET_ROOT}}/myapp

  deploy:
    runs-on: windows-latest
    needs: build
    environment:
      name: 'Production'
      url: ${{ steps.deploy-to-webapp.outputs.webapp-url }}
    permissions:
      id-token: write
      contents: read

    steps:
      - name: Download artifact from build job
        uses: actions/download-artifact@v4
        with:
          name: .net-app
      
      - name: Login to Azure
        uses: azure/login@v2
        with:
          client-id: ${{ secrets.AZUREAPPSERVICE_CLIENTID_* }}
          tenant-id: ${{ secrets.AZUREAPPSERVICE_TENANTID_* }}
          subscription-id: ${{ secrets.AZUREAPPSERVICE_SUBSCRIPTIONID_* }}

      - name: Deploy to Azure Web App
        id: deploy-to-webapp
        uses: azure/webapps-deploy@v3
        with:
          app-name: 'sfmanagement-api'
          slot-name: 'Production'
          package: .
```

### Staging Workflow

**File:** `.github/workflows/hmg_sfmanagement-api-hmg.yml`

Key differences from production:
- Triggers on `hmg` branch
- Uses `include-prerelease: true` for .NET SDK
- Artifact retention set to 1 day
- Deploys to `sfmanagement-api-hmg`
- Also targets `SFManagement.csproj` explicitly in build and publish steps

---

## Build Stage

### Build Runner

- **Operating System:** Windows (windows-latest)
- **SDK:** .NET 9.x
- **Configuration:** Release

### Build Steps

1. **Checkout Code**
   ```yaml
   - uses: actions/checkout@v4
   ```

2. **Setup .NET SDK**
   ```yaml
   - name: Set up .NET Core
     uses: actions/setup-dotnet@v4
     with:
       dotnet-version: '9.x'
   ```

3. **Build Application**
   ```yaml
   - name: Build with dotnet
     run: dotnet build SFManagement.csproj --configuration Release
   ```

4. **Publish Application**
   ```yaml
   - name: dotnet publish
     run: dotnet publish SFManagement.csproj -c Release -o "${{env.DOTNET_ROOT}}/myapp"
   ```

> **Note:** Both build and publish target `SFManagement.csproj` explicitly (not the solution file) to avoid build failures from stale or missing project references in `SFManagement.sln`.

5. **Upload Artifacts**
   ```yaml
   - name: Upload artifact for deployment job
     uses: actions/upload-artifact@v4
     with:
       name: .net-app
       path: ${{env.DOTNET_ROOT}}/myapp
   ```

### Artifact Management

| Environment | Artifact Name | Retention |
|-------------|---------------|-----------|
| Production | `.net-app` | 90 days (default) |
| Staging | `.net-app` | 1 day |

---

## Deploy Stage

### Prerequisites

The deploy job requires:
- Successful completion of the build job (`needs: build`)
- `id-token: write` permission for OIDC authentication
- Valid Azure credentials in GitHub Secrets

### Deployment Steps

1. **Download Artifacts**
   ```yaml
   - name: Download artifact from build job
     uses: actions/download-artifact@v4
     with:
       name: .net-app
   ```

2. **Azure Login**
   ```yaml
   - name: Login to Azure
     uses: azure/login@v2
     with:
       client-id: ${{ secrets.AZUREAPPSERVICE_CLIENTID_* }}
       tenant-id: ${{ secrets.AZUREAPPSERVICE_TENANTID_* }}
       subscription-id: ${{ secrets.AZUREAPPSERVICE_SUBSCRIPTIONID_* }}
   ```

3. **Deploy to Web App**
   ```yaml
   - name: Deploy to Azure Web App
     uses: azure/webapps-deploy@v3
     with:
       app-name: 'sfmanagement-api'
       slot-name: 'Production'
       package: .
   ```

---

## Azure OIDC Authentication

The pipelines use **Azure OIDC (OpenID Connect)** federated credentials for secure, secretless authentication.

### Benefits

- **No long-lived credentials** - Tokens are requested at runtime
- **Secure** - Uses GitHub's OIDC provider
- **Scoped** - Limited to specific Azure subscriptions and resources

### Required GitHub Secrets

| Secret | Purpose | Example Format |
|--------|---------|----------------|
| `AZUREAPPSERVICE_CLIENTID_*` | Azure AD application (client) ID | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |
| `AZUREAPPSERVICE_TENANTID_*` | Azure AD tenant ID | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |
| `AZUREAPPSERVICE_SUBSCRIPTIONID_*` | Azure subscription ID | `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx` |

> **Note:** Each environment (Production/Staging) has its own set of secrets with unique suffixes.

### Permissions Required

```yaml
permissions:
  id-token: write   # Required for requesting the JWT
  contents: read    # Required for actions/checkout
```

---

## Database Migrations

### Automatic Migration Strategy

Database migrations run automatically when the application starts. This is configured in `Program.cs`:

```csharp
using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    serviceScope.ServiceProvider.GetService<DataContext>().Database.Migrate();
}
```

### How It Works

1. Application starts after deployment
2. Entity Framework Core checks for pending migrations
3. Any new migrations are applied to the database
4. Application continues normal startup

### Considerations

- **Simple changes**: Automatic migration works well for additive changes (new tables, columns)
- **Breaking changes**: For destructive changes, consider manual migration before deployment
- **Rollback**: Database migrations are not automatically rolled back on deployment failure

### Manual Migration (When Needed)

For breaking schema changes, run migrations manually before deployment:

```bash
# Generate SQL script for review
dotnet ef migrations script --idempotent

# Apply specific migration
dotnet ef database update <MigrationName>
```

---

## Manual Deployment

### Triggering a Manual Deployment

1. Go to the repository's **Actions** tab in GitHub
2. Select the appropriate workflow:
   - `Build and deploy ASP.Net Core app to Azure Web App - sfmanagement-api` (Production)
   - `Build and deploy ASP.Net Core app to Azure Web App - sfmanagement-api-hmg` (Staging)
3. Click **Run workflow**
4. Select the branch (should match the workflow's target branch)
5. Click the green **Run workflow** button
6. Monitor the workflow execution

### Workflow Dispatch

Both workflows support manual triggering via `workflow_dispatch`:

```yaml
on:
  push:
    branches:
      - main  # or hmg
  workflow_dispatch:  # Enables manual trigger
```

---

## Rollback Procedures

### Option 1: Azure Portal Rollback

1. Navigate to the Azure Portal
2. Go to your Web App resource
3. Select **Deployment Center** > **Logs**
4. Find a previous successful deployment
5. Click **Redeploy**

### Option 2: GitHub Actions Re-run

1. Go to **Actions** tab in GitHub
2. Find a previous successful workflow run
3. Click **Re-run all jobs**

### Option 3: Git Revert

1. Revert the problematic commit:
   ```bash
   git revert <commit-hash>
   git push origin main  # or hmg
   ```
2. This triggers a new deployment with the reverted code

### Database Rollback Considerations

- Entity Framework migrations are **not** automatically rolled back
- For database rollback, you may need to:
  1. Create a new migration that undoes changes
  2. Restore from a database backup
  3. Manually run rollback scripts

---

## Best Practices

### Pre-Deployment

1. **Always deploy to staging first** - Validate changes in HMG before production
2. **Use pull requests** - Merge to `main` via PR for code review
3. **Review migration scripts** - Check for potentially destructive changes
4. **Test locally** - Run the application locally before pushing

### During Deployment

1. **Monitor workflow execution** - Watch for build or deploy failures
2. **Check Azure portal** - Verify deployment status in Deployment Center
3. **Verify health endpoint** - Confirm `/health` returns 200 after deployment

### Post-Deployment

1. **Smoke test** - Verify critical functionality works
2. **Check logs** - Review application logs for errors
3. **Monitor performance** - Watch for any performance degradation

### Branch Protection

Consider enabling branch protection rules for `main` and `hmg`:

- Require pull request reviews
- Require status checks to pass
- Require branches to be up to date
- Restrict force pushes

### Secret Management

- Rotate Azure credentials periodically
- Use different credentials for each environment
- Never commit secrets to the repository
- Audit secret access regularly

---

## Troubleshooting

### Common Issues

**Build fails with SDK version error:**
- Ensure `global.json` specifies a compatible SDK version
- Check that the workflow uses the correct .NET version

**Deploy fails with authentication error:**
- Verify GitHub Secrets are correctly configured
- Check that OIDC federation is set up in Azure
- Ensure the service principal has required permissions

**Application fails to start after deployment:**
- Check Application Insights or Serilog logs
- Verify environment variables are configured in Azure
- Check database connectivity
- Look for migration errors

**Health check fails:**
- Verify database connection string
- Check SQL Server is accessible from Azure Web App
- Review startup logs for configuration errors

---

## References

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure Web Apps Deploy Action](https://github.com/Azure/webapps-deploy)
- [Azure Login Action](https://github.com/Azure/login)
- [OIDC Federation with Azure](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-azure)

