
function Add-TTEvent ($Context, $Mods, $Key, $ActionID, $PCName) {
    Write-Host "DEBUG: Call Ctx='$Context' Act='$ActionID'"
    
    if ($Context -match 'Panel') {
        'Library', 'Index' | ForEach-Object {
            $pName = $_
            $realContext = $Context -replace 'Panel', $pName
            $realActionID = $ActionID -replace '\[Panel\]', $pName
            Add-TTEvent $realContext $Mods $Key $realActionID $PCName
        }
        return
    }

    if ($Context -match 'Mode') {
        'Editor', 'Table' | ForEach-Object {
            $mName = $_
            $realContext = $Context -replace 'Mode', $mName
            $realActionID = $ActionID -replace '\[Mode\]', $mName
            Add-TTEvent $realContext $Mods $Key $realActionID $PCName
        }
        return
    }

    Write-Host "REGISTER: $Context -> $ActionID"
}

Write-Host "--- Testing Add-TTEvent expansion ---"
Add-TTEvent 'Panel-Mode-*-*' 'Alt' 'C' "[Panel].[Mode].Keyword:''" $Env:Computername
