$core = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Core.dll')
$base = [Reflection.Assembly]::LoadFrom('D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\LBoL.Base.dll')
foreach ($a in @($core, $base)) {
  foreach ($type in $a.GetExportedTypes()) {
    if ($type.Name -like '*RandomPool*') {
      Write-Output "TYPE $($type.FullName)"
      $type.GetMethods() | Where-Object { $_.IsPublic -and -not $_.IsSpecialName } | Select-Object -First 15 | ForEach-Object {
        Write-Output "  $($_.Name)"
      }
    }
  }
}
