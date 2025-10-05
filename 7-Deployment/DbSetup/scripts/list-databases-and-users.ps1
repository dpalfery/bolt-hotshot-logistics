#!/usr/bin/env pwsh

# Script: list-databases-and-users.ps1
# Purpose: Connect to localhost SQL Server using SA credentials from the MSSQL_SA_PASSWORD
#          environment variable. Enumerate non-system databases and list database users
#          (including mapped server logins). Emit results as JSON to stdout.
# Security: Does not echo the SA password. Reads it from environment only.

param()

$sa = $env:MSSQL_SA_PASSWORD
if (-not $sa) {
  Write-Error "MSSQL_SA_PASSWORD environment variable is not set. Set it and re-run the script."
  exit 1
}

try {
  $connString = "Server=localhost;User ID=sa;Password=$($sa);Encrypt=False;TrustServerCertificate=True;"

  Add-Type -AssemblyName "System.Data"
  $results = @()

  # Get online, non-system databases
  $dbQuery = "SELECT name FROM sys.databases WHERE state = 0 AND name NOT IN ('master','tempdb','model','msdb');"

  $connection = New-Object System.Data.SqlClient.SqlConnection $connString
  $connection.Open()

  $command = $connection.CreateCommand()
  #!/usr/bin/env pwsh

  # Script: list-databases-and-users.ps1
  # Purpose: Connect to localhost SQL Server using SA credentials from the MSSQL_SA_PASSWORD
  #          environment variable. Enumerate non-system databases and list database users
  #          (including mapped server logins). Emit results as JSON to stdout.
  # Security: Does not echo the SA password. Reads it from environment only.

  param()

  $sa = $env:MSSQL_SA_PASSWORD
  if (-not $sa) {
    Write-Error "MSSQL_SA_PASSWORD environment variable is not set. Set it and re-run the script."
    exit 1
  }

  try {
    $connString = "Server=localhost;User ID=sa;Password=$($sa);Encrypt=False;TrustServerCertificate=True;"

    Add-Type -AssemblyName "System.Data"
    $results = @()

    # Get online, non-system databases
    $dbQuery = "SELECT name FROM sys.databases WHERE state = 0 AND name NOT IN ('master','tempdb','model','msdb');"

    $connection = New-Object System.Data.SqlClient.SqlConnection $connString
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = $dbQuery

    $reader = $command.ExecuteReader()
    $dbNames = @()
    while ($reader.Read()) {
      $dbNames += $reader.GetString(0)
    }
    $reader.Close()

    foreach ($db in $dbNames) {
      # Query database users mapped to server logins and local users
      $userQuery = @"
  USE [$db];
  SELECT
    dp.name AS DatabaseUser,
    dp.type_desc AS UserType,
    sp.name AS LoginName
  FROM sys.database_principals dp
  LEFT JOIN sys.server_principals sp ON dp.sid = sp.sid
  WHERE dp.principal_id > 4
    AND dp.type_desc NOT IN ('DATABASE_ROLE','APPLICATION_ROLE')
  ORDER BY dp.name;
  "@

      $command.CommandText = $userQuery
      $reader = $command.ExecuteReader()
      $users = @()
      while ($reader.Read()) {
        $users += [PSCustomObject]@{
          DatabaseUser = $reader["DatabaseUser"]
          UserType     = $reader["UserType"]
          LoginName    = $reader["LoginName"]
        }
      }
      $reader.Close()

      $results += [PSCustomObject]@{
        Database = $db
        Users    = $users
      }
    }

    $connection.Close()

    # Output JSON
    $json = $results | ConvertTo-Json -Depth 6
    Write-Output $json
  }
  catch {
    Write-Error ("Error: {0}" -f $_.Exception.Message)
    exit 2
  }