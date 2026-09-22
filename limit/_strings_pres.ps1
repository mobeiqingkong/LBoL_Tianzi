$root = 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\'
$bytes = [IO.File]::ReadAllBytes($root + 'LBoL.Presentation.dll')
$text = [Text.Encoding]::ASCII.GetString($bytes)
$matches = [regex]::Matches($text, '[A-Za-z][A-Za-z0-9_]{5,80}')
$matches | ForEach-Object { $_.Value } | Where-Object { $_ -match 'Dialog|Chat|Speech|Talk|Bubble|Debut|Narrative' } | Sort-Object -Unique | Select-Object -First 80
