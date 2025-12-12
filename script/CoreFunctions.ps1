
function Add-TTAction ($ActionID, $Description, [ScriptBlock]$Script, $IsVisible = $true) {
    $action = [ThinktankApp.TTAction]::new()
    $action.ID = $ActionID
    $action.Name = $Description
    $action.Script = $Script
    $action.IsVisible = $IsVisible
    $global:Application.Actions.AddItem($action)
}

function New-TTState ($StateID, $Description, $Scripts) {
    if ($StateID -match '^\[Panels\]') {
        'Library', 'Index', 'Shelf', 'Desk', 'System' | ForEach-Object {
            $realID = $StateID -replace '\[Panels\]', $_
            $realDesc = $Description -replace '\[Panels\]', $_
            New-TTState $realID $realDesc $Scripts
        }
        return
    }

    $state = [ThinktankApp.TTState]::new()
    $state.ID = $StateID
    $state.Name = $Description

    if ($Scripts -is [string]) {
        $state.Value = $Scripts
        $state.Default = $Scripts
    }
    else {
        $state.Default = $Scripts['Default']
        $state.Test = $Scripts['Test']
        $state.Apply = $Scripts['Apply']
        $state.Watch = $Scripts['Watch']
        
        if ($state.Default -ne $null) {
            if ($state.Default -is [ScriptBlock]) {
                $state.Value = $state.Default.Invoke($StateID)
            }
            else {
                $state.Value = $state.Default
            }
        }
    }
    
    $global:Application.Status.AddItem($state)
}

function Apply-TTState ($ID, $Value, $PCName) {
    if ( $PCName -notin @( $null, $Env:Computername, '*' ) ) { return }
    
    $state = $global:Application.Status.GetItem($ID)
    if ($state -eq $null) { return }

    $val = $Value
    if ($Value -eq 'Default') {
        if ($state.Default -is [ScriptBlock]) {
            $val = $state.Default.Invoke($ID)
        }
        else {
            $val = $state.Default
        }
    }

    # Test logic could be added here
    
    if ($state.Apply -ne $null -and $state.Apply -is [ScriptBlock]) {
        $state.Apply.Invoke($ID, $val)
    }
    else {
        $global:Application.Status.SetValue($ID, $val)
    }
}

function Get-TTState ($ID) {
    $state = $global:Application.Status.GetItem($ID)
    if ($state -ne $null) {
        return $state.Value
    }
    return $null
}

function Add-TTEvent ($Context, $Mods, $Key, $ActionID, $PCName) {
    if ( $PCName -notin @( $null, $Env:Computername, '*' ) ) { return }
    
    if ($Context -match 'ExPanel') {
        'Library', 'Index', 'Shelf', 'Desk', 'System' | ForEach-Object {
            $pName = $_
            $exName = "Ex$pName"
            
            $realContext = $Context -replace 'ExPanel', $exName
            $realActionID = $ActionID -replace '\[ExPanel\]', $pName
            
            Add-TTEvent $realContext $Mods $Key $realActionID $PCName
        }
        return
    }

    if ($Context -match 'Panel') {
        'Library', 'Index', 'Shelf', 'Desk', 'System' | ForEach-Object {
            $pName = $_
            
            $realContext = $Context -replace 'Panel', $pName
            $realActionID = $ActionID -replace '\[Panel\]', $pName
            
            Add-TTEvent $realContext $Mods $Key $realActionID $PCName
        }
        return
    }

    if ($Key.Contains(',')) {
        $Key.Split(',') | ForEach-Object { Add-TTEvent $Context $Mods $_.Trim() $ActionID $PCName }
        return
    }

    if ($ActionID -match '^(.+):(.+)$') {
        $stateID = $matches[1]
        $stateValue = $matches[2]
        
        if ($null -eq $global:Application.Actions.GetItem($ActionID)) {
            Add-TTAction $ActionID "Set $stateID to $stateValue" { Apply-TTState $stateID $stateValue $PCName }.GetNewClosure()
            $action = $global:Application.Actions.GetItem($ActionID)
            if ($action) { $action.IsHidden = $true }
        }
    }

    $evnt = [ThinktankApp.TTEvent]::new()
    $evnt.Name = $ActionID
    $evnt.Context = $Context
    $evnt.Mods = $Mods
    $evnt.Key = $Key
    $evnt.ID = "$Context|$Mods|$Key"
    
    $global:Application.Models.Events.AddItem($evnt)
}

function Initialize-TTStatus {
    Write-Host "Initializing TTStatus..."

    # 0. Apply Watch
    if ($global:Application.Status.Items) {
        $global:Application.Status.Items | ForEach-Object {
            if ($_.Watch -is [ScriptBlock]) {
                $_.Watch.Invoke($_.ID)
                return
            }
        }
    }

    # 1. Apply Defaults
    if ($global:Application.Status.Items) {
        $global:Application.Status.Items | ForEach-Object {
            $id = $_.ID
            Apply-TTState $id 'Default'
        }
    }

    # 2. Load from Cache
    $cacheFile = Join-Path $global:Application.MemoDir "cache\TTStatus.cache"
    if (Test-Path $cacheFile) {
        try {
            # Assuming CSV format: ID,Value (No header in file usually means we need to supply Header or use Property names if it has header)
            # Standard Import-Csv assumes headers if not supplied. 
            # If cache file has no headers, we should supply them. 
            # Let's assume the cache file is generated by Export-Csv which includes headers.
            # But the requirement says "read from .cache". I should be robust.
            
            $csv = Import-Csv $cacheFile
            # Check if it has ID and Value properties
            if ($csv -and $csv[0].PSObject.Properties['ID'] -and $csv[0].PSObject.Properties['Value']) {
                $csv | ForEach-Object {
                    Apply-TTState $_.ID $_.Value $Env:Computername
                }
            }
            else {
                # Try reading without headers implementation if needed, but let's stick to standard CSV for now.
                Write-Warning "TTStatus.cache format unrecognized. Expecting CSV with ID and Value columns."
            }
            Write-Host "Loaded status from cache: $cacheFile"
        }
        catch {
            Write-Error "Failed to load status cache: $_"
        }
    }

    # 3. Load from thinktank.md
    $mdFile = Join-Path $global:Application.BaseDir "thinktank.md"
    if (Test-Path $mdFile) {
        try {
            $content = Get-Content $mdFile -Encoding UTF8
            $content | ForEach-Object {
                $line = $_.Trim()
                if ($line -match '^Apply-TTState\s+(.+)$') {
                    $argsStr = $matches[1]
                    try {
                        # Invoke-Expression is used to parse arguments like 'Param' "Value" correctly
                        Invoke-Expression "Apply-TTState $argsStr"
                    }
                    catch {
                        # Report unimplemented items or errors
                        Write-Host "Error/Unimplemented in thinktank.md: $line ($_) " -ForegroundColor Yellow
                    }
                }
            }
            Write-Host "Loaded status from thinktank.md"
        }
        catch {
            Write-Error "Failed to process thinktank.md: $_"
        }
    }

    Write-Host "TTStatus Initialization Complete."
}
