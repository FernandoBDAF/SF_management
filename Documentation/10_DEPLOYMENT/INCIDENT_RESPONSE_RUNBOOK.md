# Incident Response Runbook

This runbook defines first-response actions for production incidents.

---

## Severity Levels

| Severity | Definition | Target first response |
|---------|------------|-----------------------|
| Sev0 | Production outage or data risk | 15 minutes |
| Sev1 | Major degradation of critical workflows | 30 minutes |
| Sev2 | Partial degradation with workaround | 2 hours |

---

## Initial Response (first 15 minutes)

1. Confirm alert validity (health check, errors, latency)
2. Classify severity
3. Assign incident owner
4. Capture timestamp and impacted services
5. Post first status update to operations channel

---

## API Down Procedure

1. Check Azure App Service status and recent deployments
2. Check `/health` endpoint response
3. Check Application Insights exceptions and failures
4. If caused by latest deployment, trigger rollback
5. Confirm recovery and keep incident open for monitoring

---

## Auth0 Incident Procedure

Symptoms:

- login failures
- token refresh failures
- 401 spikes across all endpoints

Steps:

1. Check Auth0 status page and tenant logs
2. Validate API can still process already-authenticated tokens
3. Confirm Auth0 app credentials in Azure/Vercel settings
4. Communicate impact and workaround (if any)
5. Track recovery and monitor auth error rate

---

## Database Incident Procedure

Symptoms:

- connection timeout spikes
- SQL dependency failures
- DTU saturation alerts

Steps:

1. Check SQL metrics (DTU, storage, failed connections)
2. Check recent migrations
3. Identify heavy queries via Query Performance Insight
4. Apply mitigation (scale up temporarily or rollback app)
5. If data corruption risk exists, start restore procedure

---

## Closure Checklist

- Service metrics returned to baseline
- User-facing impact window recorded
- Root cause and contributing factors documented
- Action items created with owners and deadlines
- Runbooks updated with lessons learned

