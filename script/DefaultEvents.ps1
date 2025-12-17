
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
#endregion

#region Q/E/W
Add-TTEvent     'Panel-*-*-*'           'Alt'               Q                       [Panel].Current.Mode:Table
Add-TTEvent     'Panel-*-*-*'           'Alt'               E                       [Panel].Current.Mode:Editor
Add-TTEvent     'Panel-*-*-*'           'Alt'               W                       [Panel].Current.Mode:WebView
#endregion

#endregion

#region === Keyword

#region C
Add-TTEvent     '*-*-Keyword-*'         'Alt'               C                       ExFdPanel.Keyword.Clear
#endregion

#endregion

#region === ExApp

#region Q
Add-TTEvent     '*-*-*-ExApp'           'None'              Q                       Application.Operation.Quit
#endregion

#region ←/→/↑/↓
Add-TTEvent     '*-*-*-ExApp'           'None'              Left                    Application.Window.Screen:prev
Add-TTEvent     '*-*-*-ExApp'           'None'              Right                   Application.Window.Screen:next
Add-TTEvent     '*-*-*-ExApp'           'None'              Up                      Application.Window.State:max
Add-TTEvent     '*-*-*-ExApp'           'None'              Down                    Application.Window.State:norm
#endregion

#endregion

#region === ExPanel
#region Q/E/W
Add-TTEvent     '*-*-*-ExPanel'         'None'              Q                       [ExPanel].Current.Mode:Table
Add-TTEvent     '*-*-*-ExPanel'         'None'              E                       [ExPanel].Current.Mode:Editor
Add-TTEvent     '*-*-*-ExPanel'         'None'              W                       [ExPanel].Current.Mode:WebView
#endregion

#region C
Add-TTEvent     '*-*-*-ExPanel'         'Alt'               C                       ExFdPanel.Keyword.Clear
#endregion

#endregion

