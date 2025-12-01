
#region ::: ExMode Actions
Add-TTAction 'Application.Current.ExMode:ExApp' 'Switch to ExApp mode' {
    $global:Application.SetExModMode('ExApp')
    return $true
}

Add-TTAction 'Application.Current.ExMode:ExLibrary' 'Switch to ExLibrary mode' {
    $global:Application.SetExModMode('ExLibrary')
    return $true
}

Add-TTAction 'Application.Current.ExMode:ExIndex' 'Switch to ExIndex mode' {
    $global:Application.SetExModMode('ExIndex')
    return $true
}

Add-TTAction 'Application.Current.ExMode:ExShelf' 'Switch to ExShelf mode' {
    $global:Application.SetExModMode('ExShelf')
    return $true
}

Add-TTAction 'Application.Current.ExMode:ExDesk' 'Switch to ExDesk mode' {
    $global:Application.SetExModMode('ExDesk')
    return $true
}

Add-TTAction 'Application.Current.ExMode:ExSystem' 'Switch to ExSystem mode' {
    $global:Application.SetExModMode('ExSystem')
    return $true
}

Add-TTAction 'Application.Current.ExMode:Normal' 'Switch to Normal mode' {
    $global:Application.SetExModMode('')
    return $true
}
#endregion

#region ::: Thinktank2 からの登録

Add-TTAction    Application.Run.Break                   'デバッグ画面で改行' {
    Write-Host 'Thinktank on Antigravity'
    [System.Windows.MessageBox]::Show("メッセージ本文", "タイトル")
    return $true
}
#endregion

Add-TTAction    Application.Operation.Quit              '終了' {
    $res = [System.Windows.MessageBox]::Show( "終了しますか", "QUIT", 'YesNo', 'Question')
    switch ( $res ) {
        'No' { return $true }
    }
    $global:Application.Close()
    return $true
}
