$core = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$base = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Base.dll')
foreach ($a in @($base, $core)) {
  try {
    foreach ($t in $a.GetTypes()) {
      if ($t.Name -eq 'AppearanceType') {
        Write-Output ("AppearanceType: " + (([Enum]::GetNames($t)) -join ', '))
      }
    }
  } catch {
    foreach ($ex in $_.Exception.LoaderExceptions) { }
    foreach ($t in $a.DefinedTypes) {
      if ($t.Name -eq 'AppearanceType') {
        Write-Output ("AppearanceType: " + (([Enum]::GetNames($t)) -join ', '))
      }
    }
  }
}
$stage = $core.GetType('LBoL.Core.Stage')
$m = $stage.GetMethod('GetBossExhibits', [Reflection.BindingFlags]'Public,NonPublic,Instance')
if ($m) { Write-Output "GetBossExhibits found" } else { Write-Output "GetBossExhibits missing" }
