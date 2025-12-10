
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
                try {
                    $state.Value = $state.Default.Invoke($StateID)
                }
                catch {
                    "ERROR invoking Default for $StateID : $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_init.txt" -Append
                    $state.Value = $null # Prevent crash
                }
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
    
    if ($Key.Contains(',')) {
        $Key.Split(',') | ForEach-Object { Add-TTEvent $Context $Mods $_.Trim() $ActionID $PCName }
        return
    }

    if ($ActionID -match '^(.+):(.+)$') {
        $stateID = $matches[1]
        $stateValue = $matches[2]
        
        if ($null -eq $global:Application.Actions.GetItem($ActionID)) {
            Add-TTAction $ActionID "Set $stateID to $stateValue" { Apply-TTState $stateID $stateValue $PCName; return $true }.GetNewClosure()
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
    $logFile = "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_init.txt"
    "Initializing TTStatus..." | Out-File $logFile -Encoding UTF8
    
    try {

        # 1. Apply Defaults
        "1. Apply Defaults..." | Out-File $logFile -Append
        if ($global:Application.Status.Items) {
            $global:Application.Status.Items | ForEach-Object {
                try {
                    Apply-TTState $_.ID 'Default' $Env:Computername
                }
                catch {
                    "Error applying default for $($_.ID): $_" | Out-File $logFile -Append
                }
            }
            "1. Apply Defaults FINISHED." | Out-File $logFile -Append
        }

        # 2. Load from Cache
        "2. Loading Cache..." | Out-File $logFile -Append
        $cacheFile = Join-Path $global:Application.MemoDir "cache\TTStatus.cache"
        if (Test-Path $cacheFile) {
            try {
                "  Reading: $cacheFile" | Out-File $logFile -Append
                # Assuming CSV format: ID,Value
                $csv = Import-Csv $cacheFile
                # Check if it has ID and Value properties
                if ($csv -and $csv[0].PSObject.Properties['ID'] -and $csv[0].PSObject.Properties['Value']) {
                    $csv | ForEach-Object {
                        # "  Restoring: $($_.ID)" | Out-File $logFile -Append
                        Apply-TTState $_.ID $_.Value $Env:Computername
                    }
                }
                else {
                    "TTStatus.cache format unrecognized." | Out-File $logFile -Append
                }
                "Loaded status from cache: $cacheFile" | Out-File $logFile -Append
            }
            catch {
                "Failed to load status cache: $_" | Out-File $logFile -Append
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
                            Invoke-Expression "Apply-TTState $argsStr"
                        }
                        catch {
                            "Error/Unimplemented in thinktank.md: $line ($_)" | Out-File $logFile -Append
                        }
                    }
                }
                "Loaded status from thinktank.md" | Out-File $logFile -Append
            }
            catch {
                "Failed to process thinktank.md: $_" | Out-File $logFile -Append
            }
        }


        # 4. Activate Watchers
        "4. Activating Watchers..." | Out-File $logFile -Append
        if ($global:Application.Status.Items) {
            "Activating Watchers starting..." | Out-File (Join-Path $global:Application.BaseDir "debug_init.txt") -Append -Encoding ASCII
            $items = $global:Application.Status.Items
                
            foreach ($state in $items) {
                if ($state.Watch -ne $null -and $state.Watch -is [ScriptBlock]) {
                    try {
                        "  Watching: $($state.ID)" | Out-File (Join-Path $global:Application.BaseDir "debug_init.txt") -Append -Encoding ASCII
                        $state.Watch.Invoke($state.ID)
                        "  Finished: $($state.ID)" | Out-File (Join-Path $global:Application.BaseDir "debug_init.txt") -Append -Encoding ASCII
                    }
                    catch {
                        "Failed to run Watch script for $($state.ID): $_" | Out-File (Join-Path $global:Application.BaseDir "debug_init.txt") -Append -Encoding ASCII
                    }
                }
            }
            "Activating Watchers finished." | Out-File (Join-Path $global:Application.BaseDir "debug_init.txt") -Append -Encoding ASCII
        }
    
        "TTStatus Initialization Complete." | Out-File $logFile -Append
    }
    catch {
        "CRASH in Initialize-TTStatus: $_" | Out-File $logFile -Append
        "Stack Trace: $($_.ScriptStackTrace)" | Out-File $logFile -Append
        throw
    }
}

Write-Host "TTStatus Initialization Complete."
