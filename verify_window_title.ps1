$states = @{}
function New-TTState ($id, $name, $props) {
    $states[$id] = $props
}

function Get-TTState ($id) {
    if ($id -eq 'Application.Product.Name') { return 'Thinktank' }
    return $null
}

$global:appliedStates = @{}
function Apply-TTState ($id, $val) {
    $global:appliedStates[$id] = $val
    Write-Host "Apply-TTState called with ID: $id, Val: $val"
}

$global:Models = [PSCustomObject]@{
    Status = [PSCustomObject]@{
        SetValue = { param($id, $val) Write-Host "SetValue called for $id = $val" }
    }
}

# Mock TTApplication for other states in DefaultStatus.ps1
$global:Application = [PSCustomObject]@{
    Window            = [PSCustomObject]@{
        Width               = 1000
        Height              = 800
        Left                = 0
        Top                 = 0
        FontSize            = 12
        Add_StateChanged    = { }
        Add_SizeChanged     = { }
        Add_LocationChanged = { }
    }
    Menu              = [PSCustomObject]@{
        FontSize             = 12
        Add_IsVisibleChanged = { }
    }
    SetTitle          = { }
    ExMode            = ''
    SetBorderPosition = { }
    GetBorderPosition = { }
    GetFdPanel        = { return [PSCustomObject]@{ Name = 'Desk' } }
    LibraryIndexGrid  = [PSCustomObject]@{ Add_SizeChanged = { } }
    ShelfDeskGrid     = [PSCustomObject]@{ Add_SizeChanged = { } }
    LibraryGrid       = [PSCustomObject]@{ Add_SizeChanged = { } }
    IndexGrid         = [PSCustomObject]@{ Add_SizeChanged = { } }
    ShelfGrid         = [PSCustomObject]@{ Add_SizeChanged = { } }
    DeskGrid          = [PSCustomObject]@{ Add_SizeChanged = { } }
    UserGrid          = [PSCustomObject]@{ Add_SizeChanged = { } }
    SystemGrid        = [PSCustomObject]@{ Add_SizeChanged = { } }
    PanelMap          = @{}
}

# Load the script to register states
. .\script\DefaultStatus.ps1

# Test
$versionState = $states['Application.Product.Version']
if ($versionState -eq $null) {
    Write-Host "Error: Application.Product.Version state not found."
    exit 1
}

$applyBlock = $versionState.Apply
& $applyBlock 'Application.Product.Version' 'ver.1.0.0'

if ($global:appliedStates['Application.Window.Title'] -eq 'Thinktank ver.1.0.0') {
    Write-Host "Verification PASSED"
}
else {
    Write-Host "Verification FAILED. Actual: $($global:appliedStates['Application.Window.Title'])"
}
