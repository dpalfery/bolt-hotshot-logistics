-- list-databases-and-users.sql
-- Purpose: enumerate non-system databases and list database users and mapped server logins.
-- Run in the master context (e.g. in SSMS) as a login with sufficient privileges (sa).



DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql += N'
SELECT
    DBName = N' + QUOTENAME(name,'''') + N',
    DatabaseUser = dp.name,
    UserType = dp.type_desc,
    LoginName = sp.name
FROM ' + QUOTENAME(name) + N'.sys.database_principals dp
LEFT JOIN sys.server_principals sp ON dp.sid = sp.sid
WHERE dp.principal_id > 4
  AND dp.type_desc NOT IN (''DATABASE_ROLE'',''APPLICATION_ROLE'')
UNION ALL
'
FROM sys.databases
WHERE state = 0
  AND name NOT IN (N'master',N'tempdb',N'model',N'msdb');

-- list-databases-and-users.sql
-- Purpose: enumerate non-system databases and list database users and mapped server logins.
-- Run in the master context (e.g. in SSMS) as a login with sufficient privileges (sa).



DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql += N'
SELECT
    DBName = N' + QUOTENAME(name,'''') + N',
    DatabaseUser = dp.name,
    UserType = dp.type_desc,
    LoginName = sp.name
FROM ' + QUOTENAME(name) + N'.sys.database_principals dp
LEFT JOIN sys.server_principals sp ON dp.sid = sp.sid
WHERE dp.principal_id > 4
  AND dp.type_desc NOT IN (''DATABASE_ROLE'',''APPLICATION_ROLE'')
UNION ALL
'
FROM sys.databases
WHERE state = 0
  AND name NOT IN (N'master',N'tempdb',N'model',N'msdb');

IF LEN(@sql) = 0
BEGIN
    PRINT N'No user databases found.';
END
ELSE
BEGIN
    -- Remove trailing UNION ALL
    SET @sql = LEFT(@sql, LEN(@sql) - 10);

    -- Wrap the unioned queries for ordering
    SET @sql = N'SELECT * FROM ( ' + @sql + N' ) AS t ORDER BY DBName, DatabaseUser;';

    EXEC sp_executesql @sql;
END

DECLARE @sql NVARCHAR(MAX) = N'';
-- 1) Create server-level login (run in master)
CREATE LOGIN [hotshot_user]
WITH PASSWORD = N'somevalue',
     CHECK_EXPIRATION = OFF,  -- or ON to enforce password expiration policies
     CHECK_POLICY = ON;       -- enable Windows password policy checks (ON recommended)

-- 2) Create database user (run in the target database, e.g. hotshot_logistic)
USE [hotshot_logistics];
CREATE USER [hotshot_user] FOR LOGIN [hotshot_user];

-- 3) Grant least-privilege rights the application needs (example: data-level only)
GRANT CONNECT TO [hotshot_user];
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[dbo] TO [hotshot_user];

-- Optionally, if the app needs to execute stored procs in dbo:
-- GRANT EXECUTE ON SCHEMA::[dbo] TO [hotshot_user];


-- Run in the target database (e.g. hotshot_logistic)
-- Create a role that encapsulates the rule
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'hotshot_app_role' AND type = 'R')
BEGIN
    CREATE ROLE [hotshot_app_role];
END;
GO

-- 1) Grant SELECT on all views in dbo (read views)

SELECT @sql = @sql + N'GRANT SELECT ON ' + QUOTENAME(s.name) + N'.' + QUOTENAME(v.name) + N' TO [hotshot_app_role];' + CHAR(13)
FROM sys.views v
JOIN sys.schemas s ON v.schema_id = s.schema_id
WHERE s.name = 'dbo'  -- adjust schema filter if needed

IF @sql <> N'' EXEC sp_executesql @sql;
GO
DECLARE @sql NVARCHAR(MAX) = N'';
-- 2) Grant SELECT, INSERT, UPDATE, DELETE on all user tables in dbo (read/write tables)
SET @sql = N'';
SELECT @sql = @sql + N'GRANT SELECT, INSERT, UPDATE, DELETE ON ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' TO [hotshot_app_role];' + CHAR(13)
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = 'dbo'  -- adjust schema filter if needed

IF @sql <> N'' EXEC sp_executesql @sql;
GO

-- 3) Grant EXECUTE on the dbo schema (exec stored procedures in dbo)
GRANT EXECUTE ON SCHEMA::[dbo] TO [hotshot_app_role];
GO

-- 4) Add existing database user hotshot_user to the role (idempotent)
-- Note: this checks the user exists first, and only adds if not already a member.
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'hotshot_user')
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.database_role_members drm
        JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
        JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id
        WHERE r.name = N'hotshot_app_role' AND m.name = N'hotshot_user'
    )
    BEGIN
        ALTER ROLE [hotshot_app_role] ADD MEMBER [hotshot_user];
    END
END
ELSE
BEGIN
    PRINT N'User hotshot_user does not exist in this database. Create the user (or contained user) first, then re-run this script to add to the role.';
END
GO