$path = 'C:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\script\DefaultStatus.ps1'
$content = Get-Content -Path $path
$newContent = $content | ForEach-Object {
    $line = $_
    $trimmed = $line.TrimStart()
    if ($trimmed.StartsWith("# ") -and -not $trimmed.StartsWith("#region") -and -not $trimmed.StartsWith("#endregion")) {
        $line -replace '^(\s*)# ', '$1'
    }
    else {
        $line
    }
}
$newContent | Set-Content -Path $path -Encoding UTF8
