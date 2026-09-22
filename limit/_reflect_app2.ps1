$path = 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll'
if (-not (Test-Path $path)) { $path = 'D:\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll' }
if (-not (Test-Path $path)) {
  Get-ChildItem -Path D:\ -Filter LBoL.Core.dll -Recurse -ErrorAction SilentlyContinue | Select-Object -First 3 FullName
  exit
}
Write-Output "Using $path"
$core = [Reflection.Assembly]::LoadFrom($path)
$stage = $core.GetType('LBoL.Core.Stage')
$m = $stage.GetMethod('GetBossExhibits', [Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')
Write-Output $m
# list AppearanceType
foreach ($dll in @('LBoL.Base.dll','LBoL.ConfigData.dll','LBoL.Core.dll')) {
  $p = Join-Path (Split-Path $path) $dll
  $a = [Reflection.Assembly]::LoadFrom($p)
  try { $types = $a.GetTypes() } catch { $types = $a.GetTypes() }
  foreach ($t in $types) {
    if ($t.Name -match 'Appearance') {
      Write-Output $t.FullName
      if ($t.IsEnum) { Write-Output (([Enum]::GetNames($t)) -join ', ') }
    }
  }
}
