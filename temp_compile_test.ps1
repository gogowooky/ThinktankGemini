Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase
Add-Type -AssemblyName System.Xaml
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$presentationFrameworkPath = [System.Reflection.Assembly]::Load("PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35").Location
$presentationCorePath = [System.Reflection.Assembly]::Load("PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35").Location
$windowsBasePath = [System.Reflection.Assembly]::Load("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35").Location
$systemXamlPath = [System.Reflection.Assembly]::Load("System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Location
$systemWindowsFormsPath = [System.Reflection.Assembly]::Load("System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Location
$systemDrawingPath = [System.Reflection.Assembly]::Load("System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location
$psPath = [PSObject].Assembly.Location

$sourceFiles = Get-ChildItem "source\*.cs"
$assemblies = @(
    "System.dll",
    "System.Core.dll",
    "System.Data.dll",
    "System.Xml.dll",
    "System.Xml.Linq.dll",
    $presentationFrameworkPath,
    $presentationCorePath,
    $windowsBasePath,
    $systemXamlPath,
    $systemWindowsFormsPath,
    $systemDrawingPath,
    $psPath
)

# Add AvalonEdit and WebView2 if they exist in lib
if (Test-Path "lib\ICSharpCode.AvalonEdit.dll") {
    $assemblies += (Resolve-Path "lib\ICSharpCode.AvalonEdit.dll").Path
}
if (Test-Path "lib\Microsoft.Web.WebView2.Wpf.dll") {
    $assemblies += (Resolve-Path "lib\Microsoft.Web.WebView2.Wpf.dll").Path
}
if (Test-Path "lib\Microsoft.Web.WebView2.Core.dll") {
    $assemblies += (Resolve-Path "lib\Microsoft.Web.WebView2.Core.dll").Path
}

$options = New-Object System.CodeDom.Compiler.CompilerParameters
$options.ReferencedAssemblies.AddRange($assemblies)
$options.GenerateExecutable = $false
$options.GenerateInMemory = $true

$provider = New-Object Microsoft.CSharp.CSharpCodeProvider
$results = $provider.CompileAssemblyFromFile($options, [string[]]$sourceFiles.FullName)

if ($results.Errors.HasErrors) {
    $results.Errors | ForEach-Object { Write-Host $_.ToString() }
    exit 1
}
else {
    Write-Host "Compilation successful."
    exit 0
}
