Get-ChildItem 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\*.dll' | ForEach-Object {
  try { [void][Reflection.Assembly]::LoadFrom($_.FullName) } catch {}
}
$pres = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'LBoL.Presentation' } | Select-Object -First 1
$types = @()
try { $types = $pres.GetTypes() } catch [System.Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ -ne $null } }
$ct = $types | Where-Object { $_.Name -eq 'CloudType' -or $_.FullName -match 'ChatWidget\+CloudType' } | Select-Object -First 1
Write-Output $ct.FullName
[Enum]::GetNames($ct) -join ', '
