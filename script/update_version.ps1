$ErrorActionPreference = "Stop"

try {
    $date = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $pcName = $env:COMPUTERNAME
    
    # Get repository name (folder name)
    $repoName = Split-Path -Leaf (Get-Location).Path
    
    # Get last commit message
    $commitMsg = git log -1 --pretty=%B
    if ($commitMsg) {
        $commitMsg = $commitMsg.Trim()
    }
    else {
        $commitMsg = "No commit message"
    }

    $content = "Date: $date`r`nPC: $pcName`r`nRepo: $repoName`r`nComment: $commitMsg"
    
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $versionFile = Join-Path $scriptDir "version.txt"
    
    Set-Content -Path $versionFile -Value $content -Encoding UTF8
    Write-Host "Updated version.txt"
}
catch {
    Write-Host "Error updating version.txt: $_"
}
