$out = $args[0].Trim().TrimEnd('\').Trim('"').TrimEnd('\')
$bundle = Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins\AutoCADTools.bundle\Contents'
$pkg = Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins\AutoCADTools.bundle'
if (-not (Test-Path $bundle)) { New-Item -ItemType Directory -Path $bundle -Force | Out-Null }
if (-not (Test-Path $pkg)) { New-Item -ItemType Directory -Path $pkg -Force | Out-Null }
Copy-Item "$out\*.dll" $bundle -Force
Copy-Item "$out\*.pdb" $bundle -Force
Copy-Item "$out\PackageContents.xml" $bundle -Force
Copy-Item "$out\PackageContents.xml" $pkg -Force
Get-ChildItem $out -Directory | Where-Object { $_.Name -match '^[a-z]{2}(-[A-Z]{2})?$' } | ForEach-Object {
    $cultDest = Join-Path $bundle $_.Name
    if (-not (Test-Path $cultDest)) { New-Item -ItemType Directory -Path $cultDest -Force | Out-Null }
    Copy-Item "$($_.FullName)\*.dll" $cultDest -Force
}
