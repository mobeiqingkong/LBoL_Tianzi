$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$t = $asm.GetType('LBoL.Core.GameRunController')
if (-not $t) { Write-Output 'missing'; exit }
$t.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly') |
  Where-Object { $_.Name -match 'Start|Begin|New|Reset|Init' } |
  ForEach-Object {
    $params = ($_.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ','
    "$($_.ReturnType.Name) $($_.Name)($params)"
  }
