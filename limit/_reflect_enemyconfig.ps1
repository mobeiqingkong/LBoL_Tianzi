$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.ConfigData.dll')
$t = $asm.GetType('LBoL.ConfigData.EnemyUnitConfig')
$t.GetProperties() | Where-Object { $_.Name -match 'Exhibit|Boss|Loot' } | ForEach-Object { "$($_.PropertyType.Name) $($_.Name)" }
