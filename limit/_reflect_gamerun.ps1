$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$asm.GetTypes() | Where-Object { $_.Name -match 'GameRun' } | Select-Object -First 20 -ExpandProperty FullName
