
Add-TTEvent     '*-*-*-*'               'Control'           O                       Application.Run.Break

#region === App/Panel
#region A
Add-TTEvent     '*-*-*-*'               'Alt'               A                       Application.Current.ExMode:ExApp
#endregion
#region L/S/I/D/@
Add-TTEvent     '*-*-*-*'               'Alt'               L                       Application.Current.ExMode:ExLibrary
Add-TTEvent     '*-*-*-*'               'Alt'               I                       Application.Current.ExMode:ExIndex
Add-TTEvent     '*-*-*-*'               'Alt'               S                       Application.Current.ExMode:ExShelf
Add-TTEvent     '*-*-*-*'               'Alt'               D                       Application.Current.ExMode:ExDesk
Add-TTEvent     '*-*-*-*'               'Alt'               Oem3                    Application.Current.ExMode:ExSystem          #   @ / `

Add-TTEvent     '*-*-*-*'               'Alt+Shift'         L                       Application.Focus.Panel:Library
Add-TTEvent     '*-*-*-*'               'Alt+Shift'         I                       Application.Focus.Panel:Index
Add-TTEvent     '*-*-*-*'               'Alt+Shift'         S                       Application.Focus.Panel:Shelf
Add-TTEvent     '*-*-*-*'               'Alt+Shift'         D                       Application.Focus.Panel:Desk
Add-TTEvent     '*-*-*-*'               'Alt+Shift'         Oem3                    Application.Focus.Panel:System              #  @ / `

# Add-TTEvent   '*-*-*-*'     'Alt+Shift+Control'       L                   Application.Border.Style:zenLibrary
# Add-TTEvent   '*-*-*-*'     'Alt+Shift+Control'       I                   Application.Border.Style:zenIndex
# Add-TTEvent   '*-*-*-*'     'Alt+Shift+Control'       S                   Application.Border.Style:zenShelf
# Add-TTEvent   '*-*-*-*'     'Alt+Shift+Control'       D                   Application.Border.Style:zenDesk
# Add-TTEvent   '*-*-*-*'     'Alt+Shift+Control'       Oem3                Application.Border.Style:zenSystem              #  @ / `

#endregion
#region Q/E/W
Add-TTEvent     'Panel-*-*-*'           'Alt'               Q                       [Panel].Current.Mode:Table
Add-TTEvent     'Panel-*-*-*'           'Alt'               E                       [Panel].Current.Mode:Editor
Add-TTEvent     'Panel-*-*-*'           'Alt'               W                       [Panel].Current.Mode:WebView
#endregion

#region C
Add-TTEvent     'Panel-Mode-*-*'        'Alt'               C                       [Panel].[Mode].Keyword:''
#endregion

#endregion

#region === ExMode ExApp
#region A
# Add-TTEvent     '*-*-*-ExApp'           'None'          A                   Application.Menu.Visible:toggle
#endregion

#region B/Q
Add-TTEvent     '*-*-*-ExApp'           'None'              B                       Application.Run.Break
Add-TTEvent     '*-*-*-ExApp'           'None'              Q                       Application.Operation.Quit
Add-TTEvent     '*-*-*-ExApp'           'None'              Left                    Application.Window.Screen:prev
Add-TTEvent     '*-*-*-ExApp'           'None'              Right                   Application.Window.Screen:next
Add-TTEvent     '*-*-*-ExApp'           'None'              Up                      Application.Window.State:max
Add-TTEvent     '*-*-*-ExApp'           'None'              Down                    Application.Window.State:norm
#endregion

#endregion

#region === ExMode ExPanel

#region Q/E/W
Add-TTEvent     '*-*-*-ExPanel'         'None'              Q                       [ExPanel].Current.Mode:Table
Add-TTEvent     '*-*-*-ExPanel'         'None'              E                       [ExPanel].Current.Mode:Editor
Add-TTEvent     '*-*-*-ExPanel'         'None'              W                       [ExPanel].Current.Mode:WebView
#endregion

#endregion

#region === 日付入力
# Add-TTEvent  '*-Editor-*-*'           'Alt'               T               Date.Insert.Date
# Add-TTEvent  '*-*-*-ExDate'           'None'              Y               ExDate.Advance.Year
# ...
#endregion