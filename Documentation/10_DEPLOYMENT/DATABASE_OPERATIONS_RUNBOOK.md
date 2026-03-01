# Database Operations Runbook

This runbook defines backup, restore, and migration safety procedures for Azure SQL databases used by SF Management.

---

## Scope

- Production DB
- HMG DB
- Migration execution policy
- Restore testing policy

---

## Backup Policy

Azure SQL automated backups are enabled by default.

### Required verification (monthly)

1. Confirm Point-In-Time Restore retention period
2. Confirm backup storage growth and limits
3. Confirm production and HMG are in expected pricing tiers

### Long-Term Retention (recommended)

Enable LTR for production:

- Weekly backup retention: 4-8 weeks
- Monthly backup retention: 12 months

This provides safer rollback options for delayed-detection incidents.

---

## Restore Procedure (to HMG)

Use this when validating a restore process without impacting production.

1. Azure Portal -> SQL Database -> Production DB -> Restore
2. Select **Point-in-time restore**
3. Choose timestamp and target server
4. Restore into a dedicated test database (example: `sfmanagement-restore-test`)
5. Validate:
   - schema version
   - key table counts
   - login/connectivity from HMG API (optional)
6. Remove test database after validation

---

## Migration Policy

### Production

- Do **not** rely on automatic startup migrations
- Generate and review idempotent script first:

```bash
dotnet ef migrations script --project SFManagement.csproj --idempotent --output migration.sql
```

- Keep generated script as CI artifact
- Apply only after HMG validation and reviewer approval

### HMG

- Deploy first to HMG
- Validate health endpoint and smoke tests
- Validate migration success before promoting to production

---

## Quarterly DR Test

Run once per quarter:

1. Perform point-in-time restore to test DB
2. Run smoke read queries
3. Capture total restore time and blockers
4. Update this runbook with findings

---

## SQL Monitoring Thresholds

| Metric | Threshold | Action |
|-------|-----------|--------|
| DTU | > 80% for 10 min | Investigate query hotspots and burst traffic |
| Storage | > 80% of allocated | Plan scale-up or data lifecycle work |
| Failed connections | Sustained increase | Check firewall, connection strings, transient failures |

