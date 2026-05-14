# Rebuild Services/SpoilerEntranceIdDatabase.cs from OoTMM upstream entrances.yml
# plus any parenthetical ids from "spoiler logs from feedback/*.txt" (union).
#
# Upstream: https://github.com/OoTMM/OoTMM/blob/master/data/defs/entrances.yml
# Run from repo root:  powershell -File tools/RegenerateSpoilerEntranceIdDatabaseFromUpstream.ps1

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
$csproj = Join-Path $repoRoot "OoTMMTracker.csproj"
if (-not (Test-Path $csproj)) {
    Write-Error "Run from OoTMMTracker repo (expected $csproj)."
}

$out = Join-Path $repoRoot "Services\SpoilerEntranceIdDatabase.cs"
$url = "https://raw.githubusercontent.com/OoTMM/OoTMM/master/data/defs/entrances.yml"

$set = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)

Write-Host "Fetching $url ..."
$yaml = (Invoke-WebRequest -Uri $url -UseBasicParsing).Content
foreach ($m in [regex]::Matches($yaml, '(?m)^([A-Z][A-Z0-9_]*)\s*:')) {
    [void]$set.Add($m.Groups[1].Value)
}
Write-Host "  Upstream entrance defs: $($set.Count)"

$feedback = Join-Path $repoRoot "spoiler logs from feedback"
$fromLogs = 0
if (Test-Path $feedback) {
    Get-ChildItem -Path $feedback -Filter "*.txt" -ErrorAction SilentlyContinue | ForEach-Object {
        $in = $false
        foreach ($line in [System.IO.File]::ReadLines($_.FullName)) {
            $t = $line.Trim()
            if ($t -ceq "Entrances") { $in = $true; continue }
            if (-not $in) { continue }
            if ($t -match '^(Hints|Location List|Tricks|Settings|World Flags|Starting Items|Special Conditions)\s*$') {
                $in = $false
                continue
            }
            if ($t -notmatch '->') { continue }
            foreach ($m in [regex]::Matches($line, '\(([A-Za-z][A-Za-z0-9_]*)\)')) {
                $id = $m.Groups[1].Value
                if ($set.Add($id)) { $fromLogs++ }
            }
        }
    }
    Write-Host "  Extra ids from spoiler logs (not in yaml): $fromLogs"
}

Write-Host "  Total ids: $($set.Count)"

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("// Auto-built: all keys from OoTMM data/defs/entrances.yml")
[void]$sb.AppendLine("// https://github.com/OoTMM/OoTMM/blob/master/data/defs/entrances.yml")
[void]$sb.AppendLine("// UNION parenthetical tokens from Entrances in spoiler logs from feedback/*.txt")
[void]$sb.AppendLine("// Regenerate: powershell -File tools/RegenerateSpoilerEntranceIdDatabaseFromUpstream.ps1")
[void]$sb.AppendLine("using System;")
[void]$sb.AppendLine("using System.Collections.Generic;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("namespace OoTMMTracker.Services")
[void]$sb.AppendLine("{")
[void]$sb.AppendLine("    /// <summary>Canonical OoTMM entrance ids (upstream <c>entrances.yml</c>) plus any ids seen only in sampled spoilers.</summary>")
[void]$sb.AppendLine("    public static class SpoilerEntranceIdDatabase")
[void]$sb.AppendLine("    {")
[void]$sb.AppendLine("        private static readonly HashSet<string> Ids = new(StringComparer.OrdinalIgnoreCase)")
[void]$sb.AppendLine("        {")

foreach ($id in ($set | Sort-Object)) {
    $esc = $id.Replace('\', '\\').Replace('"', '\"')
    [void]$sb.AppendLine('            "' + $esc + '",')
}

[void]$sb.AppendLine("        };")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("        /// <summary>True if this id is defined upstream or appeared in a sampled spoiler Entrances section.</summary>")
[void]$sb.AppendLine("        public static bool IsKnownId(string? id) =>")
[void]$sb.AppendLine("            !string.IsNullOrEmpty(id) && Ids.Contains(EntranceMapNavigation.NormalizeEntranceIdToken(id));")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("        public static int KnownIdCount => Ids.Count;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("        public static IEnumerable<string> GetKnownIds() => Ids;")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("}")

[System.IO.File]::WriteAllText($out, $sb.ToString())
Write-Host "Wrote $out"
