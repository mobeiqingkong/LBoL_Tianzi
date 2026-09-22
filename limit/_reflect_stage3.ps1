$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$t = $asm.GetType('LBoL.Core.Stage')
$t.GetProperties([Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly') |
  Where-Object { $_.Name -match 'GameRun|Run|Player' } |
  ForEach-Object { "$($_.PropertyType.Name) $($_.Name)" }
