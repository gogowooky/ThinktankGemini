# start.ps1

$ScriptPath = $PSScriptRoot
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$LibPath = Join-Path $ScriptPath "lib"
$SourcePath = Join-Path $ScriptPath "source"
$StylePath = Join-Path $ScriptPath "styles"
$AppPath = $ScriptPath
$AppScriptPath = Join-Path $AppPath "script"

# Load Assemblies
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
    exit
}

# Compile C#
if (-not ("ThinktankApp.TTApplication" -as [type])) {
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
    }
    catch {
        Write-Error "Failed to compile C# code: $_"
        exit
    }
}

# Run Application
$XamlPath = Join-Path $StylePath "TTApplication.xaml"
$PanelXamlPath = Join-Path $StylePath "TTPanel.xaml"
$StyleXamlPath = Join-Path $StylePath "Style.xaml"

try {
    $App = [ThinktankApp.TTApplication]::new($XamlPath, $StyleXamlPath, $PanelXamlPath, $AppScriptPath)
    $App.SetTitle("Thinktank Recreated")
    $App.Show()
}
catch {
    Write-Error "Runtime error: $_"
    Read-Host "Press Enter to exit..."
}
