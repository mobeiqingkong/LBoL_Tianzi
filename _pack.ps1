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
$dll = Join-Path $proj "bin\Debug\netstandard2.1\$asm.dll"
$dirRes = Join-Path $proj 'DirResources'
$stage = Join-Path $proj '_pack_stage'

if (-not (Test-Path -LiteralPath $dll)) {
    Write-Host "!! 找不到 DLL: $dll"
    exit 1
}
if (-not (Test-Path -LiteralPath $dirRes)) {
    Write-Host "!! 找不到 DirResources 目录: $dirRes"
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
    Remove-Item -LiteralPath $OutZip -Force
}
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $OutZip -Force
$count = (Get-ChildItem -LiteralPath $stage -File).Count
Remove-Item -LiteralPath $stage -Recurse -Force

Write-Host "[pack] $OutZip"
Write-Host "[pack] 条目 $count  大小 $((Get-Item -LiteralPath $OutZip).Length)"
