Add-Type -Path 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\UnityEngine.CoreModule.dll' -ErrorAction SilentlyContinue
$presPath = 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Presentation.dll'
# Load all managed deps first
Get-ChildItem 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\*.dll' | ForEach-Object {
  try { [void][Reflection.Assembly]::LoadFrom($_.FullName) } catch {}
}
$pres = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'LBoL.Presentation' } | Select-Object -First 1
Write-Output ("pres=" + $pres)
foreach ($t in $pres.GetTypes()) {
  foreach ($m in $t.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Static,Instance,DeclaredOnly')) {
    if ($m.Name -match 'Chat|Talk|Debut') {
      $ps = ($m.GetParameters() | ForEach-Object { $_.ParameterType.Name }) -join ','
      Write-Output "$($t.Name).$($m.Name)($ps)"
    }
  }
}
