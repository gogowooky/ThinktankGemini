$global:ScriptPath = "c:\Users\gogow\Documents\ThinktankGemini\script"

$sb = {
    $versionFile = "$global:ScriptPath\version.txt"
    
    if (Test-Path $versionFile) {
        return (Get-Content -Path $versionFile -Raw).Trim()
    }
    
    $timestamp = Get-Date
    "ver.$($timestamp.tostring('yyMMdd- HHmm')) unknown on $($Env:Computername)"
}

$result = & $sb
Write-Host "Version: $result"

if ($result -match '^ver\.\d{6}\.\d{4} on .+$') {
    Write-Host "Verification PASSED"
}
else {
    Write-Host "Verification FAILED. Result: $result"
}
