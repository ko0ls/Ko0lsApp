param([string]$out)
$out = $out.TrimEnd('\').Trim('"')
$bundle = Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins\AutoCADTools.bundle\Contents'
$pkg    = Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins\AutoCADTools.bundle'
if (-not (Test-Path $bundle)) { New-Item -ItemType Directory -Path $bundle -Force | Out-Null }
if (-not (Test-Path $pkg))    { New-Item -ItemType Directory -Path $pkg    -Force | Out-Null }

# Primary assembly + PDB
Copy-Item "$out\*.dll"       $bundle -Force
Copy-Item "$out\*.pdb"       $bundle -Force
Copy-Item "$out\PackageContents.xml" $bundle -Force
Copy-Item "$out\PackageContents.xml" $pkg    -Force

# Localised satellite assemblies (en/, vi/, etc.)
Get-ChildItem $out -Directory | Where-Object { $_.Name -match '^[a-z]{2}(-[A-Z]{2})?$' } | ForEach-Object {
    $cultDest = Join-Path $bundle $_.Name
    if (-not (Test-Path $cultDest)) { New-Item -ItemType Directory -Path $cultDest -Force | Out-Null }
    Copy-Item "$($_.FullName)\*.dll" $cultDest -Force
}

# Resources (Icons, etc.) for ribbon button images
$resDest = Join-Path $bundle 'Resources'
if (-not (Test-Path $resDest)) { New-Item -ItemType Directory -Path $resDest -Force | Out-Null }
$srcRes = Join-Path $PSScriptRoot 'Resources'
if (Test-Path $srcRes) {
    Copy-Item "$srcRes\Icons" $resDest -Recurse -Force
}

# Copy Presentation-layer NuGet dependencies that are PrivateAssets=All and therefore
# not embedded in any DLL — they live in the user's NuGet cache and must be
# explicitly pulled into the bundle so XAML can resolve them at runtime.
$nugetRoot = Join-Path $env:USERPROFILE '.nuget\packages'
$extra = @(
    'microsoft.xaml.behaviors.wpf\1.1.122\lib\net462\Microsoft.Xaml.Behaviors.dll',
    'wpf.controls.panandzoom\2.0.0\lib\netcoreapp3\Wpf.Controls.PanAndZoom.dll',
    'wpf.controls.panandzoom\2.0.0\lib\netcoreapp3\Wpf.Controls.PanAndZoom.pdb'
)
foreach ($item in $extra) {
    $src = Join-Path $nugetRoot $item
    if (Test-Path $src) {
        Copy-Item $src $bundle -Force
    }
}
