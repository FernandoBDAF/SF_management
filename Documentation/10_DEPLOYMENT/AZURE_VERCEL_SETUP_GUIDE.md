# Azure & Vercel Setup Guide

Step-by-step instructions to configure all cloud services that the codebase depends on. Each section has numbered steps and the exact values to use.

---

## Current Infrastructure Tier: Free F1

Both backend APIs (`sfmanagement-api` and `sfmanagement-api-hmg`) are currently running on **Azure App Service Free F1** tier. This guide is organized to reflect the current capabilities and future upgrade path.

### Free F1 Limitations

| Feature               | Free F1          | Impact                                                            |
| --------------------- | ---------------- | ----------------------------------------------------------------- |
| **Always On**         | ❌ Not available | App sleeps after ~20 min of inactivity, causing cold start delays |
| **Health Check**      | ❌ Not available | Cannot use Azure's built-in health monitoring                     |
| **Custom Domain SSL** | ❌ Not available | Limited to `*.azurewebsites.net` with Azure SSL                   |
| **Staging Slots**     | ❌ Not available | No zero-downtime deployments                                      |
| **CPU Time**          | 60 min/day       | After limit, app returns 403 until next day                       |
| **Auto-scale**        | ❌ Not available | Single instance only                                              |
| **Backups**           | ❌ Not available | Manual backups only via FTP/Kudu                                  |

### What Works on Free F1

- ✅ Application Insights (full telemetry)
- ✅ Application Settings / Environment Variables
- ✅ Availability Tests (ping from App Insights)
- ✅ Alert Rules (based on App Insights metrics)
- ✅ GitHub Actions CI/CD deployment
- ✅ Managed Identity (for Key Vault, if needed later)

---

## Table of Contents

### Phase 1: Free F1 Setup (Current)

