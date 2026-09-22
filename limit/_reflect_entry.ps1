$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$t = $asm.GetType('LBoL.Core.EnemyGroupEntry')
if ($t) { $t.FullName } else {
  [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.ConfigData.dll').GetType('LBoL.ConfigData.EnemyGroupEntry').FullName
}
