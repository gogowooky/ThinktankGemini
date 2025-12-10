




#region Application.Product.*
New-TTState     Application.Product.Name            'アプリ名'                      'Thinktank'
New-TTState     Application.Product.Author          '制作者'                        'Shinichiro Egashira'
New-TTState     Application.Product.Mail            '連絡先'                        'gogowooky@gmail.com'
New-TTState     Application.Product.Site            '開発サイト'                    'https://github.com/gogowooky'
New-TTState     Application.Product.Version         'バージョン'                    @{
    Default = {
        $versionFile = "$global:ScriptPath\version.txt"
        
        if (Test-Path $versionFile) {
            return (Get-Content -Path $versionFile -Raw).Trim()
        }
        
        $timestamp = Get-Date
        "ver.$($timestamp.tostring('yyMMdd-HHmm')) on unknownPC"
    }
    Apply   = {
        Param($id, $val)
        $global:Models.Status.SetValue( $id, $val )
        
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $appName = (Get-TTState 'Application.Product.Name')
                $global:Application.SetTitle("$appName $val")
            })
    }
}
#endregion
#region Application.System.*
New-TTState     Application.System.RootPath         'ルートディレクトリ'            @{
    Default = { $global:Application.BaseDir }
    Apply   = {
        Param($id, $val)
        $global:Models.Status.SetValue( $id, $val )
    }
}
New-TTState     Application.System.ScriptPath       'スクリプトディレクトリ'        "$PSScriptRoot/script"
New-TTState     Application.System.PCName           'PC名'                          $Env:Computername
New-TTState     Application.System.UserName         'User名'                        $([System.Environment]::UserName)
New-TTState     Application.System.MemoPath         'メモディレクトリ'                  @{
    Default = { "$global:RootPath\..\Memo" }
    Apply   = {   
        Param($id, $val)
        if ( -not ( Test-Path $val )) {
            New-Item $val -ItemType Directory -ErrorAction SilentlyContinue
        }
        $global:MemoPath = $val
        $global:Models.Status.SetValue( $id, $val )
    }
}
New-TTState     Application.System.CachePath        'キャッシュディレクトリ'            @{
    Default = { "$global:RootPath\..\Memo\cache" }
    Apply   = {   
        Param($id, $val)
        if ( -not ( Test-Path $val )) {
            New-Item $val -ItemType Directory -ErrorAction SilentlyContinue
        }
        $global:CachePath = $val
        $global:Models.Status.SetValue( $id, $val )
    }
}
New-TTState     Application.System.BackupPath       'バックアップディレクトリ'          @{
    Default = { "$global:RootPath\..\Memo\backup" }
    Apply   = {
        Param($id, $val)
        if ( -not ( Test-Path $val )) {
            New-Item $val -ItemType Directory -ErrorAction SilentlyContinue
        }
        $global:BackupPath = $val
        $global:Models.Status.SetValue( $id, $val )
    }
}
New-TTState     Application.System.LinkPath         'リンクディレクトリ'                @{
    Default = { "$global:RootPath\..\Link" }
    Apply   = {
        Param($id, $val)
        if ( -not ( Test-Path $val )) {
            New-Item $val -ItemType Directory -ErrorAction SilentlyContinue
        }
        $global:LinkPath = $val
        $global:Models.Status.SetValue( $id, $val )
    }
}
# New-TTState     Application.System.OutlookBackupFolder  '引用メールの保存先フォルダ'    @{
#     Default = { '' }
#     Apply   = { Param($id, $val)
#         $global:Models.Status.SetValue( $id, $val )
#     }
# }
# New-TTState     Application.System.OutlookMainFolder  'メール確認先フォルダ'            @{
#     Default = { '' }
#     Apply   = { Param($id, $val)
#         $global:Models.Status.SetValue( $id, $val )
#     }
# }
# #endregion  
# #region Application.Window.*
# New-TTState     Application.Window.Screen           'ウインドウ表示スクリーン'      @{
#     Default = { '0' }
#     Test    = { Param($id, $val); $val -match '([0-9]|next|prev)' }
#     Apply   = { Param($id, $val)
#         $val = $global:Application.ChangeScreen( $val )
#         $global:Models.Status.SetValue( $id, $val )
#     }
# }
# New-TTState     Application.Window.State            'ウインドウ状態'                @{
#     Default = { 'Normal' }
#     Test    = { Param($id, $val); $val -match '(Minimized|Maximized|Normal)' }
#     Apply   = { Param($id, $val); $global:Application.MainWindow.Dispatcher.Invoke([Action]{ $global:Application.MainWindow.WindowState = $val }) }
#     Watch   = {
#         $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
#                 $global:Application.MainWindow.Add_StateChanged({
#                         Param( $win, $evnt )
#                         $global:Models.Status.SetValue( 'Application.Window.State', [string]$global:Application.MainWindow.WindowState )
#                     })
#             } )
#     }
# }
New-TTState     Application.Window.Width            'ウインドウ幅'                  @{
    Default = { '1200' }
    Test    = { Param($id, $val); $val -match '(\d{1,4}|inc|dec)' }
    Apply   = { Param($id, $val)
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                switch -regex ($val) {
                    'inc' { $val = $global:Application.MainWindow.Width + 10 }
                    'dec' { $val = $global:Application.MainWindow.Width - 10 }
                }
                $global:Application.MainWindow.Width = [int]$val
            })
    }
    Watch   = {
        try {
            $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                    $global:Application.MainWindow.Add_SizeChanged({
                            Param( $win, $evnt )
                            try {
                                $global:Models.Status.SetValue( 'Application.Window.Width', $win.Width )
                                $global:Models.Status.SetValue( 'Application.Window.Height', $win.Height )
                            }
                            catch { "ERROR in Window.Width Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                        })
                } )
        }
        catch { "ERROR registering Window.Width Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
    }
}
New-TTState     Application.Window.Height           'ウインドウ高'                  @{
    Default = { '600' }
    Test    = { Param($id, $val); $val -match '(\d{1,4}|inc|dec)' }
    Apply   = { Param($id, $val)
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                switch -regex ($val) {
                    'inc' { $val = $global:Application.MainWindow.Height + 10 }
                    'dec' { $val = $global:Application.MainWindow.Height - 10 }
                }
                $global:Application.MainWindow.Height = [int]$val
            })
    }
    # Watch = {}  #::: Application.Window.Width の Watchと共用 
}
New-TTState     Application.Window.XPos             'ウインドウ横位置'              @{
    Default = { '100' }
    Test    = { Param($id, $val); $val -match '(\d{1,4}|right|left)' }
    Apply   = { Param($id, $val)
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                switch -regex ($val) {
                    'right' { $val = $global:Application.MainWindow.Left + 10 }
                    'left' { $val = $global:Application.MainWindow.Left - 10 }
                }
                $global:Application.MainWindow.Left = $val
            })
    }
    Watch   = {
        try {
            $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                    $global:Application.MainWindow.Add_LocationChanged({
                            Param( $win, $evnt )
                            try {
                                $global:Models.Status.SetValue( 'Application.Window.YPos', $win.Top )
                                $global:Models.Status.SetValue( 'Application.Window.XPos', $win.Left )
                            }
                            catch { "ERROR in Window.XPos Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                        })
                } )
        }
        catch { "ERROR registering Window.XPos Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
    }
}
New-TTState     Application.Window.YPos             'ウインドウ縦位置'              @{
    Default = { '50' }
    Test    = { Param($id, $val); $val -match '(\d{1,4}|down|up)' }
    Apply   = { Param($id, $val)
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                switch -regex ($val) {
                    'down' { $val = $global:Application.MainWindow.Top + 10 }
                    'up' { $val = $global:Application.MainWindow.Top - 10 }
                }
                $global:Application.MainWindow.Top = $val
            })
    }
    # Watch = {}  #::: Application.Window.Left の Watchと共用 
}
New-TTState     Application.Window.FontSize         'アプリ全体のフォントサイズ'    @{
    Default = { 12 }
    Test    = { Param($id, $val); $val -match '(\d{1,2}|up|down)' }
    Apply   = { Param($id, $val)
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                switch ( $val ) {
                    'up' { $val = ( $global:Application.MainWindow.FontSize + 1 ) }
                    'down' { $val = ( $global:Application.MainWindow.FontSize - 1 ) }
                }
                $global:Application.MainWindow.FontSize = [int]$val
                $global:Application.Menu.FontSize = [int]$val
            })
        $global:Models.Status.SetValue( 'Application.Window.FontSize', $val )
        
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                @(  
                    'Library.Panel.FontSize', 
                    'Index.Panel.FontSize', 
                    'Shelf.Panel.FontSize', 
                    'Desk.Panel.FontSize', 
                    'System.Panel.FontSize').foreach{
                    Apply-TTState $_ $val
                }
            })
    }
}
New-TTState     Application.Window.Title            'ウインドウタイトル'            @{
    Default = { 
        $name = Get-TTState 'Application.Product.Name'
        $ver = Get-TTState 'Application.Product.Version'
        "$name $ver"
    }
    Apply   = { Param($id, $val)
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.SetTitle($val)
            })
        $global:Models.Status.SetValue('Application.Window.Title', $val)
    }
}
#endregion
#region Application.*
New-TTState     Application.Focus.Panel             'フォーカスパネル'              @{
    Default = { 'Desk' }
    Apply   = { Param( $id, $val )
        if ( $val -notmatch '^(Library|Index|Shelf|Desk|System)$' ) { 
            $val = $global:Application.GetFdPanel().Name # Safe? Check implementation
        }

        # Register-DelayedRun Application.Focus.Panel 1 {
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.$val.Focus()
            })
        $curstyle = (Get-TTState Application.Border.Style)
        if ( $curstyle -like 'zen*' ) {
            Apply-TTState Application.Border.Style "zen:$val"
        }
        # }.GetNewClosure()
    }
}


