# Azure & Vercel Setup Guide

Step-by-step instructions to configure all cloud services that the codebase depends on. Each section has numbered steps and the exact values to use.

---

## Table of Contents

1. [Azure: Create Application Insights](#1-azure-create-application-insights)
2. [Azure: Set App Settings](#2-azure-set-app-settings)
3. [Azure: Enable Health Check](#3-azure-enable-health-check)
4. [Azure: Create Availability Tests](#4-azure-create-availability-tests)
5. [Azure: Create Alert Rules](#5-azure-create-alert-rules)
6. [Azure: Create Action Group](#6-azure-create-action-group)
7. [Azure: Database Long-Term Retention](#7-azure-database-long-term-retention)
8. [Sentry: Create Account and Project](#8-sentry-create-account-and-project)
9. [Vercel: Production Project Configuration](#9-vercel-production-project-configuration)
10. [Vercel: HMG Project Configuration](#10-vercel-hmg-project-configuration)
11. [Verification Checklist](#11-verification-checklist)

---

## 1. Azure: Create Application Insights

Repeat for both environments.

### Production

1. Azure Portal -> **Create a resource** -> search "Application Insights"
2. Click **Create**
3. Configure:
   - **Name:** `sfmanagement-api-insights`
   - **Resource Group:** same as `sfmanagement-api`
   - **Region:** same as Web App
   - **Log Analytics Workspace:** create new or use existing
4. Click **Review + Create** -> **Create**
5. After creation, go to the resource -> **Overview** -> copy the **Connection String**

### HMG

1. Repeat the same steps with:
   - **Name:** `sfmanagement-api-hmg-insights`
   - **Resource Group:** same as `sfmanagement-api-hmg`
2. Copy the HMG connection string

---

## 2. Azure: Set App Settings

### Production (`sfmanagement-api`)

1. Azure Portal -> **App Services** -> `sfmanagement-api`
2. **Settings** -> **Environment variables** (or Configuration -> Application settings)
3. Add or verify these settings:

| Name | Value |
|------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | *(paste the prod connection string from step 1)* |
| `ConnectionStrings__DefaultConnection` | *(your prod SQL connection string)* |
| `Auth0__Domain` | *(your Auth0 tenant domain, e.g. `semprefichas.auth0.com`)* |
| `Auth0__Audience` | `https://api.semprefichas.com.br` |
| `Auth0__ClientId` | *(your Auth0 app client ID)* |
| `Auth0__ClientSecret` | *(your Auth0 app client secret)* |

4. Click **Apply** (this restarts the Web App)

### HMG (`sfmanagement-api-hmg`)

1. Azure Portal -> **App Services** -> `sfmanagement-api-hmg`
2. **Settings** -> **Environment variables**
3. Add or verify:

| Name | Value |
|------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Staging` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | *(paste the HMG connection string from step 1)* |
| `ConnectionStrings__DefaultConnection` | *(your HMG SQL connection string)* |
| `Auth0__Domain` | *(same Auth0 tenant domain)* |
| `Auth0__Audience` | `https://api.semprefichas.com.br` |
| `Auth0__ClientId` | *(same or separate Auth0 app)* |
| `Auth0__ClientSecret` | *(same or separate Auth0 secret)* |

4. Click **Apply**

---

## 3. Azure: Enable Health Check

Repeat for both Web Apps.

1. Azure Portal -> **App Services** -> select the Web App
2. **Monitoring** -> **Health check**
3. Enable health check
4. Set **Path:** `/health`
5. Set **Load balancing threshold:** `3` (unhealthy after 3 consecutive failures)
6. Click **Save**

---

## 4. Azure: Create Availability Tests

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

## 5. Azure: Create Alert Rules

Create in the Application Insights resource for each environment.

### 5.1 High Error Rate

1. Application Insights -> **Monitoring** -> **Alerts** -> **+ Create alert rule**
2. **Condition:**
   - Signal: `Failed requests`
   - Operator: `Greater than`
   - Threshold: `5` (as percentage)
   - Aggregation: `Percentage`
   - Period: `5 minutes`
3. **Actions:** select action group (create in step 6 if not yet created)
4. **Severity:** `Sev1 - Error`
5. **Name:** `SF API [Prod/HMG] - High Error Rate`
6. Click **Create**

### 5.2 High Latency

1. New alert rule:
   - Signal: `Server response time`
   - Operator: `Greater than`
   - Threshold: `3000` (milliseconds)
   - Aggregation: `Percentile 95`
   - Period: `10 minutes`
2. Severity: `Sev2 - Warning`
3. Name: `SF API [Prod/HMG] - High Latency P95`

### 5.3 Unhandled Exceptions

1. New alert rule:
   - Signal: `Exceptions` (count)
   - Operator: `Greater than`
   - Threshold: `20`
   - Period: `10 minutes`
2. Severity: `Sev2 - Warning`
3. Name: `SF API [Prod/HMG] - Exception Spike`

### 5.4 SQL DTU

1. Azure Portal -> **SQL databases** -> select production database
2. **Monitoring** -> **Alerts** -> **+ Create alert rule**
3. **Condition:**
   - Signal: `DTU percentage`
   - Operator: `Greater than`
   - Threshold: `80`
   - Period: `10 minutes`
4. Severity: `Sev2`
5. Name: `SF SQL [Prod/HMG] - DTU High`

### 5.5 SQL Storage

1. Same SQL database -> new alert rule:
   - Signal: `Data space used percent`
   - Operator: `Greater than`
   - Threshold: `80`
   - Period: `15 minutes`
2. Severity: `Sev3 - Informational`
3. Name: `SF SQL [Prod/HMG] - Storage High`

---

## 6. Azure: Create Action Group

1. Azure Portal -> **Monitor** -> **Alerts** -> **Action groups** -> **+ Create**
2. **Basics:**
   - **Action group name:** `SF Operations`
   - **Display name:** `SF Ops`
   - **Resource group:** same as production Web App
3. **Notifications:**
   - Type: `Email/SMS/Push/Voice`
   - Name: `Ops Email`
   - Email: *(your operations email address)*
4. **(Optional) Actions:**
   - Type: `Webhook`
   - Name: `Teams/Slack Webhook`
   - URI: *(your Teams or Slack incoming webhook URL)*
5. Click **Review + Create** -> **Create**
6. Go back to each alert rule and attach this action group

---

## 7. Azure: Database Long-Term Retention

1. Azure Portal -> **SQL databases** -> select production database
2. **Data management** -> **Backups** -> **Retention policies**
3. **Long-term retention** tab:
   - **Weekly backups:** `8 weeks`
   - **Monthly backups:** `12 months`
4. Click **Apply**
5. Repeat for HMG database (optional, lower retention is fine)

---

## 8. Sentry: Create Account and Project

### 8.1 Create Account

1. Go to https://sentry.io/signup/
2. Sign up with your email or GitHub
3. Create an organization (e.g., `semprefichas`)

### 8.2 Create Project

1. After login -> **Settings** -> **Projects** -> **Create Project**
2. Platform: **Next.js**
3. Project name: `sf-management-front`
4. Click **Create Project**
5. Copy the **DSN** from the setup page (format: `https://xxxxx@oXXXXX.ingest.sentry.io/XXXXX`)

### 8.3 Create Auth Token (for source maps)

1. **Settings** -> **Auth Tokens** -> **Create New Token**
2. Scopes: `project:releases`, `org:read`
3. Copy the token

### 8.4 Configure Alert Rules

1. **Alerts** -> **Create Alert**
2. Create these rules:
   - **New Issue:** When a new issue is created, send notification (email)
   - **Regression:** When a resolved issue re-appears, send notification
   - **Error Spike:** When issue frequency exceeds 10 events in 5 minutes

### 8.5 Note Values for Vercel Config

You now have:

| Value | Where to find |
|-------|--------------|
| `NEXT_PUBLIC_SENTRY_DSN` | Project -> Settings -> Client Keys (DSN) |
| `SENTRY_AUTH_TOKEN` | Settings -> Auth Tokens |
| `SENTRY_ORG` | Organization slug (e.g., `semprefichas`) |
| `SENTRY_PROJECT` | Project slug (e.g., `sf-management-front`) |

---

## 9. Vercel: Production Project Configuration

1. Go to https://vercel.com -> select the **sfmanagement** project
2. **Settings** -> **Environment Variables**
3. Set these variables (scope: **Production**):

| Variable | Value |
|----------|-------|
| `NEXT_PUBLIC_API_URL` | `https://sfmanagement-api.azurewebsites.net/api/v1` |
| `AUTH_AUTH0_ID` | *(your Auth0 client ID)* |
| `AUTH_AUTH0_SECRET` | *(your Auth0 client secret)* |
| `AUTH_AUTH0_ISSUER` | *(your Auth0 issuer URL, e.g., `https://semprefichas.auth0.com`)* |
| `AUTH_AUTH0_AUDIENCE` | `https://api.semprefichas.com.br` |
| `AUTH_SECRET` | *(generate: `openssl rand -base64 32`)* |
| `NEXT_PUBLIC_SENTRY_DSN` | *(DSN from step 8)* |
| `SENTRY_AUTH_TOKEN` | *(token from step 8.3)* |
| `SENTRY_ORG` | `semprefichas` *(or your org slug)* |
| `SENTRY_PROJECT` | `sf-management-front` *(or your project slug)* |

4. Click **Save**
5. Trigger a redeploy: **Deployments** -> latest -> **...** -> **Redeploy**

---

## 10. Vercel: HMG Project Configuration

1. Go to https://vercel.com -> select the **sf-management-hmg** project
2. **Settings** -> **Environment Variables**
3. Set these variables (scope: **Production** on this project, since Vercel "production" = the hmg deployment):

| Variable | Value |
|----------|-------|
| `NEXT_PUBLIC_API_URL` | `https://sfmanagement-api-hmg.azurewebsites.net/api/v1` |
| `AUTH_AUTH0_ID` | *(same or separate Auth0 client ID)* |
| `AUTH_AUTH0_SECRET` | *(same or separate Auth0 client secret)* |
| `AUTH_AUTH0_ISSUER` | *(same Auth0 issuer URL)* |
| `AUTH_AUTH0_AUDIENCE` | `https://api.semprefichas.com.br` |
| `AUTH_SECRET` | *(generate a different one: `openssl rand -base64 32`)* |
| `NEXT_PUBLIC_SENTRY_DSN` | *(same DSN -- Sentry uses `environment` tag to separate)* |
| `SENTRY_AUTH_TOKEN` | *(same token)* |
| `SENTRY_ORG` | *(same org slug)* |
| `SENTRY_PROJECT` | *(same project slug)* |

4. Click **Save**
5. Trigger a redeploy

---

## 11. Verification Checklist

After completing all steps, verify:

### Azure Backend

- [ ] `https://sfmanagement-api.azurewebsites.net/health` returns 200
- [ ] `https://sfmanagement-api-hmg.azurewebsites.net/health` returns 200
- [ ] Application Insights shows incoming requests (wait 5 minutes)
- [ ] Application Insights -> Live Metrics shows real-time data
- [ ] Availability tests show green status after 10 minutes
- [ ] Log Analytics shows Serilog entries under `traces` table

### Vercel Frontend

- [ ] `https://sfmanagement.vercel.app` loads login page
- [ ] `https://sf-management-hmg.vercel.app` loads login page
- [ ] Login flow completes and dashboard loads
- [ ] Vercel Analytics shows data in dashboard (after some traffic)

### Sentry

- [ ] `https://sentry.io` -> project -> Issues shows "Waiting for events" or first events
- [ ] Verify environment tags: prod errors tagged `production`, hmg errors tagged `hmg`
- [ ] Test by temporarily adding `Sentry.captureMessage("test")` in a page, deploy to hmg, verify it appears in Sentry, then remove

### Alerts

- [ ] Availability test alert fires if you temporarily break health endpoint (optional smoke test)
- [ ] Action group sends email to configured address

---

## Quick Reference: All Values Needed

This is a summary of every value you need to collect before starting:

| Value | Source | Used In |
|-------|--------|---------|
| App Insights connection string (prod) | Azure portal, step 1 | Azure App Settings |
| App Insights connection string (hmg) | Azure portal, step 1 | Azure App Settings |
| SQL connection string (prod) | Azure SQL overview | Azure App Settings |
| SQL connection string (hmg) | Azure SQL overview | Azure App Settings |
| Auth0 domain | Auth0 dashboard -> Settings | Azure + Vercel |
| Auth0 client ID | Auth0 dashboard -> Applications | Azure + Vercel |
| Auth0 client secret | Auth0 dashboard -> Applications | Azure + Vercel |
| Auth0 API identifier | Auth0 dashboard -> APIs | Azure + Vercel |
| Sentry DSN | Sentry project settings | Vercel |
| Sentry auth token | Sentry org settings | Vercel |
| Sentry org slug | Sentry URL | Vercel |
| Sentry project slug | Sentry URL | Vercel |
| NextAuth secret (prod) | `openssl rand -base64 32` | Vercel prod |
| NextAuth secret (hmg) | `openssl rand -base64 32` | Vercel hmg |
| Operations email | Your choice | Azure action group |

---

_Last updated: March 2, 2026_
