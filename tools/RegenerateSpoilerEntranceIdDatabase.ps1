# Rebuild Services/SpoilerEntranceIdDatabase.cs from spoiler logs only (no upstream yaml).
# Prefer canonical regen from OoTMM repo: tools/RegenerateSpoilerEntranceIdDatabaseFromUpstream.ps1
# Run from repo root:  powershell -File tools/RegenerateSpoilerEntranceIdDatabase.ps1

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
$csproj = Join-Path $repoRoot "OoTMMTracker.csproj"
if (-not (Test-Path $csproj)) {
    Write-Error "Run this script from the OoTMMTracker repo (expected $csproj)."
}

$feedback = Join-Path $repoRoot "spoiler logs from feedback"
$out = Join-Path $repoRoot "Services\SpoilerEntranceIdDatabase.cs"

if (-not (Test-Path $feedback)) {
    Write-Error "Folder not found: $feedback"
}

$set = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
Get-ChildItem -Path $feedback -Filter "*.txt" | ForEach-Object {
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
            [void]$set.Add($m.Groups[1].Value)
        }
    }
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("// Auto-built union of parenthetical entrance ids from all Entrances sections in")
[void]$sb.AppendLine("// spoiler logs under ""spoiler logs from feedback/*.txt"". Regenerate: tools/RegenerateSpoilerEntranceIdDatabase.ps1")
[void]$sb.AppendLine("using System;")
[void]$sb.AppendLine("using System.Collections.Generic;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("namespace OoTMMTracker.Services")
[void]$sb.AppendLine("{")
[void]$sb.AppendLine("    /// <summary>Known OoTMM spoiler entrance ids (tokens inside parentheses in Entrances rows).</summary>")
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
[void]$sb.AppendLine("        /// <summary>True if this id appears as a parenthetical token in any sampled spoiler Entrances section.</summary>")
[void]$sb.AppendLine("        public static bool IsKnownId(string? id) =>")
[void]$sb.AppendLine("            !string.IsNullOrEmpty(id) && Ids.Contains(EntranceMapNavigation.NormalizeEntranceIdToken(id));")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("        public static int KnownIdCount => Ids.Count;")
[void]$sb.AppendLine("    }")
[void]$sb.AppendLine("}")

[System.IO.File]::WriteAllText($out, $sb.ToString())
Write-Host "Wrote $out ($($set.Count) ids)."
