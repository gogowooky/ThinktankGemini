$ScriptPath = $PSScriptRoot
$LibPath = Join-Path $ScriptPath "lib"
$SourcePath = Join-Path $ScriptPath "source"

Write-Host "Compiling files from $SourcePath..."
$SourceFiles = Get-ChildItem $SourcePath -Filter "*.cs" | Select-Object -ExpandProperty FullName
$SourceFiles | ForEach-Object { Write-Host " - $_" }

try {
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
    Write-Host "Compilation SUCCESS!"
}
catch {
    Write-Host "Compilation FAILED:"
    Write-Host $_
    
    # helper to see loader exceptions if any
    if ($_.Exception.LoaderExceptions) {
        $_.Exception.LoaderExceptions | ForEach-Object { Write-Host $_ }
    }
}
