Get-ChildItem 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\*.dll' | ForEach-Object {
  try { [void][Reflection.Assembly]::LoadFrom($_.FullName) } catch {}
}
$pres = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'LBoL.Presentation' } | Select-Object -First 1
$types = @()
try { $types = $pres.GetTypes() } catch [System.Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ -ne $null } }
foreach ($name in @('LBoL.Presentation.Units.UnitView','LBoL.Presentation.Units.EnemyUnitView','LBoL.Presentation.Units.GameDirector')) {
  $t = $types | Where-Object { $_.FullName -eq $name } | Select-Object -First 1
  if (-not $t) { Write-Output "missing $name"; continue }
  Write-Output "==== $name props ===="
  foreach ($p in $t.GetProperties([Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
    if ($p.Name -match 'Unit|View|Entity|Chat') {
      Write-Output "  $($p.PropertyType.Name) $($p.Name)"
    }
  }
}
# How Unit links to View
$core = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'LBoL.Core' } | Select-Object -First 1
$unit = $core.GetType('LBoL.Core.Units.Unit')
Write-Output 'Unit view-related:'
foreach ($p in $unit.GetProperties([Reflection.BindingFlags]'Public,NonPublic,Instance')) {
  if ($p.Name -match 'View') { Write-Output "  $($p.PropertyType.FullName) $($p.Name)" }
}
foreach ($m in $unit.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance')) {
  if ($m.Name -match 'View|Chat') { Write-Output "  method $($m.Name)" }
}
