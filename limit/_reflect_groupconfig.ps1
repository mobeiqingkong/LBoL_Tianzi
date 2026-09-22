$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.ConfigData.dll')
$t = $asm.GetType('LBoL.ConfigData.EnemyGroupConfig')
$t.GetProperties() | ForEach-Object { "$($_.PropertyType.Name) $($_.Name)" }
