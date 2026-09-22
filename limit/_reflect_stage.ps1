$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$t = $asm.GetType('LBoL.Core.Stage')
$t.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly') |
  Where-Object { $_.Name -match 'Boss' } |
  ForEach-Object {
    $params = ($_.GetParameters() | ForEach-Object { "$($_.ParameterType.Name) $($_.Name)" }) -join ', '
    "$($_.ReturnType.Name) $($_.Name)($params)"
  }