New-TTState     Application.Menu.Visible            'メニュー表示'                  @{
    Default = { 'true' }
    Test    = { Param($id, $val); $val -match '(true|false|toggle)' }
    Apply   = { Param($id, $val)
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                switch ( [string]$val ) {
                    'true' { $global:Application.Menu.Visibility = [Visibility]::Visible }
                    'false' { $global:Application.Menu.Visibility = [Visibility]::Collapsed }
                    default {
                        $vis = ( $global:Application.Menu.Visibility -eq [Visibility]::Visible )
                        $global:Application.Menu.Visibility = @( [Visibility]::Visible, [Visibility]::Collapsed )[ $vis ]
                    }
                }
            })
    }
    Watch   = {
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                if ($global:Application.Menu) {
                    $global:Application.Menu.Add_IsVisibleChanged({
                            Param($menu, $evnt)
                            $global:Models.Status.SetValue('Application.Menu.Visible', $menu.IsVisible)
                        })
                }
            } )
    }
}

New-TTState     Application.Current.ExMode          '排他モード'                    @{
    Default = { '' }
    Test    = { Param($id, $val); $val -match '(Ex.+|)' }
    Apply   = { Param($id, $val)
        switch ($val) {
            'Panel' { $val = 'Ex{0}' -f $global:Application.GetFdPanel().Name }
        }
        $global:Application.MainWindow.Dispatcher.InvokeAsync([Action] {
                $global:Application.SetExModMode( $val )
            }.GetNewClosure())
        $global:Models.Status.SetValue( 'Application.Current.ExMode', $val )
    }
}
New-TTState     Application.Border.Style            'パネル分割スタイル'            @{
    Default = { 'Free' }
    Test    = { Param($id, $val); $val -match '(free|all|standard|detail|list|zen|debug)' }
    Apply   = { Param($id, $val)
        $styles = @{
            'free'       = @{}
            'all'        = @{  User = '20'; LibraryIndex = '20'; ShelfDesk = '20'; UserSystem = '80' }
            'standard'   = @{  User = '15'; LibraryIndex = '20'; ShelfDesk = '20'; UserSystem = '100' }
            'detail'     = @{                                           ShelfDesk = '40'; UserSystem = '100' }
            'list'       = @{  User = '30'; LibraryIndex = '0'; UserSystem = '100' }
            'zen'        = @{  User = '0'; LibraryIndex = '20'; ShelfDesk = '0'; UserSystem = '100' }
            'debug'      = @{  User = '0'; LibraryIndex = '20'; ShelfDesk = '30'; UserSystem = '70' }
            'zenLibrary' = @{  User = '100'; LibraryIndex = '100'; ShelfDesk = '0'; UserSystem = '100' }
            'zenIndex'   = @{  User = '100'; LibraryIndex = '0'; ShelfDesk = '0'; UserSystem = '100' }
            'zenShelf'   = @{  User = '0'; LibraryIndex = '0'; ShelfDesk = '100'; UserSystem = '100' }
            'zenDesk'    = @{  User = '0'; LibraryIndex = '0'; ShelfDesk = '0'; UserSystem = '100' }
            'zenSystem'  = @{  User = '0'; LibraryIndex = '0'; ShelfDesk = '0'; UserSystem = '0' }
        }
        if ( $val -eq 'zen' ) {
            $curfocus = (Get-TTState Application.Focus.Panel)
            $val = "zen$curfocus"
        }
        $styles[$val].Keys.foreach{
            Apply-TTState "Application.Border.$_" $styles[$val].$_
        }
        $global:Models.Status.SetValue( 'Application.Border.Style', $val )
    }
}
New-TTState     Application.Border.User             'User境界位置'                  @{
    Default = { '20' }
    Test    = { Param($id, $val); $val -match '((\+|\-)?\d{1,2}|100)' }
    Apply   = { Param($id, $val); $global:Application.MainWindow.Dispatcher.Invoke([Action] { $global:Application.SetBorderPosition('User', $val) }) }
    Watch   = {
        try {
            $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                    $status = $global:Models.Status
                    $app = $global:Application
                    $g = $app.MainWindow.FindName('LibraryIndexGrid')
                    if ($g) {
                        $g.Add_SizeChanged({
                                try {
                                    $status.SetValue(   'Application.Border.User', $app.GetBorderPosition('User') )
                                }
                                catch { "ERROR in Border.User Watch 1: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                            }.GetNewClosure())
                    }
                    $g = $app.MainWindow.FindName('ShelfDeskGrid')
                    if ($g) {
                        $g.Add_SizeChanged({
                                try {
                                    $status.SetValue(   'Application.Border.User', $app.GetBorderPosition('User') )
                                }
                                catch { "ERROR in Border.User Watch 2: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                            }.GetNewClosure())
                    }
                } )
        }
        catch { "ERROR registering Border.User Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
    }
}
New-TTState     Application.Border.LibraryIndex     'LibraryIndex境界位置'          @{
    Default = { '20' }
    Test    = { Param($id, $val); $val -match '((\+|\-)?\d{1,2}|100)' }
    Apply   = { Param($id, $val); $global:Application.MainWindow.Dispatcher.Invoke([Action] { $global:Application.SetBorderPosition('LibraryIndex', $val) }) }
    Watch   = {
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $status = $global:Models.Status
                $app = $global:Application
                $g = $app.MainWindow.FindName('LibraryGrid')
                if ($g) {
                    $g.Add_SizeChanged({
                            $status.SetValue(   'Application.Border.LibraryIndex', $app.GetBorderPosition('LibraryIndex') )
                        }.GetNewClosure())
                }
                $g = $app.MainWindow.FindName('IndexGrid')
                if ($g) {
                    $g.Add_SizeChanged({
                            $status.SetValue(   'Application.Border.LibraryIndex', $app.GetBorderPosition('LibraryIndex') )
                        }.GetNewClosure())
                }
            } )
    }
}
New-TTState     Application.Border.ShelfDesk        'ShelfDesk境界位置'             @{
    Default = { '20' }
    Test    = { Param($id, $val); $val -match '((\+|\-)?\d{1,2}|100)' }
    Apply   = { Param($id, $val); $global:Application.MainWindow.Dispatcher.Invoke([Action] { $global:Application.SetBorderPosition('ShelfDesk', $val) }) }
    Watch   = {
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $status = $global:Models.Status
                $app = $global:Application
                $g = $app.MainWindow.FindName('ShelfGrid')
                if ($g) {
                    $g.Add_SizeChanged({
                            $status.SetValue(   'Application.Border.ShelfDesk', $app.GetBorderPosition('ShelfDesk') )
                        }.GetNewClosure())
                }
                $g = $app.MainWindow.FindName('DeskGrid')
                if ($g) {
                    $g.Add_SizeChanged({
                            $status.SetValue(   'Application.Border.ShelfDesk', $app.GetBorderPosition('ShelfDesk') )
                        }.GetNewClosure())
                }
            } )
    }
}
New-TTState     Application.Border.UserSystem       'UserSystem境界位置'            @{
    Default = { '80' }
    Test    = { Param($id, $val); $val -match '((\+|\-)?\d{1,2}|100)' }
    Apply   = { Param($id, $val); $global:Application.MainWindow.Dispatcher.Invoke([Action] { $global:Application.SetBorderPosition('UserSystem', $val) }) }
    Watch   = {
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $status = $global:Models.Status
                $app = $global:Application
                $g = $app.MainWindow.FindName('UserGrid')
                if ($g) {
                    $g.Add_SizeChanged({
                            $status.SetValue(   'Application.Border.UserSystem', $app.GetBorderPosition('UserSystem') )
                        }.GetNewClosure())
                }
                $g = $app.MainWindow.FindName('SystemGrid')
                if ($g) {
                    $g.Add_SizeChanged({
                            $status.SetValue(   'Application.Border.UserSystem', $app.GetBorderPosition('UserSystem') )
                        }.GetNewClosure())
                }
            } )
    }
}
# #endregion
# #region [Panels].Current.*
New-TTState     [Panels].Current.Mode               '[Panels]のモード'              @{
    Default = { Param($id)
        $map = @{
            Library = 'Table'; Index = 'Table'; Shelf = 'Table'; Desk = 'Editor'; System = 'WebView'
        }
        $map[ $id.split('.')[0] ]
    }
    Test    = { Param($id, $val); $val -match '^(Editor|Table|WebView|next|prev)$' }
    Apply   = { Param($id, $val); 
        $p = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.InvokeAsync([Action] {
                $global:Application.$p.SetMode( $val )
            }.GetNewClosure())
    }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $tools = $global:Application.PanelMap[$pname].Tools
                foreach ($tool in $tools) {
                    $tool.Add_IsVisibleChanged({ #::: Visibility変更時　Focusではない
                            Param($ctrl, $evnt)
                            try {
                                if ( $ctrl -ne $null -and $ctrl.Tag -ne $null -and $ctrl.IsVisible ) {
                                    $panel = $ctrl.Tag
                                    $pname = $panel.Name
                                    $mname = $ctrl.Name -replace '(Editor|Table|WebView)(Keyword|Main)', '$1'
                                    # $panel.CurrentTool = $panel."CurrentTool$mname"
                                    $global:Models.Status.SetValue( "$pname.Current.Mode", $mname )
                                }
                            }
                            catch {
                                "ERROR in [Panels].Current.Mode Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append
                            }
                        })
                }
            } )
    }
}
$global:tool_gotfocus = {
    Param( $ctrl, $evnt )

    if ($ctrl -eq $null -or $ctrl.Tag -eq $null) {
        "DEBUG: tool_gotfocus called with null ctrl or Tag" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append
        return
    }

    try {
        $pname = $ctrl.Tag.Name
        $mname = $ctrl.Name -replace '(Editor|Table|WebView)(Keyword|Main)', '$1'
        $tname = $ctrl.Name -replace '(Editor|Table|WebView)(Keyword|Main)', '$2'

        # $global:Application.PostFocused( $ctrl )  # Unimplemented
        # $global:Application.PanelMap[$pname].RestoreCurrentTool( $ctrl )  # Unimplemented

        # $global:Application.SwitchKeyTable( $pname + $ctrl.Name )  # Unimplemented

        if ($pname) {
            $global:Models.Status.SetValue( 'Application.Focus.Panel', $pname )
            $global:Models.Status.SetValue( "$pname.Current.Mode", $mname )
            $global:Models.Status.SetValue( "$pname.Current.Tool", $tname )
        }
    }
    catch {
        "ERROR in tool_gotfocus: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append
    }
}
New-TTState     [Panels].Current.Tool               '[Panels]のツール'              @{
    Default = { Param($id)
        $map = @{
            Library = 'TableKeyword'; Index = 'TableKeyword'; Shelf = 'TableKeyword'; Desk = 'EditorMain'; System = 'EditorKeyword'
        }
        $map[ $id.split('.')[0] ]
    }
    Test    = { Param($id, $val); $val -match '^(Editor|Table|WebView)?(Keyword|Main|toggle)$' }
    Apply   = { Param($id, $val);
        $p = $id.split('.')[0]; $global:Application.MainWindow.Dispatcher.InvokeAsync([Action] { $global:Application.$p.SetTool( $val ) }.GetNewClosure()) }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $tools = $global:Application.PanelMap[$pname].Tools
                foreach ($tool in $tools) {
                    $tool.Add_PreviewTouchDown($global:tool_gotfocus)
                    $tool.Add_PreviewMouseDown($global:tool_gotfocus)
                    $tool.Add_GotFocus($global:tool_gotfocus)
                }
            } )
    }
}
#endregion
#region [Panels].Title.*
New-TTState     [Panels].Title.Visible              '[Panels]タイトル表示'          @{
    Default = { 'true' }
    Test    = { Param($id, $val); $val -match '(true|false|toggle)' }
    Apply   = { Param($id, $val); $p = $id.split('.')[0]; $global:Application.MainWindow.Dispatcher.Invoke([Action] { $global:Application.$p.SetTitleVisible( $val ) }) }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $global:Application.PanelMap[$pname].Title.Add_IsVisibleChanged({
                        Param($ttl, $evnt)
                        try {
                            $pname = $ttl.Tag.Name
                            $global:Models.Status.SetValue( "$pname.Title.Visible", $ttl.IsVisible )
                        }
                        catch {
                            "ERROR in [Panels].Title.Visible Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append
                        }
                    }
                )
            } )
    }
}
New-TTState     [Panels].Title.Text                 '[Panels]タイトル文字'          @{
    Default = { Param($id); return $id.split('.')[0] }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.PanelMap[$pname].Title.Content = $val
            })
        $global:Models.Status.SetValue( $id, $val )
    }
}
#endregion
#region [Panels].Editor.*
write-host "250620: [Panels].Editor.Keywordはメモ毎に設定されるものと切り替えて設定されるものがマージされるべき"
New-TTState     [Panels].Editor.Keyword             '[Panels]エディタキーワード'    @{
    Default = { '' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.PanelMap[$pname].SetKeyword( 'Editor', $val )
            })
    }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $global:Application.PanelMap[$pname].EditorKeyword.Add_TextChanged({
                        Param($kwd, $evnt)
                        try {
                            $pn = $kwd.Tag.Name
                            $global:Models.Status.SetValue( "$pn.Editor.Keyword", $kwd.Tag.GetKeyword('Editor') )
                        }
                        catch { "ERROR in [Panels].Editor.Keyword TextChanged: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                    })
                $global:Application.PanelMap[$pname].EditorKeyword.TextArea.TextView.Add_ScrollOffsetChanged({
                        param($tv, $e)
                        try {
                            $edit = $tv.EditorComponent
                            $currentVerticalOffset = $tv.VerticalOffset
                            $isCaretAtFirstLine = $edit.Document.GetLineByOffset( $edit.CaretOffset ).LineNumber -eq 1
        
                            $halfLineHeight = $tv.DefaultLineHeight / 2
        
                            $scrollDifference = [Math]::Abs($currentVerticalOffset - $global:previousVerticalOffset)
        
                            if (    $isCaretAtFirstLine -and
                                $currentVerticalOffset -ne 0 -and
                                $scrollDifference -ge ($halfLineHeight - 0.1) -and
                                $scrollDifference -le ($halfLineHeight + 0.1) ) {
                                $edit.ScrollToVerticalOffset(0)
                            }
        
                            $global:previousVerticalOffset = $currentVerticalOffset
                        }
                        catch { "ERROR in [Panels].Editor.Keyword Scroll: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                    })
                $global:Application.PanelMap[$pname].EditorKeyword.TextArea.Caret.Add_PositionChanged({
                        Param( $crt, $evnt ) 
                        try {
                            $pn = $pname
                            $panel = $global:Application.PanelMap[$pn]
                            $global:Models.Status.SetValue( "$pn.Editor.Keyword", $panel.GetKeyword('Editor') )
                        }
                        catch { "ERROR in [Panels].Editor.Keyword Caret: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                    }.GetNewClosure())
            } )
    }
}
New-TTState     [Panels].Editor.Memo                '[Panels]メモID'                @{
    Default = { 'thinktank' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $panel = $global:Application.PanelMap[$pname]
        if ( $val -in @( '', $panel.MemoID ) ) { return }
        $panel.MemoID = $val
        $global:Controller.LoadMemo( $panel, $val )
    }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $global:Application.PanelMap[$pname].EditorMain.Add_DocumentChanged({
                        Param($edt, $evnt)
                        $pname = $edt.TTPanel.Name
                        $global:Models.Status.SetValue( "$pname.Editor.Memo", $edt.TTPanel.MemoID )
                    })
            } )
    }
}
New-TTState     [Panels].Editor.Wordwrap            '[Panels]メモWordwrap'          @{
    Default = { 'false' }
    Test    = { Param($id, $val); $val -match '(true|false|toggle)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                if ( $val -eq 'toggle' ) {
                    $val = @( 'true', 'false')[ $global:Application.PanelMap[$pname].EditorMain.Wordwrap ]
                }
                $global:Application.PanelMap[$pname].EditorMain.Wordwrap = [bool]$val
            })
    }
}
#endregion
#region [Panels].Table.*
New-TTState     [Panels].Table.Keyword              '[Panels]テーブルキーワード'    @{
    Default = { Param($id)
        $map = @{
            Library = ''
            Index   = '@7d'
            Shelf   = ''
            Desk    = ''
            System  = 'http://google.com'
        }
        $map[ $id.split('.')[0] ]
    }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.PanelMap[$pname].SetKeyword( 'Table', $val )
            })
    }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $global:Application.PanelMap[$pname].TableKeyword.Add_TextChanged({
                        Param($kwd, $evnt)
                        try {
                            $panel = $kwd.Tag
                            $pn = $panel.Name
                            $global:Models.Status.SetValue( "$pn.Table.Keyword", $panel.GetKeyword('Table') )
                        }
                        catch {
                            "ERROR in [Panels].Table.Keyword Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append
                        }
                    }.GetNewClosure())
            } )
    }
}
New-TTState     [Panels].Table.Resource             '[Panels]リソース名'            @{
    Default = { Param($id)
        $map = @{
            Library = 'Thinktank'; Index = 'Status'; Shelf = 'Actions'; Desk = 'Memos'; System = 'Memos'
        }
        $map[ $id.split('.')[0] ]
    }
    Apply   = { Param($id, $val); $p = $id.split('.')[0]; $global:Application.MainWindow.Dispatcher.Invoke([Action] { $global:Application.$p.SetTableResource( $val ) }) }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $global:Application.PanelMap[$pname].TableMain.Add_SourceUpdated({
                        Param($tbl, $evnt)
                        try {
                            $pname = $tbl.Tag.Name
                            $global:Models.Status.SetValue( "$pname.Table.Resource", $tbl.Tag.TableResource )
                        }
                        catch { "ERROR in [Panels].Table.Resource Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                    })
            } )
    }
}
New-TTState     [Panels].Table.Sort                 '[Panels]ソート'                @{
    Default = { Param($id)
        $map = @{
            Library = 'ID|Descending'
            Index   = 'UpdateDate|Descending'
            Shelf   = 'Name|Ascending'
            Desk    = 'ID|Descending'
            System  = 'UpdateDate|Descending'
        }
        $map[ $id.split('.')[0] ]
    }
    Test    = { Param($id, $val); $val -match '.+\|(Ascending|Descending)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.PanelMap[$pname].SetTableSort( $val )
            })
        $global:Models.Status.SetValue( $id, $val )
    }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $global:Application.PanelMap[$pname].TableMain.Add_Sorting({
                        Param($tbl, $evnt)
                        try {
                            $pname = $tbl.Tag.Name
                            $sort = [System.Windows.Data.CollectionViewSource]::GetDefaultView( $tbl.ItemsSource ).SortDescriptions[0]
                            $sortval = ('{0}|{1}' -f $sort.PropertyName, $sort.Direction)
                            $global:Models.Status.SetValue( "$pname.Table.Sort", $sortval )
                        }
                        catch { "ERROR in [Panels].Table.Sort Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                    })
            } )
    }
}
#endregion
#region [Panels].WebView.*
New-TTState     [Panels].WebView.Keyword            '[Panels]ウェブビューキーワード'    @{
    Default = { Param($id)
        $map = @{
            Library = ''
            Index   = ''
            Shelf   = ''
            Desk    = ''
            System  = 'https://www.google.com'
        }
        $map[ $id.split('.')[0] ]
    }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.PanelMap[$pname].SetKeyword( 'WebView', $val )
            })
    }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $global:Application.PanelMap[$pname].WebViewKeyword.Add_TextChanged({
                        Param($kwd, $evnt)
                        try {
                            $panel = $kwd.Tag
                            $pn = $panel.Name
                            $md = $panel.GetMode()
                            $global:Models.Status.SetValue( "$pn.$md.Keyword", $panel.GetKeyword('WebView') )
                        }
                        catch { "ERROR in [Panels].WebView.Keyword Watch: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                    })
                $global:Application.PanelMap[$pname].WebViewKeyword.TextArea.Caret.Add_PositionChanged({
                        Param($crt, $evnt)
                        try {
                            $pn = $pname
                            $panel = $global:Application.PanelMap[$pn]
                            $md = $panel.GetMode()
                            $global:Models.Status.SetValue( "$pn.$md.Keyword", $panel.GetKeyword('WebView') )
                        }
                        catch { "ERROR in [Panels].WebView.Keyword Caret: $_" | Out-File "c:\Users\shinichiro.egashira\Documents\ThinktankGemini\ThinktankGemini\debug_watch.txt" -Append }
                    }.GetNewClosure())
            } )
    }
}
#endregion
#endregion
#region [Panels].*
New-TTState     [Panels].Keyword.Visible            '[Panels]キーワード表示'          @{
    Default = { 'true' }
    Test    = { Param($id, $val); $val -match '(true|false)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.PanelMap[$pname].SetKeywordVisible( $val )
            })
    }
    Watch   = { Param($id)
        $global:Application.MainWindow.Dispatcher.Invoke( [Action] {
                $pname = $id.split('.')[0]
                $global:Application.PanelMap[$pname].EditorKeyword.Add_IsVisibleChanged({
                        Param($kwd, $evnt)
                        $pname = $kwd.Tag.Name
                        $global:Models.Status.SetValue( "$pname.Keyword.Visible", $kwd.IsVisible )
                    })
            } )
    }
}
New-TTState     [Panels].ColumnHeader.Visible       '[Panels]カラムヘッダー'        @{
    Default = { 'true' }
    Test    = { Param($id, $val); $val -match '(true|false|toggle)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.PanelMap[$pname].SetColumnHeaderVisible( $val )
                $newval = $global:Application.PanelMap[$pname].GetColumnHeaderVisible()
                $global:Models.Status.SetValue( $id, $newval )
            })
    }
}
New-TTState     [Panels].RowHeader.Visible          '[Panels]ロウヘッダー'          @{
    Default = { 'true' }
    Test    = { Param($id, $val); $val -match '(true|false|toggle)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.PanelMap[$pname].SetRowHeaderVisible( $val )
                $newval = $global:Application.PanelMap[$pname].GetRowHeaderVisible()
                $global:Models.Status.SetValue( $id, $newval )
            })
    }
}
New-TTState     [Panels].Panel.FontSize             '[Panels]フォントサイズ'        @{
    Default = { 12 }
    Test    = { Param($id, $val); $val -match '(\d{1,2}|up|down)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.MainWindow.Dispatcher.Invoke([Action] {
                $global:Application.PanelMap[$pname].SetFontSize( $val )
            })
        $global:Models.Status.SetValue( "$pname.Panel.FontSize", $val )
    }
}
#endregion
