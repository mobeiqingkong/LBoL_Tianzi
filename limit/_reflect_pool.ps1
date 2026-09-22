$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$t = $asm.GetType('LBoL.Core.RepeatableRandomPool`1')
if (-not $t) { $asm.GetTypes() | Where-Object { $_.Name -match 'RepeatableRandomPool' } | ForEach-Object { $_.FullName } }
else {
  $t.GetMethods([Reflection.BindingFlags]'Public,Instance,DeclaredOnly') | ForEach-Object {
    $params = ($_.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ', '
    "$($_.Name)($params)"
  }
}
