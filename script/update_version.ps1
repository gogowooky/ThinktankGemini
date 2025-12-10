$ErrorActionPreference = 'Stop'

# パス設定
$ScriptPath = $PSScriptRoot
$RootPath = Split-Path $ScriptPath -Parent
$VersionFile = Join-Path $ScriptPath "version.txt"

# 情報取得
$ProjectName = Split-Path $RootPath -Leaf
$Timestamp = Get-Date -Format "yyMMdd.HHmm"
$PCName = $env:COMPUTERNAME

# 最新のコミットメッセージ取得
# git log -1 --pretty=%B でメッセージ取得、トリムして改行を削除
try {
    $CommitMsg = git -C $RootPath log -1 --pretty=%B 2>$null
    if ($CommitMsg -is [array]) {
        $CommitMsg = $CommitMsg -join " "
    }
    if ([string]::IsNullOrWhiteSpace($CommitMsg)) {
        $CommitMsg = "No commit message"
    }
    $CommitMsg = $CommitMsg.Trim()
}
catch {
    $CommitMsg = "No commit message found"
}

# フォーマット: ver.Timestamp,ProjectName on PCName CommitMsg
$VersionString = "{0} ver.{1} on {2} {3}" -f $ProjectName, $Timestamp, $PCName, $CommitMsg

# ファイル書き込み
Set-Content -Path $VersionFile -Value $VersionString -Encoding UTF8 -NoNewline

Write-Host "Version updated: $VersionString"
