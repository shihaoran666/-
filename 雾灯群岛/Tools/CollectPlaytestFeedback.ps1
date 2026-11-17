param(
    [string]$Root = (Join-Path $PSScriptRoot '..'),
    [string]$ScreenshotDirectory = (Join-Path $env:USERPROFILE 'AppData\LocalLow\Foglight Isles\Foglight Isles Demo\Screenshots'),
    [switch]$IncludePlayerLog
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem | Out-Null
$rootPath = (Resolve-Path -LiteralPath $Root).Path
$required = @('BUILD_INFO.txt', 'FEEDBACK_TEMPLATE.md', 'playtest.log')
foreach ($name in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $rootPath $name) -PathType Leaf)) {
        throw "Missing $name. Run PLAYTEST_START.cmd, exit the game, then collect feedback from the extracted demo folder."
    }
}

$files = [System.Collections.Generic.List[object]]::new()
foreach ($name in $required) {
    $files.Add([pscustomobject]@{ Path = (Join-Path $rootPath $name); Entry = $name })
}
$logPath = Join-Path $rootPath 'playtest.log'
$priorLogs = @(Get-ChildItem -LiteralPath $rootPath -File | Where-Object {
    $_.Name -match '^playtest-\d{8}-\d{6}-\d{3}(?:-[a-f0-9]{6})?\.log$'
} | Sort-Object Name)
foreach ($priorLog in $priorLogs) {
    $files.Add([pscustomobject]@{ Path = $priorLog.FullName; Entry = $priorLog.Name })
}
$logPaths = @($priorLogs.FullName) + @($logPath)

# Include only screenshots explicitly logged by these playtest runs. Do not sweep the
# player's whole persistent-data directory or unrelated screenshots.
$screenshotCount = 0
if (Test-Path -LiteralPath $ScreenshotDirectory -PathType Container) {
    $screenshotRoot = (Resolve-Path -LiteralPath $ScreenshotDirectory).Path
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($runLog in $logPaths) {
      foreach ($line in [System.IO.File]::ReadAllLines($runLog)) {
        if ($line -notmatch '\[FoglightIsles Screenshot\] PASS .+ saved to (.+\.png)\s*$') { continue }
        $candidate = $Matches[1].Trim()
        if (-not [System.IO.Path]::IsPathRooted($candidate)) { continue }
        $candidatePath = [System.IO.Path]::GetFullPath($candidate)
        if (-not $candidatePath.StartsWith($screenshotRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) { continue }
        if (-not (Test-Path -LiteralPath $candidatePath -PathType Leaf)) { continue }
        $name = [System.IO.Path]::GetFileName($candidatePath)
        if ($name -notmatch '^Foglight-\d{8}-\d{6}-\d{3}-\d{3}\.png$') { continue }
        if (-not $seen.Add($name)) { continue }
        $files.Add([pscustomobject]@{ Path = $candidatePath; Entry = "Screenshots/$name" })
        $screenshotCount++
      }
    }
}

if ($IncludePlayerLog) {
    $playerLog = Join-Path (Split-Path -Parent $ScreenshotDirectory) 'Player.log'
    if (Test-Path -LiteralPath $playerLog -PathType Leaf) {
        $files.Add([pscustomobject]@{ Path = $playerLog; Entry = 'Player.log' })
    }
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$archivePath = Join-Path $rootPath "Foglight-Feedback-$stamp.zip"
if (Test-Path -LiteralPath $archivePath) {
    $archivePath = Join-Path $rootPath "Foglight-Feedback-$stamp-$([guid]::NewGuid().ToString('N').Substring(0, 6)).zip"
}
$stream = [System.IO.File]::Open($archivePath, [System.IO.FileMode]::CreateNew)
try {
    $archive = [System.IO.Compression.ZipArchive]::new($stream, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($file in $files) {
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $archive, $file.Path, $file.Entry, [System.IO.Compression.CompressionLevel]::Optimal
            ) | Out-Null
        }
        $contents = @(
            'Foglight Isles playtest feedback bundle'
            'Review these files before sending. This helper does not upload them.'
            'Complete FEEDBACK_TEMPLATE.md before running this helper, or replace its blank copy in this ZIP before sending.'
            "Playtest logs included: $($logPaths.Count)"
            "Screenshots from these logs: $screenshotCount"
            "Player.log included by request: $IncludePlayerLog"
        ) -join [Environment]::NewLine
        $entry = $archive.CreateEntry('CONTENTS.txt')
        $writer = [System.IO.StreamWriter]::new($entry.Open(), [System.Text.UTF8Encoding]::new($false))
        try { $writer.Write($contents) } finally { $writer.Dispose() }
    }
    finally { $archive.Dispose() }
}
finally { $stream.Dispose() }
Write-Output "Feedback ZIP: $archivePath"
Write-Output "Included $($files.Count) files, including $screenshotCount screenshots. Review and complete FEEDBACK_TEMPLATE.md before sending."
