
Write-Host "Loading CoreFunctions..."
try {
    . "$PSScriptRoot\script\CoreFunctions.ps1"
    Write-Host "Loaded successfully."
}
catch {
    Write-Host "Error loading:Str $_"
    exit 1
}
