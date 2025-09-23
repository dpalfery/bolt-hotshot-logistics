param()

$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path '.').Path
$build = Join-Path $repo 'build-sa1100-output.txt'
if (-not (Test-Path $build)) { Write-Error "Build output file not found: $build" }

$saMatches = Select-String -Path $build -Pattern 'SA1100' -SimpleMatch -CaseSensitive

$pattern = '^(?<file>.+?\.cs)\((?<line>\d+),(?<col>\d+)\):\s+warning\s+SA1100:\s+(?<message>.+?)\s+\(https?://[^\)]*\)\s+\[(?<project>.+?\.csproj)\]'

$items = @()

foreach ($m in $saMatches) {
  $lineText = $m.Line
  if ($lineText -match $pattern) {
    $items += [PSCustomObject]@{
      RawBuildLine = $lineText
      File = $Matches['file']
      Line = [int]$Matches['line']
      Column = [int]$Matches['col']
      Message = $Matches['message']
      ProjectPath = $Matches['project']
    }
  }
}

$unique = $items | Sort-Object File,Line,Column,Message -Unique

$reportObjs = @()

foreach ($it in $unique) {
  $projectName = [System.IO.Path]::GetFileNameWithoutExtension($it.ProjectPath)
  $filePath = $it.File
  $filePath = $filePath -replace '/', '\'
  $rel = $filePath
  $repoPrefix = $repo + [System.IO.Path]::DirectorySeparatorChar
  if ($rel.ToLower().StartsWith($repoPrefix.ToLower())) {
    $rel = $rel.Substring($repoPrefix.Length)
  }
  $content = @()
  if (Test-Path -LiteralPath $filePath) {
    $content = Get-Content -LiteralPath $filePath
  }
  $start = [Math]::Max(1, $it.Line - 3)
  $end = if ($content.Count -gt 0) { [Math]::Min($content.Count, $it.Line + 3) } else { $it.Line + 3 }
  $snippet = @()
  for ($i = $start; $i -le $end; $i++) {
    if ($content.Count -ge $i) {
      $snippet += ("{0} | {1}" -f $i, $content[$i-1])
    }
  }

  $reportObjs += [PSCustomObject]@{
    project = $projectName
    relativeFilePath = $rel
    line = $it.Line
    column = $it.Column
    message = $it.Message
    rawBuildLine = $it.RawBuildLine
    snippet = $snippet
  }
}

$reportPath = Join-Path $repo 'sa1100-report.json'
$reportObjs | ConvertTo-Json -Depth 8 | Set-Content -Path $reportPath -Encoding UTF8

$total = $reportObjs.Count
$perProject = $reportObjs | Group-Object project | Sort-Object Count -Descending
$perFile = $reportObjs | Group-Object relativeFilePath | Sort-Object Count -Descending
$top10 = $perFile | Select-Object -First 10

$md = New-Object System.Text.StringBuilder
$null = $md.AppendLine('# SA1100 Summary')
$null = $md.AppendLine()
$null = $md.AppendLine("- Total SA1100 occurrences: $total")
$null = $md.AppendLine()
$null = $md.AppendLine('## Per-project counts')
foreach ($g in $perProject) { $null = $md.AppendLine("- $($g.Name): $($g.Count)") }
$null = $md.AppendLine()
$null = $md.AppendLine('## Per-file counts')
foreach ($g in $perFile) { $null = $md.AppendLine("- $($g.Name): $($g.Count)") }
$null = $md.AppendLine()
$null = $md.AppendLine('## Top 10 files by occurrences')
foreach ($g in $top10) { $null = $md.AppendLine("- $($g.Name): $($g.Count)") }

$mdPath = Join-Path $repo 'sa1100-summary.md'
$md.ToString() | Set-Content -Path $mdPath -Encoding UTF8