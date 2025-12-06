$global:ScriptPath = "c:\Users\gogow\Documents\ThinktankGemini\script"

$sb = {
    $versionFile = "$global:ScriptPath\version.txt"
    
    if (Test-Path $versionFile) {
        $content = Get-Content -Path $versionFile -Raw
        if ($content -match 'LastComment:\s*(.+)') {
            return $matches[1].Trim()
        }
    }
    
    $timestamp = Get-Date
    "ver.$($timestamp.tostring('yyMMdd- HHmm')) unknown on $($Env:Computername)"
}

$result = & $sb
Write-Host "Version: $result"

if ($result -eq "251206 1113 E15") {
    Write-Host "Verification PASSED"
}
else {
    Write-Host "Verification FAILED"
}
