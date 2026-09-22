Get-ChildItem 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\*.dll' | ForEach-Object {
  try { [void][Reflection.Assembly]::LoadFrom($_.FullName) } catch {}
}
$pres = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'LBoL.Presentation' } | Select-Object -First 1
$types = @()
try { $types = $pres.GetTypes() } catch [System.Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ -ne $null } }
$t = $types | Where-Object { $_.FullName -eq 'LBoL.Presentation.Units.GameDirector' }
Write-Output "GameDirector methods with Chat/Debut/Battle:"
foreach ($m in $t.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Static,Instance,DeclaredOnly')) {
  if ($m.Name -match 'Chat|Debut|Battle|Speak|Dialog') {
    $ps = ($m.GetParameters() | ForEach-Object { $_.ParameterType.Name + ' ' + $_.Name }) -join ', '
    Write-Output "  $($m.Name)($ps)"
  }
}
# UnitView.Chat and how to get view from EnemyUnit
$uv = $types | Where-Object { $_.FullName -eq 'LBoL.Presentation.Units.UnitView' }
Write-Output "UnitView Chat overloads:"
foreach ($m in $uv.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly')) {
  if ($m.Name -eq 'Chat') {
    $ps = ($m.GetParameters() | ForEach-Object { $_.ParameterType.Name + ' ' + $_.Name }) -join ', '
    Write-Output "  Chat($ps)"
  }
}
