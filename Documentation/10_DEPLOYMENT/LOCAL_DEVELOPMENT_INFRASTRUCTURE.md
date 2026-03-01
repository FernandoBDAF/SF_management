# Local Development Infrastructure

This document describes the Docker-based local observability stack and the Makefile that centralizes development commands.

---

## Overview

The workspace root (`Sempre Fichas/`) contains shared infrastructure that supports both the backend (`SF_management/`) and frontend (`SF_management-front/`) projects:

```
Sempre Fichas/
├── Makefile                             # Centralized dev commands
├── docker-compose.observability.yml     # Observability stack
├── infrastructure/
│   ├── prometheus/prometheus.yml        # Prometheus scrape config
│   └── grafana/provisioning/            # Grafana auto-provisioning
│       ├── datasources/datasources.yml
│       └── dashboards/dashboards.yml
├── SF_management/                       # Backend project
└── SF_management-front/                 # Frontend project
```

---

## Prerequisites

| Tool | Version | Installation |
|------|---------|-------------|
| Docker | 20+ | https://docs.docker.com/get-docker/ |
| Docker Compose | v2+ | Included with Docker Desktop |
| .NET SDK | 9.x | https://dotnet.microsoft.com/download |
| Node.js | 20+ | https://nodejs.org |
| Yarn | 4.x | Bundled in frontend project via `corepack` |
| dotnet-ef | 9.x | `dotnet tool install --global dotnet-ef` |
| make | any | Pre-installed on macOS |

---

## Docker Observability Stack

### Services

| Service | Image | Host Port | Container Port | Purpose |
|---------|-------|-----------|----------------|---------|
| Seq | `datalust/seq:latest` | `5341` (ingest), `5342` (UI) | `80`, `5341` | Structured log aggregation |
| Prometheus | `prom/prometheus:latest` | `9090` | `9090` | Metrics collection |
| Grafana | `grafana/grafana:latest` | `3001` | `3000` | Dashboards |

### Quick Start

```bash
# Start all containers
make obs-up

# Check status
make obs-status

# View logs
make obs-logs

# Stop
make obs-down

# Full reset (removes volumes)
make obs-reset
```

### Seq

Seq is a structured log search engine. The backend Serilog configuration (`appsettings.Development.json`) sends logs to `http://localhost:5341`.

- **UI:** http://localhost:5342
- **Ingest API:** http://localhost:5341

In the Seq dashboard you can:

- Search logs by level, property, or text
- Filter by `Application`, `Environment`, `RequestId`
- Set up local alerts and dashboards
- Inspect full structured log properties

### Prometheus

Prometheus scrapes health metrics from the local API.

- **UI:** http://localhost:9090
- **Scrape config:** `infrastructure/prometheus/prometheus.yml`
- **Target:** `host.docker.internal:7078` (the local API)

### Grafana

Grafana provides dashboards backed by Prometheus.

- **UI:** http://localhost:3001
- **Login:** `admin` / `admin`
- **Pre-configured datasource:** Prometheus

To add custom dashboards, place JSON files in `infrastructure/grafana/provisioning/dashboards/`.

---

## Makefile Reference

### Observability

| Target | Description |
|--------|-------------|
| `make obs-up` | Start Seq, Prometheus, Grafana |
| `make obs-down` | Stop all containers |
| `make obs-logs` | Tail container logs |
| `make obs-status` | Show container status |
| `make obs-reset` | Stop and remove all volumes |

### Backend

| Target | Description |
|--------|-------------|
| `make api-restore` | Restore NuGet packages |
| `make api-build` | Build in Release mode |
| `make api-run` | Run in Development mode |
| `make api-watch` | Run with hot-reload |
| `make api-test` | Run unit tests |
| `make api-migrate` | Apply pending migrations |
| `make api-migration-add NAME=X` | Create new migration |
| `make api-migration-script` | Generate idempotent SQL script |
| `make api-vuln` | Check for vulnerable packages |
| `make api-health` | Ping `/health` endpoint |

### Frontend

| Target | Description |
|--------|-------------|
| `make web-install` | Install yarn dependencies |
| `make web-dev` | Start dev server |
| `make web-build` | Production build (lint + typecheck + build) |
| `make web-lint` | Run ESLint |
| `make web-typecheck` | TypeScript type-check |
| `make web-check` | Run architecture guardrails |
| `make web-start` | Start production server |
| `make web-audit` | Check for vulnerable packages |

### Composite

| Target | Description |
|--------|-------------|
| `make setup` | First-time bootstrap (obs + restore + install) |
| `make dev` | Print instructions for running both projects |
| `make check` | Full pre-push validation (both projects + vuln) |
| `make clean` | Stop containers and remove build artifacts |

---

## Typical Development Session

```bash
# 1. Start observability (once per session)
make obs-up

# 2. Start backend (Terminal 1)
make api-watch

# 3. Start frontend (Terminal 2)
make web-dev

# 4. Open tools as needed
#    Seq:     http://localhost:5342
#    Swagger: https://localhost:7078
#    App:     http://localhost:3000

# 5. Before pushing
make check

# 6. End of session
make obs-down
```

---

_Last updated: March 2, 2026_
