Get-ChildItem 'D:\software\steam\steamapps\common\LBoL\LBoL_Data\Managed\*.dll' | ForEach-Object {
  try { [void][Reflection.Assembly]::LoadFrom($_.FullName) } catch {}
}
$pres = [AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.GetName().Name -eq 'LBoL.Presentation' } | Select-Object -First 1
$types = @()
try { $types = $pres.GetTypes() } catch [System.Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ -ne $null } }
foreach ($t in $types) {
  try {
    foreach ($m in $t.GetMethods([Reflection.BindingFlags]'Public,NonPublic,Static,Instance,DeclaredOnly')) {
      if ($m.Name -match '^(InternalChat|DebutChat|ShowChat|Chat)$') {
        $ps = ($m.GetParameters() | ForEach-Object { $_.ParameterType.FullName }) -join ', '
        Write-Output "$($t.FullName).$($m.Name)($ps) -> $($m.ReturnType.Name)"
      }
    }
  } catch {}
}
