$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$t = $asm.GetType('LBoL.Core.Stage')
$t.GetProperties([Reflection.BindingFlags]'Public,NonPublic,Instance,DeclaredOnly') |
  ForEach-Object { "$($_.PropertyType.Name) $($_.Name)" }
