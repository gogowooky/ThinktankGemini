
Add-TTEvent     '*-*-*-*'             'Control'           O                  Application.Run.Break

#region === App/Panel
#region A
Add-TTEvent     '*-*-*-*'             'Alt'           A                   Application.Current.ExMode:ExApp
#endregion
#region L/S/I/D/@
Add-TTEvent     '*-*-*-*'             'Alt'           L                   Application.Current.ExMode:ExLibrary
Add-TTEvent     '*-*-*-*'             'Alt'           I                   Application.Current.ExMode:ExIndex
Add-TTEvent     '*-*-*-*'             'Alt'           S                   Application.Current.ExMode:ExShelf
Add-TTEvent     '*-*-*-*'             'Alt'           D                   Application.Current.ExMode:ExDesk
Add-TTEvent     '*-*-*-*'             'Alt'           ImeProcessed        Application.Current.ExMode:ExSystem          #   @ / `

# Add-TTEvent     App             'Alt+Shift'     L                   Application.Focus.Panel:Library
# ... (Other focus actions need to be defined or mapped)
#endregion

#region === ExModMode ExApp
#region A
# Add-TTEvent     '*-*-*-ExApp'           'None'          A                   Application.Menu.Visible:toggle
#endregion
#region B/Q
Add-TTEvent     '*-*-*-ExApp'           'None'          B                   Application.Run.Break
Add-TTEvent     '*-*-*-ExApp'           'None'          Q                   Application.Operation.Quit
#endregion
#endregion

#region === ExModMode ExPanel
#region Q/E/W
# Add-TTEvent     '*-*-*-ExPanel'         'None'          Q                   [ExPanel].Current.Mode:Table
# Add-TTEvent     '*-*-*-ExPanel'         'None'          E                   [ExPanel].Current.Mode:Editor
# Add-TTEvent     '*-*-*-ExPanel'         'None'          W                   [ExPanel].Current.Mode:WebView
#endregion
#endregion

#region === 日付入力
# Add-TTEvent  '*-Editor-*-*'           'Alt'               T               Date.Insert.Date
# Add-TTEvent  '*-*-*-ExDate'           'None'              Y               ExDate.Advance.Year
# ...
#endregion