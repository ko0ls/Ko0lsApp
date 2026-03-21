$json = Get-Content "tests/AutoCADTools.Test/bin/Debug/net8.0/AutoCADTools.Test.deps.json" -Raw | ConvertFrom-Json
$target = $json.targets."net8.0"
$props = $target.PSObject.Properties
$execProps = $props | Where-Object { $_.Name -like "*execution*" }
foreach ($p in $execProps) {
    Write-Host "Property: $($p.Name)"
    $val = $p.Value
    foreach ($sub in $val.PSObject.Properties) {
        Write-Host "  $($sub.Name): $($sub.Value)"
    }
}
