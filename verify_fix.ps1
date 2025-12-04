# verify_fix.ps1
$ScriptPath = $PSScriptRoot
$LibPath = Join-Path $ScriptPath "lib"
$SourcePath = Join-Path $ScriptPath "source"

try {
    Add-Type -Path (Join-Path $LibPath "ICSharpCode.AvalonEdit.dll")
    Add-Type -Path (Join-Path $LibPath "Microsoft.Web.WebView2.Wpf.dll")
    Add-Type -Path (Join-Path $LibPath "Microsoft.Web.WebView2.Core.dll")
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName PresentationFramework
    Add-Type -AssemblyName PresentationCore
    Add-Type -AssemblyName WindowsBase
    Add-Type -AssemblyName System.Xaml
}
catch {
    Write-Error "Failed to load assemblies: $_"
    exit 1
}

try {
    $SourceFiles = Get-ChildItem $SourcePath -Filter "*.cs" | Select-Object -ExpandProperty FullName
    $SMA = [PSObject].Assembly.Location
    Add-Type -Path $SourceFiles -ReferencedAssemblies @(
        (Join-Path $LibPath "ICSharpCode.AvalonEdit.dll"),
        (Join-Path $LibPath "Microsoft.Web.WebView2.Wpf.dll"),
        (Join-Path $LibPath "Microsoft.Web.WebView2.Core.dll"),
        "System.Windows.Forms",
        "PresentationFramework",
        "PresentationCore",
        "WindowsBase",
        "System.Xaml",
        "System.Xml",
        "System.Drawing",
        $SMA
    )
    Write-Host "Compilation Successful"
}
catch {
    Write-Error "Failed to compile C# code: $_"
    exit 1
}

try {
    # Verify TTExModMode Class is GONE
    if ("TTExModMode" -as [type]) {
        Write-Error "TTExModMode class STILL EXISTS. It should have been removed."
    }
    else {
        Write-Host "TTExModMode class successfully removed."
    }

    # Verify GetFdPanel method exists in TTApplication
    $appType = [ThinktankApp.TTApplication]
    if ($appType) {
        $method = $appType.GetMethod("GetFdPanel")
        if ($method) {
            Write-Host "GetFdPanel method exists in TTApplication."
        }
        else {
            Write-Error "GetFdPanel method NOT found in TTApplication."
        }
    }
    else {
        Write-Error "ThinktankApp.TTApplication type not found."
    }
}
catch {
    Write-Error "Failed to verify changes: $_"
    exit 1
}
