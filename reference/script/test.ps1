# System.Threading.Tasks名前空間を読み込む
Add-Type -AssemblyName System.Threading.Tasks

# 並列処理する関数
function ParallelFunction {
    param([int]$number)
    Write-Host "Starting processing for $number"
    Start-Sleep -Seconds 3  # 仮の処理として3秒待機
    Write-Host "Completed processing for $number"
}

# 並列処理するデータのリスト
$numbers = 1..5

# 各処理を並列で実行する
$tasks = @()
foreach ($number in $numbers) {
    $task = [System.Threading.Tasks.Task]::Factory.StartNew({
        ParallelFunction $number
    })
    $tasks += $task
}

# すべてのタスクが完了するのを待機する
[System.Threading.Tasks.Task]::WaitAll($tasks)

Write-Host "All tasks completed"

