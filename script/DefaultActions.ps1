

Add-TTAction    Application.Run.Break       'デバッグ画面で改行' {
    Write-Host 'Thinktank on Antigravity'
    [System.Windows.MessageBox]::Show("メッセージ本文", "タイトル")
}
#endregion

Add-TTAction    Application.Operation.Quit  '終了' {
    $res = [System.Windows.MessageBox]::Show( "終了しますか", "QUIT", 'YesNo', 'Question')
    switch ( $res ) {
        'No' { return $false }
    }
    $global:Application.Close()
}
