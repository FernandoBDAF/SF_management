# Production Management Guide

This is the master reference for operating the SF Management system in production. It consolidates infrastructure architecture, environment configuration, security hardening, observability, and operational procedures into a single knowledge base.

---

## Table of Contents

- [Infrastructure Overview](#infrastructure-overview)
- [Environment Architecture](#environment-architecture)
- [Configuration Management](#configuration-management)
- [Security Hardening](#security-hardening)
- [Observability Stack](#observability-stack)
- [Alerting Strategy](#alerting-strategy)
- [Database Management](#database-management)
- [CI/CD Pipeline](#cicd-pipeline)
- [Deployment Procedures](#deployment-procedures)
- [Incident Response](#incident-response)
- [Cost and Scaling](#cost-and-scaling)
- [Local Development Infrastructure](#local-development-infrastructure)
- [Azure Manual Configuration Checklist](#azure-manual-configuration-checklist)
- [Vercel Manual Configuration Checklist](#vercel-manual-configuration-checklist)
- [Related Runbooks](#related-runbooks)

---

## Infrastructure Overview

### System Components

```
Internet
   │
   ├── Vercel ─────────────────────────────────────────────┐
   │   ├── Frontend PROD (Next.js 14, @sentry/nextjs)     │
   │   └── Frontend HMG  (Next.js 14, @sentry/nextjs)     │
   │                                                        │
   ├── Azure ──────────────────────────────────────────────┐│
   │   ├── Web App: sfmanagement-api       (Free F1)      ││
   │   │   └── .NET 9 + Serilog + App Insights            ││
   │   ├── Web App: sfmanagement-api-hmg   (Free F1)      ││
   │   │   └── .NET 9 + Serilog + App Insights            ││
   │   ├── Azure SQL: Production DB                        ││
   │   ├── Azure SQL: HMG DB                               ││
   │   ├── Application Insights (PROD + HMG)               ││
   │   └── Azure Monitor (alerts + availability tests)     ││
   │                                                        ││
   └── Auth0 ──────────────────────────────────────────────┘│
       └── Tenant: JWT auth, roles, permissions             │
                                                            │
```

> **Current Tier:** Free F1 (validation phase). See [AZURE_VERCEL_SETUP_GUIDE.md](./AZURE_VERCEL_SETUP_GUIDE.md#11-upgrade-path-basic-b1) for upgrade triggers and path.

### Technology Stack

| Layer                 | Technology                         | Notes                        |
| --------------------- | ---------------------------------- | ---------------------------- |
| Frontend              | Next.js 14.2, React 18, TypeScript | Deployed on Vercel           |
| Backend               | ASP.NET Core 9 (C#)                | Deployed on Azure Web Apps   |
| Database              | Azure SQL (SQL Server)             | EF Core 9                    |
| Auth                  | Auth0 + NextAuth v5                | JWT Bearer, RBAC             |
| Monitoring (backend)  | Serilog + Application Insights     | Structured logging + APM     |
| Monitoring (frontend) | Sentry + Vercel Analytics          | Error tracking + Web Vitals  |
| CI/CD (backend)       | GitHub Actions                     | Branch-triggered deployments |
| CI/CD (frontend)      | Vercel                             | Auto-deploy on push          |

---

## Environment Architecture

### Environments

| Environment   | Backend                                  | Frontend                       | Database         | Branch |
| ------------- | ---------------------------------------- | ------------------------------ | ---------------- | ------ |
| Production    | `sfmanagement-api.azurewebsites.net`     | `sfmanagement.vercel.app`      | Production DB    | `main` |
| HMG (Staging) | `sfmanagement-api-hmg.azurewebsites.net` | `sf-management-hmg.vercel.app` | HMG DB           | `hmg`  |
| Development   | `localhost:7078`                         | `localhost:3000`               | Local SQL Server | any    |

### Environment Variables (Backend)

Set via Azure App Settings. They override `appsettings.json` / `appsettings.{Environment}.json`:

| Variable                                | Production       | HMG             | Development          |
| --------------------------------------- | ---------------- | --------------- | -------------------- |
| `ASPNETCORE_ENVIRONMENT`                | `Production`     | `Staging`       | `Development`        |
| `ConnectionStrings__DefaultConnection`  | prod conn string | hmg conn string | local / user secrets |
| `Auth0__Domain`                         | tenant domain    | tenant domain   | tenant domain        |
| `Auth0__Audience`                       | API identifier   | API identifier  | API identifier       |
| `Auth0__ClientId`                       | app client ID    | app client ID   | app client ID        |
| `Auth0__ClientSecret`                   | app secret       | app secret      | user secrets         |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | prod AI string   | hmg AI string   | (empty)              |
| `EnableDetailedLogging`                 | `false`          | `true`          | `true`               |
| `RunMigrationsOnStartup`                | `false`          | `false`         | `true`               |

### Environment Variables (Frontend)

Set via Vercel project settings:

| Variable                 | Production     | HMG            | Development                     |
| ------------------------ | -------------- | -------------- | ------------------------------- |
| `NEXT_PUBLIC_API_URL`    | prod API URL   | hmg API URL    | `https://localhost:7078/api/v1` |
| `AUTH_AUTH0_ID`          | client ID      | client ID      | client ID                       |
| `AUTH_AUTH0_SECRET`      | client secret  | client secret  | `.env.local`                    |
| `AUTH_AUTH0_ISSUER`      | issuer URL     | issuer URL     | issuer URL                      |
| `AUTH_AUTH0_AUDIENCE`    | API identifier | API identifier | API identifier                  |
| `AUTH_SECRET`            | random secret  | random secret  | `.env.local`                    |
| `NEXT_PUBLIC_SENTRY_DSN` | Sentry DSN     | Sentry DSN     | (empty)                         |
| `SENTRY_AUTH_TOKEN`      | Sentry token   | Sentry token   | (empty)                         |
| `SENTRY_ORG`             | org slug       | org slug       | (empty)                         |
| `SENTRY_PROJECT`         | project slug   | project slug   | (empty)                         |

---

## Configuration Management

### Backend Config File Hierarchy

```
1. appsettings.json                  ← shared defaults (committed)
2. appsettings.{Environment}.json    ← environment overrides (committed)
3. Azure App Settings / env vars     ← runtime overrides (not committed)
4. User Secrets                      ← local dev secrets (not committed)
```

### Key Configuration Decisions

| Setting                      | Default        | Dev            | HMG           | Prod          | Reason                        |
| ---------------------------- | -------------- | -------------- | ------------- | ------------- | ----------------------------- |
| `EnableSensitiveDataLogging` | code-level     | `true`         | `false`       | `false`       | Prevents PII in logs          |
| `EnableDetailedLogging`      | `false`        | `true`         | `true`        | `false`       | Reduces log noise             |
| `RunMigrationsOnStartup`     | `false`        | `true`         | `false`       | `false`       | Prevents migration conflicts  |
| Swagger + DevExceptionPage   | guarded        | enabled        | enabled       | disabled      | Prevents API surface exposure |
| Serilog Seq sink             | not in default | enabled        | disabled      | disabled      | Local-only log aggregation    |
| Serilog App Insights sink    | in default     | (no AI string) | enabled       | enabled       | Cloud telemetry               |
| Rate limit (general)         | 240/min        | whitelisted    | 240/min       | 240/min       | Supports SPA burst pattern    |
| Auth log level               | `Information`  | `Debug`        | `Information` | `Warning`     | Reduces prod log volume       |
| CORS origins                 | empty          | localhost      | hmg frontend  | prod frontend | Per-environment isolation     |

---

## Security Hardening

### Backend

| Control                | Implementation                                                                                                   | File                                          |
| ---------------------- | ---------------------------------------------------------------------------------------------------------------- | --------------------------------------------- |
| Security headers       | `SecurityHeadersMiddleware` adds X-Content-Type-Options, X-Frame-Options, CSP, Referrer-Policy, X-XSS-Protection | `Api/Middleware/SecurityHeadersMiddleware.cs` |
| HSTS                   | `app.UseHsts()` in non-development                                                                               | `Program.cs`                                  |
| CORS                   | Per-environment origins from `Cors:AllowedOrigins` config                                                        | `Program.cs`                                  |
| Swagger gating         | Only enabled in Development/Staging                                                                              | `Program.cs`                                  |
| Auth                   | Auth0 JWT Bearer + RBAC policies                                                                                 | `DependencyInjectionExtensions.cs`            |
| Rate limiting          | IP-based, 240 req/min general                                                                                    | `appsettings.json`                            |
| Sensitive data logging | Disabled outside Development                                                                                     | `Program.cs`                                  |
| Dependency scanning    | `dotnet list package --vulnerable` in CI                                                                         | GitHub Actions workflows                      |

### Frontend

| Control             | Implementation                                                                | File                          |
| ------------------- | ----------------------------------------------------------------------------- | ----------------------------- |
| Security headers    | CSP, X-Frame-Options, X-XSS-Protection, Referrer-Policy via `next.config.mjs` | `next.config.mjs`             |
| `poweredByHeader`   | Disabled                                                                      | `next.config.mjs`             |
| TLS dev workaround  | `NODE_TLS_REJECT_UNAUTHORIZED=0` only in `dev` script, not in build/start     | `package.json`                |
| Build validation    | Lint + TypeScript check before build                                          | `package.json` `build` script |
| Dependency scanning | `yarn npm audit` (manual or in CI)                                            | Makefile target `web-audit`   |

---

## Observability Stack

### Production (Cloud)

| Component          | Tool                                                | Cost                                           |
| ------------------ | --------------------------------------------------- | ---------------------------------------------- |
| Backend APM        | Azure Application Insights                          | Free up to 5 GB/month                          |
| Backend logging    | Serilog -> App Insights sink                        | Included                                       |
| Backend health     | `/health` endpoint + App Insights availability test | Free (Health Check feature requires Basic B1+) |
| Frontend errors    | Sentry (`@sentry/nextjs`)                           | Free tier: 5K errors/month                     |
| Frontend analytics | Vercel Analytics (`@vercel/analytics`)              | Free with Vercel                               |
| Alerting           | Azure Monitor alert rules                           | Free (included with App Insights)              |

### Local Development (Docker)

| Container  | Port                         | Purpose                             |
| ---------- | ---------------------------- | ----------------------------------- |
| Seq        | `5341` (ingest), `5342` (UI) | Structured log search and dashboard |
| Prometheus | `9090`                       | Metrics scraping and querying       |
| Grafana    | `3001`                       | Dashboards and visualization        |

Start with `make obs-up`. See [LOCAL_DEVELOPMENT_INFRASTRUCTURE.md](#local-development-infrastructure).

### Sentry Configuration

Files in frontend root:

- `instrumentation-client.ts` — Client-side Sentry init + router transition capture
- `instrumentation.ts` — Server/edge Sentry init loader
- `sentry.server.config.ts` — Node.js runtime config
- `sentry.edge.config.ts` — Edge runtime config
- `src/app/global-error.tsx` — Root error boundary + Sentry capture
- `src/app/not-found.tsx` — Custom 404 page

Sentry is enabled only in production (`process.env.NODE_ENV === "production"`).

---

## Alerting Strategy

### Azure Monitor Alert Rules

Create in Azure portal for both `sfmanagement-api` and `sfmanagement-api-hmg`:

| Alert                | Condition                          | Evaluation Window | Severity             |
| -------------------- | ---------------------------------- | ----------------- | -------------------- |
| API availability     | URL ping test fails >= 2 locations | 5 min             | Sev0 (Critical)      |
| High error rate      | HTTP 5xx > 5% of requests          | 5 min             | Sev1 (Error)         |
| High latency         | P95 response time > 3000 ms        | 10 min            | Sev2 (Warning)       |
| Unhandled exceptions | Exception count > 20               | 10 min            | Sev2                 |
| SQL DTU saturation   | DTU usage > 80%                    | 10 min            | Sev2                 |
| SQL storage pressure | Storage > 80% of allocated         | 15 min            | Sev3 (Informational) |

### Availability Tests

- URL: `https://<app>.azurewebsites.net/health`
- Frequency: every 5 minutes
- Locations: at least 3 Azure regions
- Alert on: 2+ location failures

### Sentry Alert Rules (Frontend)

- New issue in production: immediate notification
- Regression detected: immediate notification
- Error rate spike (per release): 5-min evaluation window

### Action Groups

Use a single Azure Monitor Action Group:

- Email to operations distribution list
- Webhook to Teams/Slack channel
- Separate escalation group for Sev0/Sev1

---

## Database Management

### Backup Strategy

Azure SQL provides automated backups by default:

| Feature                      | Basic Tier                     |
| ---------------------------- | ------------------------------ |
| Full backups                 | Weekly                         |
| Differential backups         | Every 12-24 hours              |
| Transaction log backups      | Every 5-10 minutes             |
| Point-in-time restore (PITR) | Last 7 days                    |
| Long-term retention (LTR)    | Optional (recommended monthly) |

### Migration Strategy

Production does **not** auto-run migrations on startup (`RunMigrationsOnStartup = false`).

Safe migration workflow:

1. Generate idempotent script: `make api-migration-script`
2. CI generates and saves `migration.sql` as build artifact
3. Deploy to HMG first; validate
4. Review migration artifact before merging to `main`
5. After production deploy, validate `/health` and critical flows

### Quarterly DR Test

1. Point-in-time restore production DB to test database
2. Run read queries to validate data
3. Capture restore duration and any blockers
4. Update `DATABASE_OPERATIONS_RUNBOOK.md` with findings

---

## CI/CD Pipeline

### Backend (GitHub Actions)

| Workflow                       | Trigger        | Target                           |
| ------------------------------ | -------------- | -------------------------------- |
| `main_sfmanagement-api.yml`    | Push to `main` | `sfmanagement-api` (Production)  |
| `hmg_sfmanagement-api-hmg.yml` | Push to `hmg`  | `sfmanagement-api-hmg` (Staging) |

Both workflows:

1. Checkout code
2. Setup .NET 9.x
3. Install `dotnet-ef` tool
4. Run vulnerability scan (`dotnet list package --vulnerable`)
5. Generate idempotent migration script
6. Build in Release mode
7. Publish application
8. Upload artifacts: app bundle, `migration.sql`, `vulnerability-report.txt`
9. Login to Azure (OIDC)
10. Deploy to Azure Web App

### Frontend (Vercel)

Vercel auto-deploys on push. Build command: `next lint && tsc --noEmit && next build`.

Ensure each environment (prod/hmg) uses separate Vercel projects or environment-scoped variables.

---

## Deployment Procedures

### Standard Deployment

1. Merge feature branch -> `hmg`
2. Wait for HMG workflow to complete
3. Run HMG smoke test checklist (see `DEPLOYMENT_AND_ROLLBACK_RUNBOOK.md`)
4. If migration changed, review `migration.sql` artifact
5. Merge `hmg` -> `main`
6. Wait for production workflow to complete
7. Run production smoke test checklist
8. Monitor alerts for 15 minutes

### Rollback

| Scenario                    | Action                                                    |
| --------------------------- | --------------------------------------------------------- |
| App bug, no schema change   | Redeploy previous successful build from GitHub Actions    |
| App bug after merge to main | `git revert` + push to trigger redeploy                   |
| Schema/migration issue      | Restore DB to prior point-in-time + redeploy previous app |

---

## Incident Response

### Severity Model

| Severity | Definition                              | Target First Response |
| -------- | --------------------------------------- | --------------------- |
| Sev0     | Production outage or data risk          | 15 minutes            |
| Sev1     | Major degradation of critical workflows | 30 minutes            |
| Sev2     | Partial degradation with workaround     | 2 hours               |

### First Response Steps

1. Confirm alert validity
2. Classify severity
3. Capture timestamp and impacted services
4. Post status update to operations channel
5. Follow the applicable procedure (API Down / Auth0 / Database)

See `INCIDENT_RESPONSE_RUNBOOK.md` for detailed procedures.

---

## Cost and Scaling

### Current Costs (Free F1 Tier — Validation Phase)

| Resource             | ~Monthly Cost       |
| -------------------- | ------------------- |
| Web App PROD (F1)    | $0                  |
| Web App HMG (F1)     | $0                  |
| Azure SQL (Basic)    | ~$5 each            |
| Application Insights | Free (< 5 GB/month) |
| **Total Azure**      | **~$10/month**      |
| Vercel (Hobby)       | Free                |
| Sentry (Free tier)   | $0                  |
| Auth0 (Free tier)    | $0                  |

### Free F1 Limitations

- **Cold starts:** App sleeps after ~20 min of inactivity, causing 10-30s delay on first request
- **CPU quota:** 60 min/day limit — app returns 403 if exceeded
- **No Always On:** Cannot prevent the app from sleeping
- **No Health Check:** Must use App Insights availability tests instead

### Upgrade Path

| Phase                      | Trigger                                                             | Upgrade To   | Monthly Cost (2 apps) |
| -------------------------- | ------------------------------------------------------------------- | ------------ | --------------------- |
| **1. Validation Complete** | Real users onboarding, cold starts problematic, CPU quota exhausted | Basic B1     | ~$26                  |
| **2. Growth**              | Need zero-downtime deploys, staging slots, auto-scale               | Standard S1  | ~$150                 |
| **3. Scale**               | High traffic, P95 latency >2s, need zone redundancy                 | Premium P1v3 | ~$280                 |

**Current Phase:** Validation (Free F1) — appropriate while validating the product with internal users.

**Next Upgrade Trigger:** Upgrade to Basic B1 when any of these occur:

- Real customers begin using the system
- Cold start complaints from users
- CPU quota exhausted (403 errors)
- Need custom domain with SSL

See [AZURE_VERCEL_SETUP_GUIDE.md](./AZURE_VERCEL_SETUP_GUIDE.md#11-upgrade-path-basic-b1) for detailed upgrade steps and decision criteria.

---

## Local Development Infrastructure

### Prerequisites

- Docker and Docker Compose
- .NET 9 SDK
- Node.js 20+ with Yarn 4
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`
- `make` (pre-installed on macOS)

### First-Time Setup

```bash
make setup
```

This starts the observability stack, restores backend packages, and installs frontend dependencies.

### Starting Development

```bash
# Terminal 1: Observability stack
make obs-up

# Terminal 2: Backend API
make api-watch

# Terminal 3: Frontend
make web-dev
```

### Observability UIs

| Tool       | URL                    | Purpose                           |
| ---------- | ---------------------- | --------------------------------- |
| Seq        | http://localhost:5342  | Search and filter structured logs |
| Prometheus | http://localhost:9090  | Metrics and query language        |
| Grafana    | http://localhost:3001  | Dashboards (login: admin/admin)   |
| Swagger    | https://localhost:7078 | API explorer (dev/staging only)   |

### Makefile Reference

Run `make help` for the full command list. Key targets:

| Target                      | Description                             |
| --------------------------- | --------------------------------------- |
| `make setup`                | First-time bootstrap                    |
| `make obs-up` / `obs-down`  | Start/stop Docker observability stack   |
| `make api-watch`            | Run backend with hot-reload             |
| `make web-dev`              | Run frontend dev server                 |
| `make check`                | Full pre-push validation                |
| `make api-migration-script` | Generate migration SQL for review       |
| `make api-vuln`             | Check backend for vulnerable packages   |
| `make clean`                | Stop containers and clean build outputs |

---

## Azure Manual Configuration Checklist

After deploying code changes, complete these one-time Azure portal steps:

### Application Insights

- [ ] Create Application Insights resource for production
- [ ] Create Application Insights resource for HMG
- [ ] Set `APPLICATIONINSIGHTS_CONNECTION_STRING` in each Web App's App Settings
- [ ] Enable Application Insights in the App Service blade

### Availability Tests

- [ ] Create URL ping test for `https://sfmanagement-api.azurewebsites.net/health`
- [ ] Create URL ping test for `https://sfmanagement-api-hmg.azurewebsites.net/health`
- [ ] Configure: 5-min interval, 3+ regions, alert on 2+ failures

### Alert Rules

- [ ] API availability (Sev0)
- [ ] Error rate > 5% (Sev1)
- [ ] P95 latency > 3s (Sev2)
- [ ] Exception count > 20 (Sev2)
- [ ] SQL DTU > 80% (Sev2)
- [ ] SQL storage > 80% (Sev3)

### Action Group

- [ ] Create action group with email recipients
- [ ] (Optional) Add webhook for Teams/Slack

### Database

- [ ] Verify PITR retention period
- [ ] Enable Long-Term Retention (recommended: weekly 8 weeks, monthly 12 months)

### CORS Origins

- [ ] Set `Cors__AllowedOrigins__0` etc. in App Settings (or rely on `appsettings.Production.json`)

### Web App Health Check

- [ ] Enable built-in health check in Azure portal (path: `/health`)

---

## Vercel Manual Configuration Checklist

### Per-Environment Project

- [ ] Ensure prod and hmg are separate Vercel projects (or use environment-scoped vars)
- [ ] Set all variables from `.env.example` for each environment

### Sentry Integration

- [ ] Create Sentry project at sentry.io
- [ ] Set `NEXT_PUBLIC_SENTRY_DSN` in Vercel env vars
- [ ] Set `SENTRY_AUTH_TOKEN`, `SENTRY_ORG`, `SENTRY_PROJECT` for source map uploads
- [ ] Configure Sentry alert rules (new issue, regression, error spike)

### Vercel Analytics

- [ ] Enable Analytics in Vercel dashboard (or rely on `@vercel/analytics` already in layout)

---

## Related Runbooks

| Document                                                                   | Purpose                                   |
| -------------------------------------------------------------------------- | ----------------------------------------- |
| [CI_CD_PIPELINE.md](CI_CD_PIPELINE.md)                                     | GitHub Actions workflow details           |
| [AZURE_INFRASTRUCTURE.md](AZURE_INFRASTRUCTURE.md)                         | Azure resource configuration              |
| [PRODUCTION_MONITORING_AND_ALERTS.md](PRODUCTION_MONITORING_AND_ALERTS.md) | Alert catalog and KQL queries             |
| [DATABASE_OPERATIONS_RUNBOOK.md](DATABASE_OPERATIONS_RUNBOOK.md)           | Backup, restore, and migration procedures |
| [DEPLOYMENT_AND_ROLLBACK_RUNBOOK.md](DEPLOYMENT_AND_ROLLBACK_RUNBOOK.md)   | Deploy checklist and rollback matrix      |
| [INCIDENT_RESPONSE_RUNBOOK.md](INCIDENT_RESPONSE_RUNBOOK.md)               | Severity model and incident handling      |

---

_Last updated: March 2, 2026_
