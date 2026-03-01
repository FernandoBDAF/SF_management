# Production Monitoring and Alerts

This runbook defines the observability stack and alert strategy for the production and HMG APIs hosted on Azure.

---

## Objectives

- Detect outages quickly with health checks
- Detect degradation before users report issues
- Keep alerting actionable and low-noise
- Track API and database behavior over time

---

## Monitoring Stack

### Backend

- **Serilog** for structured logs
- **Application Insights** for traces, exceptions, dependency monitoring, and request performance
- **Azure Monitor** for availability tests and alerts

### Databases

- **Azure SQL metrics** for DTU, storage, and connection pressure
- **Query Performance Insight** for expensive queries

---

## Required Azure Configuration

For each environment (`sfmanagement-api` and `sfmanagement-api-hmg`):

1. Create or attach an **Application Insights** resource
2. Set app setting:
   - `APPLICATIONINSIGHTS_CONNECTION_STRING`
3. Enable Application Insights in App Service blade
4. Configure availability test:
   - URL: `https://<app>.azurewebsites.net/health`
   - Interval: 5 minutes
   - Test locations: at least 3 regions

---

## Alert Rules

Create these Azure Monitor alert rules for both environments.

| Alert | Signal | Condition | Window | Severity | Action |
|------|--------|-----------|--------|----------|--------|
| API availability | Availability test | Failed locations >= 2 | 5 min | Sev0 | Email + webhook |
| API error rate | Requests (failed %) | > 5% | 5 min | Sev1 | Email |
| API latency | Server response time | P95 > 3000 ms | 10 min | Sev2 | Email |
| Unhandled exceptions | Exceptions count | > 20 | 10 min | Sev2 | Email |
| SQL DTU high | SQL DB DTU | > 80% | 10 min | Sev2 | Email |
| SQL storage high | SQL DB storage % | > 80% | 15 min | Sev3 | Email |

---

## Dashboards

Create one shared Azure dashboard with:

- API request volume (1h, 24h, 7d)
- API error rate by endpoint
- API P50 / P95 response time
- Dependency failures (SQL + external calls)
- SQL DTU and data size
- Health check availability status

---

## KQL Queries (Application Insights)

### Top failing endpoints (last 24h)

```kusto
requests
| where timestamp > ago(24h)
| where success == false
| summarize failures=count() by name, resultCode
| order by failures desc
```

### Slow endpoints P95 (last 24h)

```kusto
requests
| where timestamp > ago(24h)
| summarize p95=percentile(duration,95) by name
| order by p95 desc
```

### Frequent exceptions (last 24h)

```kusto
exceptions
| where timestamp > ago(24h)
| summarize total=count() by type, outerMessage
| order by total desc
```

---

## Alert Routing

Use one Azure Monitor Action Group:

- Primary channel: operations email distribution list
- Secondary channel: webhook for incident channel (Teams/Slack)
- Add escalation recipients for Sev0/Sev1

---

## Weekly Operations Checklist

- Review top 5 failing endpoints
- Review top 5 slow endpoints
- Check SQL DTU trend and storage trend
- Verify availability test had no blind spots
- Validate that alert rules are still actionable

