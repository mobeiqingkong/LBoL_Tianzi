param(
    [Parameter(Mandatory = $true)][string]$ProjectDir,
    [string]$OutZip
)

$ErrorActionPreference = 'Stop'

$proj = [System.IO.Path]::GetFullPath($ProjectDir)
$asm = 'TianziMod_windows'
if (-not $OutZip) {
    $OutZip = Join-Path $proj ($asm + '-thunderstore.zip')
}
# NOTE: keep this file pure ASCII. Windows PowerShell 5.1 decodes a BOM-less
# .ps1 as ANSI/GBK, so any non-ASCII text here gets garbled and can break parsing.
# Do not hardcode bin\Debug: on a Release build it either misses the file, or
# packs a stale Debug artifact into the zip. Take the newest one under bin\.
$dll = Get-ChildItem -Path (Join-Path $proj 'bin') -Filter "$asm.dll" -Recurse -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1 -ExpandProperty FullName
$dirRes = Join-Path $proj 'DirResources'
$stage = Join-Path $proj '_pack_stage'

if (-not $dll) {
    Write-Host "!! DLL not found: $proj\bin\**\$asm.dll -- build first"
    exit 1
}
Write-Host "[pack] using: $dll"
if (-not (Test-Path -LiteralPath $dirRes)) {
    Write-Host "!! DirResources not found: $dirRes"
    exit 1
}

if (Test-Path -LiteralPath $stage) {
    Remove-Item -LiteralPath $stage -Recurse -Force
}
New-Item -ItemType Directory -Path $stage | Out-Null

Copy-Item -LiteralPath $dll -Destination (Join-Path $stage "$asm.dll")
foreach ($name in @('manifest.json', 'README.md')) {
    $p = Join-Path $proj $name
    if (Test-Path -LiteralPath $p) {
        Copy-Item -LiteralPath $p -Destination (Join-Path $stage $name)
    }
}
Copy-Item -Path (Join-Path $dirRes '*') -Destination $stage -Force

if (Test-Path -LiteralPath $OutZip) {
    try {
        Remove-Item -LiteralPath $OutZip -Force -ErrorAction Stop
    }
    catch {
        Write-Host "!! Cannot overwrite the old zip (locked by another process): $OutZip"
        Write-Host "   Close whatever holds it (archiver / mod manager / previewer) and rebuild."
        exit 1
    }
}
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $OutZip -Force
$count = (Get-ChildItem -LiteralPath $stage -File).Count
Remove-Item -LiteralPath $stage -Recurse -Force

Write-Host "[pack] $OutZip"
Write-Host "[pack] entries $count  size $((Get-Item -LiteralPath $OutZip).Length)"
