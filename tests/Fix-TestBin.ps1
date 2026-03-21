$ErrorActionPreference = "SilentlyContinue"
$dest = "tests/AutoCADTools.Test/bin/Debug/net8.0"
$pkgs = "C:/Users/Ko0ls/.nuget/packages"
$depsPath = "$dest/AutoCADTools.Test.deps.json"

Write-Host "Reading deps.json..."
$json = Get-Content $depsPath -Raw
$data = $json | ConvertFrom-Json

$targets = $data.targets.PSObject.Properties | Where-Object { $_.Name -match "^net8\.0|^netstandard" }
$libMap = @{}

foreach ($target in $targets) {
    $libs = $target.Value.PSObject.Properties
    foreach ($lib in $libs) {
        if ($lib.Name -match "^(.+)/([0-9.]+)$") {
            $pkg = $Matches[1]
            $ver = $Matches[2]
            $pathVal = $lib.Value.path
            if (-not $pathVal) { continue }

            # Normalize package name for path lookup
            $pkgDir = $pkg -replace "\.", "_"
            $pkgSearch = $pkg -replace "\.", "."

            # Find the package in NuGet cache
            $pkgSrc = Get-ChildItem -Path $pkgs -Directory | Where-Object { $_.Name -eq $pkgSearch }
            if (-not $pkgSrc) {
                # Try without dots
                $pkgSrc = Get-ChildItem -Path $pkgs -Directory | Where-Object { $_.Name -eq $pkg }
            }
            if (-not $pkgSrc) { continue }

            $pkgVerDir = Get-ChildItem -Path $pkgSrc.FullName -Directory | Sort-Object Name -Descending | Select-Object -First 1
            if (-not $pkgVerDir) { continue }

            # Extract the filename from the path
            $filename = Split-Path $pathVal -Leaf
            $destSubDir = Split-Path $pathVal -Parent

            # Search for the DLL in all lib folders
            $foundDll = $null
            $destDllPath = $null

            $libDirs = Get-ChildItem -Path $pkgVerDir.FullName/lib -Directory
            foreach ($dir in $libDirs) {
                $dllPath = Join-Path $dir.FullName $filename
                if (Test-Path $dllPath) {
                    # Prefer netstandard2.1 > net8.0 > netstandard2.0 > netstandard1.1
                    $foundDll = $dllPath
                    if ($dir.Name -eq "netstandard2.1") { break }
                    if ($dir.Name -eq "net8.0") { break }
                    if ($dir.Name -eq "netstandard2.0") { break }
                }
            }

            # Also check runtimes
            if (-not $foundDll) {
                $runtimes = Get-ChildItem -Path $pkgVerDir.FullName/runtimes -Directory
                foreach ($rt in $runtimes) {
                    $rtLibs = Get-ChildItem -Path $rt.FullName/lib -Directory
                    foreach ($dir in $rtLibs) {
                        $dllPath = Join-Path $dir.FullName $filename
                        if (Test-Path $dllPath) {
                            $foundDll = $dllPath
                            break
                        }
                    }
                    if ($foundDll) { break }
                }
            }

            if ($foundDll) {
                # Create destination dir
                $destDirPath = Join-Path $dest $destSubDir
                if ($destDirPath -ne $dest) {
                    if (-not (Test-Path $destDirPath)) {
                        New-Item -ItemType Directory -Path $destDirPath -Force | Out-Null
                    }
                }
                # Copy DLL
                Copy-Item $foundDll (Join-Path $dest $pathVal) -Force
                $libMap[$lib.Name] = $pathVal
                Write-Host "  [OK] $($lib.Name) -> $pathVal"
            } else {
                Write-Host "  [MISS] $($lib.Name) (pkg: $pkgSearch/$($pkgVerDir.Name))"
            }
        }
    }
}

Write-Host ""
Write-Host "Done. Copied $($libMap.Count) assemblies."
