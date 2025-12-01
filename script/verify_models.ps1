# Verification Script: TTState and TTStatus

try {
    # Load assemblies
    $sourceFiles = @(
        "c:\Users\gogow\Documents\AIThinktank\AIThinktank\ThinktankApp\source\TTObject.cs",
        "c:\Users\gogow\Documents\AIThinktank\AIThinktank\ThinktankApp\source\TTCollection.cs",
        "c:\Users\gogow\Documents\AIThinktank\AIThinktank\ThinktankApp\source\TTState.cs",
        "c:\Users\gogow\Documents\AIThinktank\AIThinktank\ThinktankApp\source\TTStatus.cs"
    )

    Add-Type -TypeDefinition (Get-Content $sourceFiles | Out-String) -ReferencedAssemblies "System.ObjectModel", "System.ComponentModel", "System.Runtime"

    Write-Host "Class compilation and load success." -ForegroundColor Green

    # Instantiate TTStatus
    $status = New-Object ThinktankApp.TTStatus
    Write-Host "TTStatus Instance: OK"

    # Instantiate TTState and Add
    $state1 = New-Object ThinktankApp.TTState
    $state1.ID = "App.Test.State1"
    $state1.Value = "InitialValue"
    $status.AddItem($state1)
    Write-Host "TTState Add (App.Test.State1): OK"

    # Verify GetItem
    $retrievedState = $status.GetItem("App.Test.State1")
    if ($retrievedState.Value -eq "InitialValue") {
        Write-Host "Get Value (InitialValue): OK" -ForegroundColor Green
    }
    else {
        Write-Host "Get Value Failed: Expected 'InitialValue', Actual $($retrievedState.Value)" -ForegroundColor Red
    }

    # Verify SetValue
    $status.SetValue("App.Test.State1", "NewValue")
    if ($state1.Value -eq "NewValue") {
        Write-Host "SetValue (NewValue): OK" -ForegroundColor Green
    }
    else {
        Write-Host "SetValue Failed: Expected 'NewValue', Actual $($state1.Value)" -ForegroundColor Red
    }

}
catch {
    Write-Host "Error occurred: $_" -ForegroundColor Red
    $_.Exception | Format-List * -Force
}
