
function Test-Args ($A, $B, $C, $D) {
    Write-Host "A: $A"
    Write-Host "B: $B"
    Write-Host "C: $C"
    Write-Host "D: $D"
}

Write-Host "--- Test 1: Quoted ---"
Test-Args 'One' 'Two' 'Three' '[Panel].[Mode].Keyword:'''

Write-Host "`n--- Test 2: Unquoted ---"
# This mirrors the user's file
try {
    Invoke-Expression "Test-Args 'One' 'Two' 'Three' [Panel].[Mode].Keyword:''" 
}
catch {
    Write-Host "Error in Test 2: $_"
}

Write-Host "`n--- Test 3: Unquoted direct call ---"
# If I write it directly here, and it's a syntax error, this script won't parse.
# So I'll put it in a separate file or use Invoke-Expression.