1. [Azure: Create Application Insights](#1-azure-create-application-insights)
2. [Azure: Set App Settings](#2-azure-set-app-settings)
3. [Azure: Create Availability Tests](#3-azure-create-availability-tests)
4. [Azure: Create Alert Rules](#4-azure-create-alert-rules)
5. [Azure: Create Action Group](#5-azure-create-action-group)
6. [Azure: Database Long-Term Retention](#6-azure-database-long-term-retention)
7. [Sentry: Create Account and Project](#7-sentry-create-account-and-project)
8. [Vercel: Production Project Configuration](#8-vercel-production-project-configuration)
9. [Vercel: HMG Project Configuration](#9-vercel-hmg-project-configuration)
10. [Verification Checklist](#10-verification-checklist)

### Phase 2: Basic B1 Upgrade (Future)

11. [Upgrade Path: Basic B1](#11-upgrade-path-basic-b1)

### Phase 3: Production Tiers (Future)

12. [Upgrade Path: Standard/Premium](#12-upgrade-path-standard-premium)

---

# Phase 1: Free F1 Setup (Current)

## 1. Azure: Create Application Insights

Application Insights works fully on Free F1 and is essential for observability. Create one for each environment.

Both APIs share the same resource group. Create two Application Insights resources inside it.

### Production

1. Azure Portal -> **Create a resource** -> search "Application Insights"
2. Click **Create**
3. Configure:
   - **Name:** `sfmanagement-api-insights`
   - **Resource Group:** _(your existing shared resource group)_
   - **Region:** same as Web Apps
   - **Log Analytics Workspace:** create new or use existing (one workspace can serve both)
4. Click **Review + Create** -> **Create**
5. After creation, go to the resource -> **Overview** -> copy the **Connection String**

### HMG

1. Repeat the same steps within the same resource group:
   - **Name:** `sfmanagement-api-hmg-insights`
2. Copy the HMG connection string

> **Note:** Keeping both environments in a single resource group is fine at current scale. It simplifies management and cost visibility. Consider separating only if you later need distinct RBAC permissions per environment (e.g., restricting production access for new team members).

---

## 2. Azure: Set App Settings

### Production (`sfmanagement-api`)

1. Azure Portal -> **App Services** -> `sfmanagement-api`
2. **Settings** -> **Environment variables** (or Configuration -> Application settings)
3. Add or verify these settings:

| Name                                    | Value                                                       |
| --------------------------------------- | ----------------------------------------------------------- |
| `ASPNETCORE_ENVIRONMENT`                | `Production`                                                |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | _(paste the prod connection string from step 1)_            |
| `ConnectionStrings__DefaultConnection`  | _(your prod SQL connection string)_                         |
| `Auth0__Domain`                         | _(your Auth0 tenant domain, e.g. `semprefichas.auth0.com`)_ |
| `Auth0__Audience`                       | `https://api.semprefichas.com.br`                           |
| `Auth0__ClientId`                       | _(your Auth0 app client ID)_                                |
| `Auth0__ClientSecret`                   | _(your Auth0 app client secret)_                            |

4. Click **Apply** (this restarts the Web App)

### HMG (`sfmanagement-api-hmg`)

1. Azure Portal -> **App Services** -> `sfmanagement-api-hmg`
2. **Settings** -> **Environment variables**
3. Add or verify:

| Name                                    | Value                                           |
| --------------------------------------- | ----------------------------------------------- |
| `ASPNETCORE_ENVIRONMENT`                | `Staging`                                       |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | _(paste the HMG connection string from step 1)_ |
| `ConnectionStrings__DefaultConnection`  | _(your HMG SQL connection string)_              |
| `Auth0__Domain`                         | _(same Auth0 tenant domain)_                    |
| `Auth0__Audience`                       | `https://api.semprefichas.com.br`               |
| `Auth0__ClientId`                       | _(same or separate Auth0 app)_                  |
| `Auth0__ClientSecret`                   | _(same or separate Auth0 secret)_               |

4. Click **Apply**

---

## 3. Azure: Create Availability Tests

> **Note:** Azure's built-in Health Check feature is **not available on Free F1**. Instead, we use Application Insights Availability Tests to monitor endpoint availability. These tests ping your endpoints from multiple Azure regions and alert on failures.

### Production API

1. Azure Portal -> **Application Insights** -> `sfmanagement-api-insights`
2. **Investigate** -> **Availability**
3. Click **+ Add Standard test**
4. Configure:
   - **Name:** `SF API Prod - Health Check`
   - **URL:** `https://sfmanagement-api.azurewebsites.net/health`
   - **Test frequency:** `5 minutes`
   - **Test locations:** select at least 3 (e.g., East US, West Europe, Southeast Asia)
   - **Success criteria:** HTTP status code equals `200`
5. Under **Alerts**, enable alert:
   - **Alert when:** `2` of `3` test locations report failures
6. Click **Create**

### HMG API

1. Repeat in `sfmanagement-api-hmg-insights`:
   - **Name:** `SF API HMG - Health Check`
   - **URL:** `https://sfmanagement-api-hmg.azurewebsites.net/health`
   - Same frequency, locations, and alert config

### Production Frontend

1. In the same Application Insights resource (prod), add another test:
   - **Name:** `SF Frontend Prod`
   - **URL:** `https://sfmanagement.vercel.app`
   - **Frequency:** `5 minutes`
   - **Locations:** 3 regions
   - **Success criteria:** HTTP 200

### HMG Frontend

1. In HMG Application Insights:
   - **Name:** `SF Frontend HMG`
   - **URL:** `https://sf-management-hmg.vercel.app`
   - Same settings

---

## 4. Azure: Create Alert Rules

Create in the Application Insights resource for each environment.

### 4.1 High Error Rate

1. Application Insights -> **Monitoring** -> **Alerts** -> **+ Create alert rule**
2. **Condition:**
   - Signal name: `Failed requests`
   - **Alert logic:**
     - Aggregation type: `Count`
     - Operator: `Greater than`
     - Threshold: `5`
   - **When to evaluate:**
     - Check every: `1 minute`
     - Lookback period: `5 minutes`
3. **Actions:** select action group (create in step 5 if not yet created)
4. **Details:**
   - Severity: `1 - Error`
   - Alert rule name: `SF API [Prod/HMG] - High Error Rate`
5. Click **Review + create** -> **Create**

### 4.2 High Latency

1. New alert rule:
   - Signal name: `Server response time`
   - **Alert logic:**
     - Aggregation type: `Average`
     - Operator: `Greater than`
     - Threshold: `3000` (milliseconds)
   - **When to evaluate:**
     - Check every: `1 minute`
     - Lookback period: `5 minutes`
2. Severity: `2 - Warning`
3. Name: `SF API [Prod/HMG] - High Latency`

> **Note:** For P95 latency alerts, use a custom log query (see section 4.6).

### 4.3 Unhandled Exceptions

1. New alert rule:
   - Signal name: `Exceptions`
   - **Alert logic:**
     - Aggregation type: `Count`
     - Operator: `Greater than`
     - Threshold: `20`
   - **When to evaluate:**
     - Check every: `1 minute`
     - Lookback period: `10 minutes`
2. Severity: `2 - Warning`
3. Name: `SF API [Prod/HMG] - Exception Spike`

### 4.4 SQL DTU

1. Azure Portal -> **SQL databases** -> select production database
2. **Monitoring** -> **Alerts** -> **+ Create alert rule**
3. **Condition:**
   - Signal name: `DTU percentage`
   - **Alert logic:**
     - Aggregation type: `Average`
     - Operator: `Greater than`
     - Threshold: `80`
   - **When to evaluate:**
     - Check every: `5 minutes`
     - Lookback period: `10 minutes`
4. Severity: `2 - Warning`
5. Name: `SF SQL [Prod/HMG] - DTU High`

### 4.5 SQL Storage

1. Same SQL database -> new alert rule:
   - Signal name: `Data space used percent`
   - **Alert logic:**
     - Aggregation type: `Maximum`
     - Operator: `Greater than`
     - Threshold: `80`
   - **When to evaluate:**
     - Check every: `5 minutes`
     - Lookback period: `15 minutes`
2. Severity: `3 - Informational`
3. Name: `SF SQL [Prod/HMG] - Storage High`

### 4.6 (Optional) P95 Latency via Log Query

For percentile-based alerts, create a custom log alert:

1. Application Insights -> **Monitoring** -> **Alerts** -> **+ Create alert rule**
2. **Condition:** Click **See all signals** -> select `Custom log search`
3. **Log query:**
   ```kusto
   requests
   | where timestamp > ago(10m)
   | summarize p95 = percentile(duration, 95)
   | where p95 > 3000
   ```
4. **Alert logic:**
   - Measure: `Table rows`
   - Operator: `Greater than`
   - Threshold: `0`
5. **When to evaluate:**
   - Check every: `5 minutes`
   - Lookback period: `10 minutes`
6. Severity: `2 - Warning`
7. Name: `SF API [Prod/HMG] - P95 Latency High`

---

## 5. Azure: Create Action Group

1. Azure Portal -> **Monitor** -> **Alerts** -> **Action groups** -> **+ Create**
2. **Basics:**
   - **Action group name:** `SF Operations`
   - **Display name:** `SF Ops`
   - **Resource group:** same as production Web App
3. **Notifications:**
   - Type: `Email/SMS/Push/Voice`
   - Name: `Ops Email`
   - Email: _(your operations email address)_
4. **(Optional) Actions:**
   - Type: `Webhook`
   - Name: `Teams/Slack Webhook`
   - URI: _(your Teams or Slack incoming webhook URL)_
5. Click **Review + Create** -> **Create**
6. Go back to each alert rule and attach this action group

---

## 6. Azure: Database Long-Term Retention

Long-Term Retention (LTR) is configured at the **SQL Server** level, not the individual database.

1. Azure Portal -> **SQL servers** (not SQL databases)
2. Select your server (e.g., `semprefichas-dbserver`)
3. **Data management** -> **Backups**
4. Click **Retention policies** tab
5. Select the database checkbox (`sfmanagement-db`) -> click **Configure policies**
6. Configure:
   - **Point-in-time-restore:** `7` days (maximum for Basic tier)
   - **Differential backup frequency:** `12 Hours`
   - **Weekly LTR Backups:** `8` Week(s)
   - **Monthly LTR Backups:** `12` **Month(s)** ← _change dropdown from Week(s) to Month(s)_
   - **Yearly LTR Backups:** `0` (optional)
   - **LTR Immutability:** leave unchecked
7. Click **Apply**
8. Repeat for HMG database (optional, lower retention is fine)

> **Note:** Azure SQL Basic tier limits PITR to 7 days. LTR extends backup retention for compliance/disaster recovery needs.

---

## 7. Sentry: Create Account and Project

### 7.1 Create Account

1. Go to https://sentry.io/signup/
2. Sign up with your email or GitHub
3. Create an organization (e.g., `sempre-fichas`)

### 7.2 Create Project

1. After login -> **Projects** -> **Create Project**
2. Platform: **Next.js**
3. Project name: `sf-management-front`
4. Click **Create Project**
5. You'll see the "Configure Next.js SDK" page
6. Click **Copy DSN** button (top right) — save this value
7. **Skip the wizard command** — we already have Sentry configured in the codebase

> **Note:** The codebase already has Sentry configured via `instrumentation.ts`, `instrumentation-client.ts`, `sentry.server.config.ts`, and `sentry.edge.config.ts`. You just need the DSN and auth token.

### 7.3 Create Auth Token (for source maps)

1. Go to **Settings** (gear icon)
2. Under **Developer Settings** -> **Organization Tokens**
3. Click **Create New Token**
4. Name: `vercel-deploy`
5. Click **Create Token**
6. Copy the token immediately (it won't be shown again)

> **Note:** Organization Tokens have the necessary permissions for source map uploads built-in. No scope selection is needed.

### 7.4 Verify Alert Rules

Sentry creates default alerts automatically when you create a project.

1. Go to **Issues** (left sidebar) -> **Alerts**
2. Verify you see "Send a notification for high priority issues" for `sf-management-front`
3. This default alert is sufficient to start — it notifies you of high-priority errors via email

**(Optional)** To create additional alerts, click **Create Alert** and choose:

- **Issue Alert:** triggers on new issues, regressions, etc.
- **Metric Alert:** triggers when error count exceeds a threshold

### 7.5 Note Values for Vercel Config

You now have these values (save them for Vercel configuration):

| Value                    | Value                                   | Where to find                                                          |
| ------------------------ | --------------------------------------- | ---------------------------------------------------------------------- |
| `NEXT_PUBLIC_SENTRY_DSN` | `https://fddde038e10........`           | "Copy DSN" button on setup page, or Project -> Settings -> Client Keys |
| `SENTRY_AUTH_TOKEN`      | `sntrys_eyJpYXQiOjE3NzIzOTEyMjE.......` | Settings -> Auth Tokens (step 7.3)                                     |
| `SENTRY_ORG`             | `sempre-fichas`                         | Your organization slug (visible in URL)                                |
| `SENTRY_PROJECT`         | `sf-management-front`                   | Your project slug (visible in URL)                                     |

---

## 8. Vercel: Production Project Configuration

1. Go to https://vercel.com -> select the **sfmanagement** project
2. **Settings** -> **Environment Variables**
3. Set these variables (scope: **Production**):

| Variable                 | Value                                                             |
| ------------------------ | ----------------------------------------------------------------- |
| `NEXT_PUBLIC_API_URL`    | `https://sfmanagement-api.azurewebsites.net/api/v1`               |
| `AUTH_AUTH0_ID`          | _(your Auth0 client ID)_                                          |
| `AUTH_AUTH0_SECRET`      | _(your Auth0 client secret)_                                      |
| `AUTH_AUTH0_ISSUER`      | _(your Auth0 issuer URL, e.g., `https://semprefichas.auth0.com`)_ |
| `AUTH_AUTH0_AUDIENCE`    | `https://api.semprefichas.com.br`                                 |
| `AUTH_SECRET`            | _(generate: `openssl rand -base64 32`)_                           |
| `NEXT_PUBLIC_SENTRY_DSN` | _(DSN from step 7)_                                               |
| `SENTRY_AUTH_TOKEN`      | _(token from step 7.3)_                                           |
| `SENTRY_ORG`             | `sempre-fichas` _(or your org slug)_                              |
| `SENTRY_PROJECT`         | `sf-management-front` _(or your project slug)_                    |

4. Click **Save**
5. Trigger a redeploy: **Deployments** -> latest -> **...** -> **Redeploy**

---

## 9. Vercel: HMG Project Configuration

1. Go to https://vercel.com -> select the **sf-management-hmg** project
2. **Settings** -> **Environment Variables**
3. Set these variables (scope: **Production** on this project, since Vercel "production" = the hmg deployment):

| Variable                 | Value                                                     |
| ------------------------ | --------------------------------------------------------- |
| `NEXT_PUBLIC_API_URL`    | `https://sfmanagement-api-hmg.azurewebsites.net/api/v1`   |
| `AUTH_AUTH0_ID`          | _(same or separate Auth0 client ID)_                      |
| `AUTH_AUTH0_SECRET`      | _(same or separate Auth0 client secret)_                  |
| `AUTH_AUTH0_ISSUER`      | _(same Auth0 issuer URL)_                                 |
| `AUTH_AUTH0_AUDIENCE`    | `https://api.semprefichas.com.br`                         |
| `AUTH_SECRET`            | _(generate a different one: `openssl rand -base64 32`)_   |
| `NEXT_PUBLIC_SENTRY_DSN` | _(same DSN -- Sentry uses `environment` tag to separate)_ |
| `SENTRY_AUTH_TOKEN`      | _(same token)_                                            |
| `SENTRY_ORG`             | _(same org slug)_                                         |
| `SENTRY_PROJECT`         | _(same project slug)_                                     |

4. Click **Save**
5. Trigger a redeploy

---

## 10. Verification Checklist

After completing all steps, verify:

### Azure Backend

> **Free F1 Note:** On first request after inactivity (~20 min), expect a cold start delay of 10-30 seconds. This is normal for Free tier. The app will sleep again after inactivity.

- [ ] `https://sfmanagement-api.azurewebsites.net/health` returns 200 (may take 30s on first hit)
- [ ] `https://sfmanagement-api-hmg.azurewebsites.net/health` returns 200
- [ ] Application Insights → Overview shows incoming requests (Server requests chart)
- [ ] Application Insights → Investigate → Live Metrics shows real-time data (1 server online)
- [ ] Application Insights → Investigate → Availability shows green status
- [ ] Application Insights → Monitoring → Logs: run `traces | take 50` to see Serilog entries

### Vercel Frontend

- [ ] `https://sfmanagement.vercel.app` loads login page
- [ ] `https://sf-management-hmg.vercel.app` loads login page
- [ ] Login flow completes and dashboard loads
- [ ] Vercel → Project → Analytics tab shows visitors and page views (enable Web Analytics first if prompted)

### Sentry

- [ ] Sentry → Issues → Feed shows events or "Waiting for first error"
- [ ] Sentry → Alerts shows default alert rules for your project
- [ ] **Test Sentry** (optional): Add this to `instrumentation-client.ts`, deploy, visit site, verify message appears in Sentry, then remove:
  ```typescript
  if (isProduction && sentryEnvironment === "hmg") {
    Sentry.captureMessage("Sentry test - DELETE AFTER VERIFYING", "info");
  }
  ```

### Alerts (Optional Smoke Tests)

These tests verify alerting works but are optional:

- [ ] **Test availability alert:** Temporarily stop the App Service, wait 5-10 min, verify alert email arrives, then restart
- [ ] **Test error alert:** Trigger a 500 error in the API, verify it appears in Application Insights → Failures

> **Note:** Alert emails go to the address configured in the Azure Action Group (step 5).

---

## Quick Reference: All Values Needed

This is a summary of every value you need to collect before starting:

| Value                                 | Source                          | Used In            |
| ------------------------------------- | ------------------------------- | ------------------ |
| App Insights connection string (prod) | Azure portal, step 1            | Azure App Settings |
| App Insights connection string (hmg)  | Azure portal, step 1            | Azure App Settings |
| SQL connection string (prod)          | Azure SQL overview              | Azure App Settings |
| SQL connection string (hmg)           | Azure SQL overview              | Azure App Settings |
| Auth0 domain                          | Auth0 dashboard -> Settings     | Azure + Vercel     |
| Auth0 client ID                       | Auth0 dashboard -> Applications | Azure + Vercel     |
| Auth0 client secret                   | Auth0 dashboard -> Applications | Azure + Vercel     |
| Auth0 API identifier                  | Auth0 dashboard -> APIs         | Azure + Vercel     |
| Sentry DSN                            | Sentry project settings         | Vercel             |
| Sentry auth token                     | Sentry org settings             | Vercel             |
| Sentry org slug                       | Sentry URL                      | Vercel             |
| Sentry project slug                   | Sentry URL                      | Vercel             |
| NextAuth secret (prod)                | `openssl rand -base64 32`       | Vercel prod        |
| NextAuth secret (hmg)                 | `openssl rand -base64 32`       | Vercel hmg         |
| Operations email                      | Your choice                     | Azure action group |

---

# Phase 2: Basic B1 Upgrade (Future)

---

## 11. Upgrade Path: Basic B1

### When to Upgrade to Basic B1

Upgrade from Free F1 to Basic B1 when **any** of these conditions are met:

| Trigger                      | Description                                                         | How to Detect                                                           |
| ---------------------------- | ------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| **Cold start complaints**    | Users report slow first loads (10-30s)                              | User feedback; availability test latency spikes                         |
| **CPU quota exhausted**      | App returns 403 errors after heavy usage                            | Azure Portal -> App Service -> Quotas; App Insights shows 403 responses |
| **Custom domain needed**     | You want `api.semprefichas.com.br` instead of `*.azurewebsites.net` | Business requirement                                                    |
| **Active real users**        | Internal validation complete, real customers onboarding             | Business milestone                                                      |
| **Consistent daily traffic** | More than ~100 requests/day average                                 | App Insights -> Usage; check daily request count                        |

### Basic B1 Cost

| Region       | Approximate Monthly Cost       |
| ------------ | ------------------------------ |
| Brazil South | ~$13 USD/month per app (~R$65) |
| East US      | ~$13 USD/month per app         |

**Total for 2 apps (prod + hmg):** ~$26 USD/month

### Basic B1 Features Unlocked

| Feature            | Value                                                          |
| ------------------ | -------------------------------------------------------------- |
| **Always On**      | App never sleeps — no cold starts                              |
| **Health Check**   | Azure monitors `/health` and auto-restarts unhealthy instances |
| **Custom Domains** | Use your own domain with free managed SSL                      |
| **Manual Scale**   | Scale up to 3 instances (for load, not auto-scale)             |
| **No CPU quota**   | Unlimited CPU time                                             |

### Upgrade Steps

1. **Azure Portal** -> **App Services** -> `sfmanagement-api`
2. **Settings** -> **Scale up (App Service plan)**
3. Select **Basic B1** tier
4. Click **Apply** (causes brief restart, ~30 seconds downtime)
5. Repeat for `sfmanagement-api-hmg`

### Post-Upgrade Configuration

After upgrading to Basic B1, enable the features that weren't available on Free:

#### Enable Always On

1. **App Services** -> select app -> **Configuration** -> **General settings**
2. Set **Always On:** `On`
3. Click **Save**

#### Enable Health Check

1. **App Services** -> select app -> **Monitoring** -> **Health check**
2. Enable health check
3. Set **Path:** `/health`
4. Set **Load balancing threshold:** `3`
5. Click **Save**

#### (Optional) Configure Custom Domain

1. **App Services** -> select app -> **Custom domains**
2. Click **+ Add custom domain**
3. Enter your domain (e.g., `api.semprefichas.com.br`)
4. Follow DNS validation steps (CNAME or A record)
5. Enable **App Service Managed Certificate** for free SSL

---

# Phase 3: Production Tiers (Future)

---

## 12. Upgrade Path: Standard/Premium

### When to Upgrade to Standard S1

Upgrade from Basic B1 to Standard S1 when **any** of these conditions are met:

| Trigger                              | Description                                          | How to Detect                                                                 |
| ------------------------------------ | ---------------------------------------------------- | ----------------------------------------------------------------------------- |
| **Zero-downtime deployments needed** | Can't afford restart delays during deploy            | Business SLA requirement                                                      |
| **Staging slot required**            | Need to test in Azure before swapping to production  | Deployment process requirement                                                |
| **Auto-scale needed**                | Traffic is variable and you need automatic scaling   | App Insights shows traffic spikes causing latency; manual scaling is reactive |
| **5+ active users daily**            | Consistent business usage beyond validation          | Business milestone                                                            |
| **Automated backups needed**         | Need Azure-managed app backups (not just DB backups) | Compliance or recovery requirements                                           |

### When to Upgrade to Standard S2/S3 or Premium

| Trigger                           | Description                                               |
| --------------------------------- | --------------------------------------------------------- |
| **High memory/CPU usage**         | App Insights shows >70% sustained memory or CPU           |
| **Slow response times**           | P95 latency consistently >2s under normal load            |
| **More than 10 concurrent users** | Need more compute capacity                                |
| **Zone redundancy**               | Critical app requiring regional resilience (Premium only) |

### Tier Comparison

| Feature                  | Basic B1   | Standard S1   | Standard S2   | Premium P1v3  |
| ------------------------ | ---------- | ------------- | ------------- | ------------- |
| **Price (Brazil South)** | ~$13/mo    | ~$75/mo       | ~$150/mo      | ~$140/mo      |
| **vCPU / RAM**           | 1 / 1.75GB | 1 / 1.75GB    | 2 / 3.5GB     | 2 / 8GB       |
| **Always On**            | ✅         | ✅            | ✅            | ✅            |
| **Health Check**         | ✅         | ✅            | ✅            | ✅            |
| **Custom Domains**       | ✅         | ✅            | ✅            | ✅            |
| **Staging Slots**        | ❌         | ✅ (5 slots)  | ✅ (5 slots)  | ✅ (20 slots) |
| **Auto-scale**           | ❌         | ✅ (up to 10) | ✅ (up to 10) | ✅ (up to 30) |
| **Daily Backups**        | ❌         | ✅            | ✅            | ✅            |
| **VNet Integration**     | ❌         | ❌            | ❌            | ✅            |
| **Zone Redundancy**      | ❌         | ❌            | ❌            | ✅            |

### Standard Upgrade Steps

1. **Azure Portal** -> **App Services** -> select app
2. **Settings** -> **Scale up (App Service plan)**
3. Select **Standard S1** (or S2/S3 based on needs)
4. Click **Apply**

### Post-Upgrade: Configure Staging Slot (Standard+ only)

1. **App Services** -> select app -> **Deployment** -> **Deployment slots**
2. Click **+ Add Slot**
3. **Name:** `staging`
4. **Clone settings from:** `sfmanagement-api` (production)
5. Click **Add**
6. Configure staging slot environment variables (set `ASPNETCORE_ENVIRONMENT=Staging`)

#### Zero-Downtime Deployment Flow

With staging slots:

```
GitHub Actions deploys to staging slot
    ↓
Run smoke tests against staging URL
    ↓
If tests pass → Swap staging ↔ production
    ↓
Zero-downtime deployment complete
```

### Post-Upgrade: Configure Auto-scale (Standard+ only)

1. **App Services** -> select app -> **Settings** -> **Scale out (App Service plan)**
2. Click **Custom autoscale**
3. Add rule:
   - **Metric:** `CPU Percentage`
   - **Operator:** `Greater than`
   - **Threshold:** `70%`
   - **Duration:** `5 minutes`
   - **Action:** `Increase instance count by 1`
   - **Cool down:** `5 minutes`
4. Add scale-in rule:
   - **Metric:** `CPU Percentage`
   - **Operator:** `Less than`
   - **Threshold:** `30%`
   - **Action:** `Decrease instance count by 1`
5. Set **Instance limits:**
   - **Minimum:** `1`
   - **Maximum:** `3`
   - **Default:** `1`
6. Click **Save**

---

## Upgrade Decision Summary

Use this flowchart to decide when to upgrade:

```
START (Free F1)
    │
    ├── Cold starts annoying users? ──────────────────────► Upgrade to Basic B1
    ├── CPU quota exhausted? ─────────────────────────────► Upgrade to Basic B1
    ├── Need custom domain? ──────────────────────────────► Upgrade to Basic B1
    ├── Real users onboarding? ───────────────────────────► Upgrade to Basic B1
    │
    └── [On Basic B1]
            │
            ├── Need zero-downtime deploys? ──────────────► Upgrade to Standard S1
            ├── Need staging slots for testing? ──────────► Upgrade to Standard S1
            ├── Traffic spikes causing issues? ───────────► Upgrade to Standard S1
            ├── Need auto-scale? ─────────────────────────► Upgrade to Standard S1
            │
            └── [On Standard S1]
                    │
                    ├── P95 latency >2s? ─────────────────► Upgrade to S2 or Premium
                    ├── Memory >70% sustained? ───────────► Upgrade to S2 or Premium
                    ├── Need zone redundancy? ────────────► Upgrade to Premium
                    └── VNet integration needed? ─────────► Upgrade to Premium
```

---

## Monitoring Metrics for Upgrade Decisions

Set up these App Insights alerts to know when an upgrade might be needed:

| Metric                       | Threshold       | Indicates                                    |
| ---------------------------- | --------------- | -------------------------------------------- |
| **Availability**             | < 99%           | Cold starts causing timeouts (upgrade to B1) |
| **Server response time P95** | > 3s            | Need more resources (upgrade tier)           |
| **HTTP 403 count**           | > 0             | CPU quota exhausted on F1 (upgrade to B1)    |
| **CPU %**                    | > 70% sustained | Need more CPU (upgrade tier)                 |
| **Memory %**                 | > 70% sustained | Need more RAM (upgrade tier)                 |
| **Request count**            | > 1000/day      | Significant usage (consider B1 if on F1)     |

---

_Last updated: Feb 23, 2026_
