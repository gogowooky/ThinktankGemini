
function Test-Param ($P) {
    Write-Host "Param received: '$P'"
}

Write-Host "--- Test: arg:'' ---"
Test-Param arg:''

Write-Host "--- Test: 'arg:''' ---"
Test-Param 'arg:'''

Write-Host "--- Test: `"arg:''`" ---"
Test-Param "arg:''"
