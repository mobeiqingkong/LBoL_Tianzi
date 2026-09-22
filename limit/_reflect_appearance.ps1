$asm = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Base.dll')
$e = [Enum]::GetNames($asm.GetType('LBoL.ConfigData.AppearanceType'))
$e -join ', '
