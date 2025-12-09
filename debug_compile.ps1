$ScriptPath = $PSScriptRoot
$LibPath = Join-Path $ScriptPath "lib"
$SourcePath = Join-Path $ScriptPath "source"

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
    Write-Error "Compilation Failed: $_"
    $_.Exception.LoaderExceptions | ForEach-Object { Write-Error $_ }
}
