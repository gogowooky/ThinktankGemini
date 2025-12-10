

#region ::: アプリ

Add-TTAction    Application.Run.Break       'デバッグ画面で改行' {
    Write-Host 'Thinktank on Antigravity'
    $global:Application.ShowMessage("メッセージ本文", "タイトル")
}


Add-TTAction    Application.Operation.Quit  '終了' {
    $res = $global:Application.ShowMessage("終了しますか", "QUIT", 'YesNo', 'Question')
    switch ( $res ) {
        'No' { return $false }
    }
    $global:Application.Close()
}

#endregion

#region ::: パネル
# Add-TTAction    Panel.Keyword.Clear             'Keywordクリア' {
#     $panel = [TTExModMode]::ExFdPanel()
#     $mode = $panel.GetMode()
#     Apply-TTState "$($panel.Name).$mode.Keyword" ''
#     return $true
#  }
# Add-TTAction    Panel.FontSize.Up               'パネル文字サイズ拡大' {
#     $pname = [TTExModMode]::ExFdPanel()
#     $state = Get-TTState "$pname.Panel.FontSize"
#     if ( [int]$state -lt 20 ) { $state = [int]$state + 1 }
#     Apply-TTState "$pname.Panel.FontSize" $state
#     return $true
#  }
# Add-TTAction    Panel.FontSize.Down             'パネル文字サイズ縮小' {
#     $pname = [TTExModMode]::ExFdPanel()
#     $state = Get-TTState "$pname.Panel.FontSize"
#     if ( 7 -lt [int]$state ) { $state = [int]$state - 1 }
#     Apply-TTState "$pname.Panel.FontSize" $state

#     return $true
#  }
#endregion