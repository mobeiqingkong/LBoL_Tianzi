$root = 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\'
foreach ($dll in @('LBoL.Presentation.dll','LBoL.Core.dll','LBoL.EntityLib.dll','LBoL.ConfigData.dll')) {
  $bytes = [IO.File]::ReadAllBytes($root + $dll)
  $text = [Text.Encoding]::ASCII.GetString($bytes)
  $matches = [regex]::Matches($text, '[A-Za-z][A-Za-z0-9_]{4,80}')
  $hits = $matches | ForEach-Object { $_.Value } | Where-Object { $_ -match 'DebutChat|InternalChat|ShowChat|UnitChat|ChatWidget|PreBattle' } | Sort-Object -Unique
  if ($hits) {
    Write-Output "==== $dll ===="
    $hits | ForEach-Object { Write-Output $_ }
  }
}
