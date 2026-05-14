# Optional: verify last string literal on each ME* line in MapRegionsData.cs is in SpoilerEntranceIdDatabase.
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$dbPath = Join-Path $root "Services\SpoilerEntranceIdDatabase.cs"
$mapPath = Join-Path $root "Services\MapRegionsData.cs"
$db = @{}
Select-String -Path $dbPath -Pattern '"([A-Z][A-Z0-9_]+)"' -AllMatches | ForEach-Object {
    foreach ($m in $_.Matches) { $db[$m.Groups[1].Value] = $true }
}
$bad = New-Object System.Collections.Generic.List[string]
Select-String -Path $mapPath -Pattern '^\s*ME[A-Z]*\(' | ForEach-Object {
    $all = [regex]::Matches($_.Line, '"([^"]*)"')
    if ($all.Count -eq 0) { return }
    $last = $all[$all.Count - 1].Groups[1].Value
    if ($last -notmatch '^[A-Z][A-Z0-9_]*$') { return }
    if (-not $db.ContainsKey($last)) { $bad.Add($last) }
}
if ($bad.Count -eq 0) {
    Write-Host "OK: all ME* trailing ids exist in SpoilerEntranceIdDatabase."
    exit 0
}
Write-Host "Missing from database:"
$bad | Sort-Object -Unique | ForEach-Object { Write-Host "  $_" }
exit 1
