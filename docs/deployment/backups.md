# Backup and restore runbook

Back up two independent assets: the SQL Server database and `PhotoStorage:RootPath`. The latter contains original photographs and generated variants; originals are the priority because variants can be regenerated.

## Schedule and retention

- Database: nightly full backup; retain 30 daily backups and 12 monthly backups.
- Photo storage: nightly incremental backup plus a weekly full backup; retain the same schedule.
- Keep a copy in a separate account or region from the Plesk server. A backup on the same disk does not protect against host loss.
- Encrypt backup archives at rest and restrict restore access to the site owner.

Configure these tasks in the hosting provider's scheduler or a managed backup service. Record the exact destination, job owner, and failure-notification address in the provider—not in this repository.

## Restore drill (quarterly)

1. Restore the latest database backup to a new, isolated database.
2. Restore the matching photo-storage backup to a new directory.
3. Point a temporary staging configuration at those restored locations.
4. Check the health endpoint, at least one album, original metadata, and several image variants.
5. Record the backup date, restore duration, verifier, and any corrective action.

Do not overwrite the live database or photo directory during a drill. A backup is considered valid only after this restore check succeeds.
