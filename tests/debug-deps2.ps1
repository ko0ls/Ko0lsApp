$json = Get-Content "tests/AutoCADTools.Test/bin/Debug/net8.0/AutoCADTools.Test.deps.json" -Raw | ConvertFrom-Json
$target = $json.targets."net8.0"
$props = $target.PSObject.Properties
Write-Host "Total properties: $($props.Count)"
# Get first 5
$props | Select-Object -First 5 | ForEach-Object {
    Write-Host "Property: $($_.Name)"
}
