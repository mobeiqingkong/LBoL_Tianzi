$root = 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\'
$bytes = [IO.File]::ReadAllBytes($root + 'LBoL.Presentation.dll')
$text = [Text.Encoding]::Unicode.GetString($bytes)
# also try UTF8/ASCII mix - find near DebutChat
$idx = $text.IndexOf('DebutChat')
Write-Output "unicode idx $idx"
$ascii = [Text.Encoding]::ASCII.GetString($bytes)
$i = 0
while (($i = $ascii.IndexOf('DebutChat', $i)) -ge 0 -and $i -lt $ascii.Length) {
  $start = [Math]::Max(0, $i - 80)
  $chunk = $ascii.Substring($start, [Math]::Min(200, $ascii.Length - $start)) -replace '[^\x20-\x7E]', '.'
  Write-Output $chunk
  Write-Output '---'
  $i++
  if ($i -gt 20) { break }
}
