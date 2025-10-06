# Database Backups

This directory contains SQL Server database backups for the Hotshot Logistics project.

## Current Backup

- **File**: `hotshot_logistics_backup_20251006-022118.bak`
- **Database**: `hotshot_logistics`
- **Created**: October 6, 2025 at 02:21:18 AM
- **Size**: 9.56 MB
- **Container**: `hotshot_sqlserver`

## How to Restore the Database

### Option 1: Restore to the same container

```powershell
# Copy the backup file back to the container
docker cp "database-backups/hotshot_logistics_backup_20251006-022118.bak" "hotshot_sqlserver:/var/opt/mssql/backup/"

# Restore the database (this will overwrite the existing database)
docker exec hotshot_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${env:SQL_SA_PASSWORD}" -C -Q "RESTORE DATABASE [hotshot_logistics] FROM DISK = '/var/opt/mssql/backup/hotshot_logistics_backup_20251006-022118.bak' WITH REPLACE"
```

### Option 2: Restore to a new database name

```powershell
# Copy the backup file to the container
docker cp "database-backups/hotshot_logistics_backup_20251006-022118.bak" "hotshot_sqlserver:/var/opt/mssql/backup/"

# Restore to a new database name (e.g., hotshot_logistics_restored)
docker exec hotshot_sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${env:SQL_SA_PASSWORD}" -C -Q "RESTORE DATABASE [hotshot_logistics_restored] FROM DISK = '/var/opt/mssql/backup/hotshot_logistics_backup_20251006-022118.bak' WITH MOVE 'hotshot_logistics' TO '/var/opt/mssql/data/hotshot_logistics_restored.mdf', MOVE 'hotshot_logistics_log' TO '/var/opt/mssql/data/hotshot_logistics_restored.ldf'"
```

### Option 3: Restore to a different SQL Server instance

1. Copy the `.bak` file to your target SQL Server instance
2. Use SQL Server Management Studio (SSMS) or sqlcmd to restore:

```sql
RESTORE DATABASE [hotshot_logistics] 
FROM DISK = 'C:\path\to\hotshot_logistics_backup_20251006-022118.bak' 
WITH REPLACE
```

## Environment Information

- **Docker Container**: `hotshot_sqlserver`
- **SQL Server Version**: 2022-latest
- **Port**: 1433 (mapped to host)
- **SA Password**: Set via `SQL_SA_PASSWORD` environment variable

## Notes

- This backup was created using the native SQL Server BACKUP DATABASE command
- The backup is a full database backup including both data and log files
- The backup file is stored in git for version control of database states
- Always test restores in a development environment before applying to production