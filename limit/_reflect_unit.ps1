$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$u = $asm.GetType('LBoL.Core.Units.Unit')
$u.GetProperties([Reflection.BindingFlags]'Public,Instance') | Where-Object { $_.Name -match 'Power' } | ForEach-Object { "$($_.PropertyType.Name) $($_.Name)" }
