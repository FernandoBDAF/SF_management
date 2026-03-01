# Deployment and Rollback Runbook

This runbook standardizes safe deployments to HMG and production.

---

## Deployment Flow

1. Merge changes to `hmg`
2. Confirm HMG workflow succeeded
3. Validate HMG smoke test checklist
4. Review migration artifact (`migration.sql`) if schema changed
5. Merge to `main`
6. Confirm production workflow succeeded
7. Validate production smoke test checklist

---

## HMG Smoke Test Checklist

- `GET /health` returns 200
- Authenticated login works from HMG frontend
- Dashboard and main pages load
- Core transaction creation flow works
- No high-severity errors in logs

---

## Production Smoke Test Checklist

- `GET /health` returns 200
- Login and session refresh work
- Core read endpoints behave normally
- At least one critical write flow succeeds
- No spike in 5xx or latency alerts

---

## Rollback Decision Matrix

| Situation | Preferred rollback |
|----------|---------------------|
| App-level bug without schema change | Redeploy previous successful build |
| App-level bug after main merge | `git revert` and redeploy |
| Schema issue after migration | Restore DB to prior point-in-time and redeploy previous app |

---

## Fast Rollback Procedure

1. Stop further deployments
2. Identify last known good deployment run
3. Redeploy previous artifact from GitHub Actions or Azure deployment history
4. If schema regression exists, execute DB restore procedure from `DATABASE_OPERATIONS_RUNBOOK.md`
5. Validate `/health` and critical business flows
6. Keep incident notes for postmortem

---

## Post-Rollback Tasks

- Open incident summary with root cause
- Add regression test or guardrail
- Update deployment checklist/runbook to prevent recurrence

