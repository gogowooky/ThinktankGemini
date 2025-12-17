




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
        "Thinktank ver.$($timestamp.tostring('yyMMdd-HHmm')) on unknownPC"
    }
    Apply   = {
        Param($id, $val)
        $global:Models.Status.SetValue( $id, $val )
        
        $appName = (Get-TTState 'Application.Product.Name')
        $global:Application.Title = "$appName/$val"
    }
}
#endregion
#region Application.System.*
New-TTState     Application.System.RootPath         'ルートディレクトリ'            @{
    Default = { $global:Application.BaseDir }
    Apply   = {
        Param($id, $val)
        $global:Models.Status.SetValue( $id, $val )

        $global:Application.BaseDir = $val
    }
}
New-TTState     Application.System.PCName           'PC名'                          $global:Application.PCName
New-TTState     Application.System.UserName         'User名'                        $global:Application.UserName
New-TTState     Application.System.MemoPath         'メモディレクトリ'                  @{
    Default = { $global:Application.MemoDir }
    Apply   = {   
        Param($id, $val)
        $global:Application.MemoDir = $val
        $global:Models.Status.SetValue( $id, $val )
    }
}
New-TTState     Application.System.CachePath        'キャッシュディレクトリ'            @{
    Default = { Join-Path $global:Application.MemoDir "gcache" }
    Apply   = {   
        Param($id, $val)
        $global:Models.Status.SetValue( $id, $val )
    }
}
New-TTState     Application.System.BackupPath       'バックアップディレクトリ'          @{
    Default = { Join-Path $global:Application.MemoDir "gbackup" }
    Apply   = {
        Param($id, $val)
        $global:Models.Status.SetValue( $id, $val )
    }
}
New-TTState     Application.System.LinkPath         'リンクディレクトリ'                @{
    Default = { $global:Application.LinkDir }
    Apply   = {
        Param($id, $val)
        $global:Application.LinkDir = $val
        $global:Models.Status.SetValue( $id, $val )
    }
}
New-TTState     Application.System.ChatPath         'チャットディレクトリ'               @{
    Default = { $global:Application.ChatDir }
    Apply   = {
        Param($id, $val)
        $global:Application.ChatDir = $val
        $global:Models.Status.SetValue( $id, $val )
    }
}
#endregion  
#region Application.Window.*
New-TTState     Application.Window.Screen           'ウインドウ表示スクリーン'      @{
    Default = { '0' }
    Test    = { Param($id, $val); $val -match '([0-9]|next|prev)' }
    Apply   = { Param($id, $val)
        $val = $global:Application.ChangeScreen( $val )
        $global:Models.Status.SetValue( $id, $val )
    }
}
New-TTState     Application.Window.State            'ウインドウ状態'                @{
    Default = { 'norm' }
    Test    = { Param($id, $val); $val -match '(min|max|norm)' }
    Apply   = { Param($id, $val); $global:Application.Window.State = $val }
    Watch   = {
        $global:Application.Window.Add_StateChanged({
                Param( $win, $evnt )
                $global:Models.Status.SetValue( 'Application.Window.State', [string]$global:Application.Window.State )
            })
    }
}
New-TTState     Application.Focus.Panel             'フォーカスパネル'              @{
    Default = { 'Desk' }
    Test    = { Param($id, $val); $val -match '^(Library|Index|Shelf|Desk|System)$' }
    Apply   = { Param( $id, $val )
        $global:Application.CurrentPanel = $val
    }
    Watch   = {
        $global:Application.Panels | ForEach-Object {
            $_.Add_FocusChanged({
                    Param( $pname, $mode, $tool )
                    $global:Models.Status.SetValue( 'Application.Focus.Panel', $pname )
                }.GetNewClosure())
        }
    }
}
New-TTState     Application.Current.ExMode          '排他モード'                    @{
    Default = { '' }
    Test    = { Param($id, $val); $val -match '(Ex.+|)' }
    Apply   = { Param($id, $val)
        $global:Application.ExMode = $val
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
    Apply   = { Param($id, $val); $global:Application.SetBorderPosition('User', $val) }
    Watch   = {
        $status = $global:Models.Status
        $app = $global:Application
        $app.LibraryIndexGrid.Add_SizeChanged({
                $status.SetValue(   'Application.Border.User', $app.GetBorderPosition('User') )
            }.GetNewClosure())
        # $app.LibraryIndexGrid.Add_SizeChanged({})
        $app.ShelfDeskGrid.Add_SizeChanged({
                $status.SetValue(   'Application.Border.User', $app.GetBorderPosition('User') )
            }.GetNewClosure())
        $app.ShelfDeskGrid.Add_SizeChanged({})
    }
}
New-TTState     Application.Border.LibraryIndex     'LibraryIndex境界位置'          @{
    Default = { '20' }
    Test    = { Param($id, $val); $val -match '((\+|\-)?\d{1,2}|100)' }
    Apply   = { Param($id, $val); $global:Application.SetBorderPosition('LibraryIndex', $val) }
    Watch   = {
        $status = $global:Models.Status
        $app = $global:Application
        $app.LibraryGrid.Add_SizeChanged({
                $status.SetValue(   'Application.Border.LibraryIndex', $app.GetBorderPosition('LibraryIndex') )
            }.GetNewClosure())
        # $app.LibraryGrid.Add_SizeChanged({})
        $app.IndexGrid.Add_SizeChanged({
                $status.SetValue(   'Application.Border.LibraryIndex', $app.GetBorderPosition('LibraryIndex') )
            }.GetNewClosure())
        # $app.IndexGrid.Add_SizeChanged({})
    }
}
New-TTState     Application.Border.ShelfDesk        'ShelfDesk境界位置'             @{
    Default = { '20' }
    Test    = { Param($id, $val); $val -match '((\+|\-)?\d{1,2}|100)' }
    Apply   = { Param($id, $val); $global:Application.SetBorderPosition('ShelfDesk', $val) }
    Watch   = {
        $status = $global:Models.Status
        $app = $global:Application
        $app.ShelfGrid.Add_SizeChanged({
                $status.SetValue(   'Application.Border.ShelfDesk', $app.GetBorderPosition('ShelfDesk') )
            }.GetNewClosure())
        # $app.ShelfGrid.Add_SizeChanged({})
        $app.DeskGrid.Add_SizeChanged({
                $status.SetValue(   'Application.Border.ShelfDesk', $app.GetBorderPosition('ShelfDesk') )
            }.GetNewClosure())
        # $app.DeskGrid.Add_SizeChanged({})
    }
}
New-TTState     Application.Border.UserSystem       'UserSystem境界位置'            @{
    Default = { '80' }
    Test    = { Param($id, $val); $val -match '((\+|\-)?\d{1,2}|100)' }
    Apply   = { Param($id, $val); $global:Application.SetBorderPosition('UserSystem', $val) }
    Watch   = {
        $status = $global:Models.Status
        $app = $global:Application
        # $app.UserGrid.Add_SizeChanged({
        #         $status.SetValue(   'Application.Border.UserSystem', $app.GetBorderPosition('UserSystem') )
        #     }.GetNewClosure())
        $app.UserGrid.Add_SizeChanged({})
        # $app.SystemGrid.Add_SizeChanged({
        #         $status.SetValue(   'Application.Border.UserSystem', $app.GetBorderPosition('UserSystem') )
        #     }.GetNewClosure())
        $app.SystemGrid.Add_SizeChanged({})
    }
}
#endregion
#region [Panels].Current.*
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
        $global:Application.PanelMap[$p].Mode = $val
    }
    Watch   = { Param($id)
        $pname = $id.split('.')[0]
        $global:Application.PanelMap[$pname].Tools.foreach{
            # $_.Add_IsVisibleChanged({ #::: Visibility変更時　Focusではない
            #         Param($ctrl, $evnt)
            #         if ( $ctrl.IsVisible ) {
            #             $panel = $ctrl.TTPanel
            #             $pname = $panel.Name
            #             $mname = $ctrl.Name -replace '(Editor|Table|WebView)(Keyword|Main)', '$1'
            #             $panel.CurrentTool = $panel."CurrentTool$mname"
            #             $global:Models.Status.SetValue( "$pname.Current.Mode", $mname )
            #         }
            #     })
            $_.Add_IsVisibleChanged({})
        }
    }
}
$global:tool_gotfocus = {
    Param( $ctrl, $evnt )

    $pname = $ctrl.TTPanel.Name
    $mname = $ctrl.Name -replace '(Editor|Table|WebView)(Keyword|Main)', '$1'
    $tname = $ctrl.Name -replace '(Editor|Table|WebView)(Keyword|Main)', '$2'

    $global:Application.PostFocused( $ctrl )
    $global:Application.PanelMap[$pname].RestoreCurrentTool( $ctrl )

    $global:Application.SwitchKeyTable( $pname + $ctrl.Name )

    $global:Models.Status.SetValue( 'Application.Focus.Panel', $pname )
    $global:Models.Status.SetValue( "$pname.Current.Mode", $mname )
    $global:Models.Status.SetValue( "$pname.Current.Tool", $tname )

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
        $p = $id.split('.')[0]; $global:Application.PanelMap[$p].Tool = $val }
    Watch   = { Param($id)
        $pname = $id.split('.')[0]

        $global:Application.PanelMap[$pname].Tools.foreach{
            # $_.Add_PreviewTouchDown($global:tool_gotfocus)
            $_.Add_PreviewTouchDown({})
            # $_.Add_PreviewMouseDown($global:tool_gotfocus)
            $_.Add_PreviewMouseDown({})
            # $_.Add_GotFocus($global:tool_gotfocus)
            $_.Add_GotFocus({})
        }
    }
}
#endregion
#region [Panels].Title.*
New-TTState     [Panels].Title.Visible              '[Panels]タイトル表示'          @{
    Default = { 'true' }
    Test    = { Param($id, $val); $val -match '(true|false|toggle)' }
    Apply   = { Param($id, $val); $p = $id.split('.')[0]; $global:Application.PanelMap[$p].SetTitleVisible( $val ) }
    Watch   = { Param($id)
        $pname = $id.split('.')[0]
        # $global:Application.PanelMap[$pname].Title.Add_IsVisibleChanged({
        #         Param($ttl, $evnt)
        #         $pname = $ttl.TTPanel.Name
        #         $global:Models.Status.SetValue( "$pname.Title.Visible", $ttl.IsVisible )
        #     }
        # )
        $global:Application.PanelMap[$pname].Title.Add_IsVisibleChanged({})
    }
}
New-TTState     [Panels].Title.Text                 '[Panels]タイトル文字'          @{
    Default = { Param($id); return $id.split('.')[0] }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.PanelMap[$pname].Title.Content = $val
        $global:Models.Status.SetValue( $id, $val )
    }
}
#endregion
# #region [Panels].Editor.*
New-TTState     [Panels].Editor.Keyword             '[Panels]エディタキーワード'    @{
    Default = { '' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.PanelMap[$pname].SetKeyword( 'Editor', $val )
    }
    Watch   = { Param($id)
        $pname = $id.split('.')[0]
        $panel = $global:Application.PanelMap[$pname]

        if ($panel.EditorKeyword -ne $null) {
            $panel.EditorKeyword.Add_TextChanged({
                    Param($kwd, $evnt)
                    # $pn = $kwd.TTPanel.Name
                    # $global:Models.Status.SetValue( "$pn.Editor.Keyword", $kwd.TTPanel.GetKeyword('Editor') )
                    # # Register-DelayedRun "$pn.EditorKeyword.TextChanged" 3 {
                    # $global:Application.$pn.UpdateKeywordRegex() # EditorMainの変更時はこちらは不要
                    # $global:Application.$pn.UpdateHighlight()
                    # }.GetNewClosure()
                })
            # Check for TextArea/TextView existence if needed, but EditorKeyword usually has them
            if ($panel.EditorKeyword.TextArea -ne $null) {
                $panel.EditorKeyword.TextArea.TextView.Add_ScrollOffsetChanged({
                        param($tv, $e)
                        # ...
                    })
                $panel.EditorKeyword.TextArea.Caret.Add_PositionChanged({
                        Param( $crt, $evnt ) 
                        # ...
                    })
            }
        }
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
        $pname = $id.split('.')[0]
        $panel = $global:Application.PanelMap[$pname]
        
        if ($panel.EditorMain -ne $null) {
            $panel.EditorMain.Add_DocumentChanged({
                    Param($edt, $evnt)
                    $pname = $edt.TTPanel.Name
                    $global:Models.Status.SetValue( "$pname.Editor.Memo", $edt.TTPanel.MemoID )
                })
        }
    }
}
New-TTState     [Panels].Editor.Wordwrap            '[Panels]メモWordwrap'          @{
    Default = { 'false' }
    Test    = { Param($id, $val); $val -match '(true|false|toggle)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        if ( $val -eq 'toggle' ) {
            $val = @( 'true', 'false')[ $global:Application.PanelMap[$pname].EditorMain.Wordwrap ]
        }
        $global:Application.PanelMap[$pname].EditorMain.Wordwrap = [bool]$val
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
        $global:Application.PanelMap[$pname].SetKeyword( 'Table', $val )
    }
    Watch   = { Param($id)
        $pname = $id.split('.')[0]
        $panel = $global:Application.PanelMap[$pname]

        if ($panel.TableKeyword -ne $null) {
            $panel.TableKeyword.Add_TextChanged({
                    Param($kwd, $evnt)
                    $pn = $pname
                    $global:Models.Status.SetValue( "$pn.Table.Keyword", $global:Application.PanelMap[$pn].GetKeyword('Table') )
                    $global:Application.Panels | ForEach-Object {
                        if ($_.GetMode() -eq 'Table') { $_.UpdateTableFilter() }
                    }
                }.GetNewClosure())
            $panel.TableKeyword.TextArea.Caret.Add_PositionChanged({
                    Param( $crt, $evnt ) 
                    $pn = $pname
                    $panel = $global:Application.PanelMap[$pn]
                    $editor = $panel.TableKeyword
                    
                    $global:Models.Status.SetValue( "$pn.Table.Keyword", $panel.GetKeyword('Table') )
                    $panel.UpdateTableFilter()

                    $line = $editor.Document.GetLineByOffset($editor.CaretOffset).LineNumber
                    $editor.ScrollToLine($line)
                    if ($line -eq 1) {
                        $editor.ScrollToVerticalOffset(0)
                    }
                }.GetNewClosure())
        }
    }
}
New-TTState     [Panels].Table.Resource             '[Panels]リソース名'            @{
    Default = { Param($id)
        $map = @{
            Library = 'Thinktank'; Index = 'Status'; Shelf = 'Actions'; Desk = 'Events'; System = 'Memos'
        }
        $map[ $id.split('.')[0] ]
    }
    Apply   = { Param($id, $val); $p = $id.split('.')[0]; $global:Application.PanelMap[$p].SetTableResource( $val ) }
    Watch   = { Param($id)
        $pname = $id.split('.')[0]
        $panel = $global:Application.PanelMap[$pname]
        
        if ($panel.TableMain -ne $null) {
            # $panel.TableMain.Add_SourceUpdated({
            #         Param($tbl, $evnt)
            #         $pname = $tbl.TTPanel.Name
            #         $global:Models.Status.SetValue( "$pname.Table.Resource", $tbl.TTPanel.TableResource )
            #     })
            $panel.TableMain.Add_SourceUpdated({})
        }
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
        $global:Application.PanelMap[$pname].SetTableSort( $val )
        $global:Models.Status.SetValue( $id, $val )
    }
    Watch   = { Param($id)
        $pname = $id.split('.')[0]
        $panel = $global:Application.PanelMap[$pname]
        
        if ($panel.TableMain -ne $null) {
            # $panel.TableMain.Add_Sorting({
            #         Param($tbl, $evnt)
            #         $pname = $tbl.TTPanel.Name
            #         $sort = [System.Windows.Data.CollectionViewSource]::GetDefaultView( $tbl.ItemsSource ).SortDescriptions[0]
            #         $sortval = ('{0}|{1}' -f $sort.PropertyName, $sort.Direction)
            #         $global:Models.Status.SetValue( "$pname.Table.Sort", $sortval )
            #     })
            $panel.TableMain.Add_Sorting({})
        }
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
        $global:Application.PanelMap[$pname].SetKeyword( 'WebView', $val )
    }
    Watch   = { Param($id)
        $pname = $id.split('.')[0]
        $panel = $global:Application.PanelMap[$pname]
        
        if ($panel.WebViewKeyword -ne $null) {
            # $panel.WebViewKeyword.Add_TextChanged({
            #         Param($kwd, $evnt)
            #         $panel = $kwd.TTPanel
            #         $pn = $panel.Name
            #         $md = $panel.GetMode()
            #         $global:Models.Status.SetValue( "$pn.$md.Keyword", $panel.GetKeyword('WebView') )
            #         # $panel.UpdateMarker('WebView')
            #     })
            $panel.WebViewKeyword.Add_TextChanged({})
            # $panel.WebViewKeyword.TextArea.Caret.Add_PositionChanged({
            #         Param($kwd, $evnt)
            #         $panel = $kwd.TTPanel
            #         $pn = $panel.Name
            #         $md = $panel.GetMode()
            #         $global:Models.Status.SetValue( "$pn.$md.Keyword", $panel.GetKeyword('WebView') )
            #         # $panel.UpdateMarker('WebView')
            #     })
            if ($panel.WebViewKeyword.TextArea -ne $null -and $panel.WebViewKeyword.TextArea.Caret -ne $null) {
                $panel.WebViewKeyword.TextArea.Caret.Add_PositionChanged({})
            }
        }
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
        $global:Application.PanelMap[$pname].SetKeywordVisible( $val )
    }
    Watch   = { Param($id)
        $pname = $id.split('.')[0]
        # $global:Application.PanelMap[$pname].EditorKeyword.Add_IsVisibleChanged({
        #         Param($kwd, $evnt)
        #         $pname = $kwd.TTPanel.Name
        #         $global:Models.Status.SetValue( "$pname.Keyword.Visible", $kwd.IsVisible )
        #     })
        $global:Application.PanelMap[$pname].EditorKeyword.Add_IsVisibleChanged({})
    }
}
New-TTState     [Panels].ColumnHeader.Visible       '[Panels]カラムヘッダー'        @{
    Default = { 'true' }
    Test    = { Param($id, $val); $val -match '(true|false|toggle)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.PanelMap[$pname].ColumnHeaderVisibility = $val
        $newval = $global:Application.PanelMap[$pname].ColumnHeaderVisibility
        $global:Models.Status.SetValue( $id, $newval )
    }
}
New-TTState     [Panels].RowHeader.Visible          '[Panels]ロウヘッダー'          @{
    Default = { 'true' }
    Test    = { Param($id, $val); $val -match '(true|false|toggle)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.PanelMap[$pname].RowHeaderVisibility = $val
        $newval = $global:Application.PanelMap[$pname].RowHeaderVisiblity
        $global:Models.Status.SetValue( $id, $newval )
    }
}
New-TTState     [Panels].Panel.FontSize             '[Panels]フォントサイズ'        @{
    Default = { 12 }
    Test    = { Param($id, $val); $val -match '(\d{1,2}|up|down)' }
    Apply   = { Param($id, $val)
        $pname = $id.split('.')[0]
        $global:Application.PanelMap[$pname].SetFontSize( $val )
        $global:Models.Status.SetValue( "$pname.Panel.FontSize", $val )
    }
}
#endregion








