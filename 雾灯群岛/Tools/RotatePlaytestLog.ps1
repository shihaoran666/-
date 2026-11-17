param(
    [string]$Root = (Join-Path $PSScriptRoot '..')
)

$ErrorActionPreference = 'Stop'
$rootPath = (Resolve-Path -LiteralPath $Root).Path
$current = Join-Path $rootPath 'playtest.log'
if (-not (Test-Path -LiteralPath $current -PathType Leaf)) { return }

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$name = "playtest-$stamp.log"
$archive = Join-Path $rootPath $name
if (Test-Path -LiteralPath $archive) {
    $name = "playtest-$stamp-$([guid]::NewGuid().ToString('N').Substring(0, 6)).log"
    $archive = Join-Path $rootPath $name
}
Move-Item -LiteralPath $current -Destination $archive -ErrorAction Stop
Write-Output "Preserved previous playtest log: $name"
