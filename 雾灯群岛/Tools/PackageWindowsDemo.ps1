param(
    [string]$Version = '0.2.14'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem | Out-Null
$projectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$releaseRoot = Join-Path $projectRoot 'Builds\ReleaseCandidate'
$distributionRoot = Join-Path $projectRoot 'Builds\Distribution'
$archivePath = Join-Path $distributionRoot "FoglightIslesDemo-Windows-$Version.zip"

$required = @(
    'FoglightIslesDemo.exe',
    'FoglightIslesDemo_Data',
    'MonoBleedingEdge',
    'UnityPlayer.dll',
    'UnityCrashHandler64.exe'
)
$optional = @('D3D12')
$testerFiles = @('PLAYTEST.txt', 'FEEDBACK_TEMPLATE.md', 'PLAYTEST_START.cmd', 'COLLECT_FEEDBACK.cmd', 'Tools/CollectPlaytestFeedback.ps1', 'Tools/RotatePlaytestLog.ps1')
foreach ($name in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $releaseRoot $name))) {
        throw "Missing release file: $name"
    }
}
foreach ($name in $testerFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $projectRoot $name))) { throw "Missing tester document: $name" }
}

$managedAssembly = Join-Path $releaseRoot 'FoglightIslesDemo_Data\Managed\Assembly-CSharp.dll'
if (-not (Test-Path -LiteralPath $managedAssembly)) { throw 'Missing managed game assembly for build identification.' }
$assemblyHash = (Get-FileHash -LiteralPath $managedAssembly -Algorithm SHA256).Hash
$buildUtc = (Get-Item -LiteralPath $managedAssembly).LastWriteTimeUtc
$buildId = '{0}-{1}' -f $buildUtc.ToString('yyyyMMddHHmmss'), $assemblyHash.Substring(0, 12)
$buildInfo = @(
    "Foglight Isles Demo Windows $Version"
    "Build ID: $buildId"
    "Managed assembly SHA-256: $assemblyHash"
    "Build UTC: $($buildUtc.ToString('yyyy-MM-ddTHH:mm:ssZ'))"
    "Package UTC: $((Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ'))"
) -join [Environment]::NewLine

New-Item -ItemType Directory -Path $distributionRoot -Force | Out-Null
$stream = [System.IO.File]::Open($archivePath, [System.IO.FileMode]::Create)
try {
    $archive = [System.IO.Compression.ZipArchive]::new($stream, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($name in ($required + $optional)) {
            $source = Join-Path $releaseRoot $name
            if (-not (Test-Path -LiteralPath $source)) { continue }
            $item = Get-Item -LiteralPath $source
            $files = if ($item.PSIsContainer) { Get-ChildItem -LiteralPath $source -File -Recurse -Force } else { @($item) }
            foreach ($file in $files) {
                $entryName = $file.FullName.Substring($releaseRoot.Length + 1).Replace('\', '/')
                [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                    $archive, $file.FullName, $entryName, [System.IO.Compression.CompressionLevel]::Optimal
                ) | Out-Null
            }
        }
        foreach ($name in $testerFiles) {
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $archive, (Join-Path $projectRoot $name), $name, [System.IO.Compression.CompressionLevel]::Optimal
            ) | Out-Null
        }
        $infoEntry = $archive.CreateEntry('BUILD_INFO.txt', [System.IO.Compression.CompressionLevel]::Optimal)
        $infoWriter = [System.IO.StreamWriter]::new($infoEntry.Open(), [System.Text.UTF8Encoding]::new($false))
        try { $infoWriter.Write($buildInfo) }
        finally { $infoWriter.Dispose() }
    }
    finally { $archive.Dispose() }
}
finally { $stream.Dispose() }

$readBack = [System.IO.Compression.ZipFile]::OpenRead($archivePath)
try {
    $entries = @($readBack.Entries | ForEach-Object { $_.FullName })
    if ('FoglightIslesDemo.exe' -notin $entries -or 'UnityPlayer.dll' -notin $entries -or 'PLAYTEST.txt' -notin $entries -or 'FEEDBACK_TEMPLATE.md' -notin $entries -or 'PLAYTEST_START.cmd' -notin $entries -or 'COLLECT_FEEDBACK.cmd' -notin $entries -or 'Tools/CollectPlaytestFeedback.ps1' -notin $entries -or 'Tools/RotatePlaytestLog.ps1' -notin $entries -or 'BUILD_INFO.txt' -notin $entries) {
        throw 'Package verification failed: required executable files are absent.'
    }
    if ($entries | Where-Object { $_ -match 'DoNotShip|review-.*\.png' }) {
        throw 'Package verification failed: review or debug artifacts were included.'
    }
    Write-Output "[FoglightIsles Package] PASS — build $buildId; $archivePath ($($entries.Count) files, $([math]::Round((Get-Item -LiteralPath $archivePath).Length / 1MB, 1)) MB)."
}
finally { $readBack.Dispose() }
