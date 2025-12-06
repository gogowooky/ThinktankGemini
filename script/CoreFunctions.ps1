
function Add-TTAction ($ActionID, $Description, [ScriptBlock]$Script) {
    $action = [ThinktankApp.TTAction]::new()
    $action.ID = $ActionID
    $action.Name = $Description
    $action.Script = $Script
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
