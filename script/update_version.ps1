$ErrorActionPreference = "Stop"

try {
    $date = Get-Date -Format "yyMMdd.HHmm"
    $pcName = $env:COMPUTERNAME
    
    $content = "ver.$date on $pcName"
    
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $versionFile = Join-Path $scriptDir "version.txt"
    
    Set-Content -Path $versionFile -Value $content -Encoding UTF8
    Write-Host "Updated version.txt"
}
catch {
    Write-Host "Error updating version.txt: $_"
}
