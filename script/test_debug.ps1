# Mock global objects
$global:Models = @{
    Status = @{
        AddItem = { Param($id, $desc, $scripts) Write-Host "Added $id" }
    }
}
$global:Application = @{
    SetMode          = {}
    SetTool          = {}
    SetKeyword       = {}
    SetTableResource = {}
    SetTableSort     = {}
    SetTitleVisible  = {}
}

# Source CoreFunctions
. "$PSScriptRoot\CoreFunctions.ps1"

# Test [Panels] states
Write-Host "Testing [Panels].Current.Mode"
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
        $global:Application.$p.SetMode( $val )
    }
}

Write-Host "Testing [Panels].Current.Tool"
New-TTState     [Panels].Current.Tool               '[Panels]のツール'              @{
    Default = { Param($id)
        $map = @{
            Library = 'TableKeyword'; Index = 'TableKeyword'; Shelf = 'TableKeyword'; Desk = 'EditorMain'; System = 'EditorKeyword'
        }
        $map[ $id.split('.')[0] ]
    }
    Test    = { Param($id, $val); $val -match '^(Editor|Table|WebView)?(Keyword|Main|toggle)$' }
    Apply   = { Param($id, $val);
        $p = $id.split('.')[0]; $global:Application.$p.SetTool( $val ) }
}

Write-Host "Testing [Panels].Table.Keyword"
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
        $global:Application.$pname.SetKeyword( 'Table', $val )
    }
}

Write-Host "Testing [Panels].Table.Resource"
New-TTState     [Panels].Table.Resource             '[Panels]リソース名'            @{
    Default = { Param($id)
        $map = @{
            Library = 'Thinktank'; Index = 'Status'; Shelf = 'Actions'; Desk = 'Memos'; System = 'Memos'
        }
        $map[ $id.split('.')[0] ]
    }
    Apply   = { Param($id, $val); $p = $id.split('.')[0]; $global:Application.$p.SetTableResource( $val ) }
}

Write-Host "Testing [Panels].Table.Sort"
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
        $global:Application.$pname.SetTableSort( $val )
        $global:Models.Status.SetValue( $id, $val )
    }
}

Write-Host "Testing [Panels].WebView.Keyword"
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
        $global:Application.$pname.SetKeyword( 'WebView', $val )
    }
}
