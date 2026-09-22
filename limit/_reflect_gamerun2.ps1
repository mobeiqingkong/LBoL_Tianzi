$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$t = $asm.GetType('LBoL.Core.GameRun')
if ($t) {
  $t.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Instance,Static,DeclaredOnly') |
    Where-Object { $_.Name -match 'Start|New|Init|Create' } |
    ForEach-Object { $_.Name }
}
