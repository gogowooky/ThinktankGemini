using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms; // For Screen class
using System.Windows.Controls.Primitives; // For StatusBar
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace ThinktankApp
{
    public class TTApplication
    {
        public Window MainWindow { get; private set; }
        public List<TTPanel> Panels { get; private set; }
        public System.Windows.Controls.Primitives.StatusBar StatusBar { get; private set; }
        public System.Windows.Controls.Menu Menu { get; private set; }

        // Dictionary to access panels by name
        public Dictionary<string, TTPanel> PanelMap { get; private set; }

        private string _currentPanelName = "";
        private string _currentMode = "";
        private string _currentTool = "";
        private string _currentKeyInfo = "";

        public TTActions Actions { get; private set; }
        public TTStatus Status { get; private set; }
        public TTModels Models { get; private set; }
        private Runspace _runspace;

        public TTApplication(string xamlPath, string stylePath, string panelXamlPath, string scriptDir)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Panels = new List<TTPanel>();
            PanelMap = new Dictionary<string, TTPanel>();
            
            Models = new TTModels();
            Actions = Models.Actions;
            Status = Models.Status;

            LoadWindow(xamlPath, stylePath);
            InitializePanels(panelXamlPath, stylePath);
            InitializePowerShell(scriptDir);
        }

        private void InitializePowerShell(string scriptDir)
        {
            try
            {
                _runspace = RunspaceFactory.CreateRunspace();
                _runspace.Open();
                
                // Expose TTApplication instance as $global:Application
                _runspace.SessionStateProxy.SetVariable("global:Application", this);
                
                // Expose TTModels instance as $global:Models
                _runspace.SessionStateProxy.SetVariable("global:Models", Models);

                // Set RootPath and ScriptPath
                string rootDir = Path.GetDirectoryName(scriptDir); // Assuming scriptDir is .../ThinktankApp/script
                _runspace.SessionStateProxy.SetVariable("global:RootPath", rootDir);
                _runspace.SessionStateProxy.SetVariable("global:ScriptPath", scriptDir);

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = _runspace;

                    // Load CoreFunctions.ps1
                    string coreScriptPath = Path.Combine(scriptDir, "CoreFunctions.ps1");
                    if (File.Exists(coreScriptPath))
                    {
                        ps.Commands.Clear();
                        ps.AddScript(File.ReadAllText(coreScriptPath));
                        ps.Invoke();
                        
                        if (ps.HadErrors)
                        {
                            string errorMsg = "Errors loading CoreFunctions.ps1:\n";
                            foreach (var error in ps.Streams.Error)
                            {
                                errorMsg += error.ToString() + "\n";
                            }
                            System.Windows.MessageBox.Show(errorMsg);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("CoreFunctions.ps1 not found at: " + coreScriptPath);
                    }

                    // Load DefaultEvents.ps1
                    string eventsScriptPath = Path.Combine(scriptDir, "DefaultEvents.ps1");
                    if (File.Exists(eventsScriptPath))
                    {
                        ps.Commands.Clear();
                        ps.AddScript(File.ReadAllText(eventsScriptPath));
                        ps.Invoke();
                        
                        if (ps.HadErrors)
                        {
                            string errorMsg = "Errors loading DefaultEvents.ps1:\n";
                            foreach (var error in ps.Streams.Error)
                            {
                                errorMsg += error.ToString() + "\n";
                            }
                            System.Windows.MessageBox.Show(errorMsg);
                        }
                    }

                    // Load DefaultStatus.ps1
                    string statusScriptPath = Path.Combine(scriptDir, "DefaultStatus.ps1");
                    if (File.Exists(statusScriptPath))
                    {
                        ps.Commands.Clear();
                        ps.AddScript(File.ReadAllText(statusScriptPath));
                        ps.Invoke();
                        
                        if (ps.HadErrors)
                        {
                            string errorMsg = "Errors loading DefaultStatus.ps1:\n";
                            foreach (var error in ps.Streams.Error)
                            {
                                errorMsg += error.ToString() + "\n";
                            }
                            System.Windows.MessageBox.Show(errorMsg);
                        }
                    }

                    // Load DefaultActions.ps1
                    string scriptPath = Path.Combine(scriptDir, "DefaultActions.ps1");
                    
                    if (File.Exists(scriptPath))
                    {
                        ps.Commands.Clear();
                        ps.AddScript(File.ReadAllText(scriptPath));
                        ps.Invoke();

                        if (ps.HadErrors)
                        {
                            string errorMsg = "Errors loading DefaultActions.ps1:\n";
                            foreach (var error in ps.Streams.Error)
                            {
                                errorMsg += error.ToString() + "\n";
                            }
                            System.Windows.MessageBox.Show(errorMsg);
                        }
                    }
                    else
                    {
                         System.Windows.MessageBox.Show("DefaultActions.ps1 not found at: " + scriptPath);
                    }

                    // Apply all default statuses
                    // System.Windows.MessageBox.Show("Applying default statuses. Count: " + Models.Status.Count);
                    ps.AddScript(@"
                        $global:Application.Status.Items | ForEach-Object {
                            # Write-Host ""Applying $($_.ID)""
                            Apply-TTState $_.ID 'Default' $Env:Computername
                        }
                    ");
                    ps.Invoke();
                    // System.Windows.MessageBox.Show("Finished applying default statuses.");
                }

                // Setup KeyTables after loading all scripts and states
                SetupKeyTables();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error initializing PowerShell: " + ex.Message);
            }
        }

        private void LoadWindow(string xamlPath, string stylePath)
        {
            try
            {
                string xamlContent = File.ReadAllText(xamlPath);
                string styleContent = File.ReadAllText(stylePath);
                
                // Replace ResourceDictionary with style content
                xamlContent = xamlContent.Replace("<ResourceDictionary Source=\"Style.xaml\" />", styleContent);

                using (var sr = new StringReader(xamlContent))
                using (var xmlReader = XmlReader.Create(sr))
                {
                    MainWindow = (Window)XamlReader.Load(xmlReader);
                }

                StatusBar = (System.Windows.Controls.Primitives.StatusBar)MainWindow.FindName("StatusBar");
                Menu = (System.Windows.Controls.Menu)MainWindow.FindName("Menu");

                MainWindow.PreviewKeyDown += OnPreviewKeyDown;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Error loading TTApplication: {0}\n{1}", ex.Message, ex.StackTrace));
                throw;
            }
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Ignore modifier keys themselves
            if (e.Key == System.Windows.Input.Key.LeftCtrl || e.Key == System.Windows.Input.Key.RightCtrl ||
                e.Key == System.Windows.Input.Key.LeftAlt || e.Key == System.Windows.Input.Key.RightAlt ||
                e.Key == System.Windows.Input.Key.LeftShift || e.Key == System.Windows.Input.Key.RightShift ||
                e.Key == System.Windows.Input.Key.LWin || e.Key == System.Windows.Input.Key.RWin)
            {
                return;
            }

            if (InvokeActionOnKey(e))
            {
                e.Handled = true;
            }
        }

        private void UpdateStatusBar()
        {
            if (StatusBar != null)
            {
                StatusBar.Items.Clear();
                StatusBar.Items.Add(string.Format("{0} : {1} : {2}   {3}", _currentPanelName, _currentMode, _currentTool, _currentKeyInfo));
            }
        }

        public TTPanel Library { get; private set; }
        public TTPanel Index { get; private set; }
        public TTPanel Shelf { get; private set; }
        public TTPanel Desk { get; private set; }
        public TTPanel SystemPanel { get; private set; }

        private void InitializePanels(string panelXamlPath, string stylePath)
        {
            string[] panelNames = { "Library", "Index", "Shelf", "Desk", "System" };
            foreach (var name in panelNames)
            {
                var panel = new TTPanel(name, panelXamlPath, stylePath, Models);
                panel.Setup();
                Panels.Add(panel);
                PanelMap[name] = panel;

                switch (name)
                {
                    case "Library": 
                        Library = panel; 
                        panel.SetMode("Table");
                        break;
                    case "Index": 
                        Index = panel; 
                        panel.SetMode("Table");
                        break;
                    case "Shelf": 
                        Shelf = panel; 
                        panel.SetMode("Table");
                        break;
                    case "Desk": 
                        Desk = panel; 
                        panel.SetMode("Editor");
                        break;
                    case "System": 
                        SystemPanel = panel; 
                        panel.SetMode("WebView");
                        break;
                }

                // Add to grid
                var grid = (Grid)MainWindow.FindName(name + "Grid");
                if (grid != null)
                {
                    grid.Children.Add(panel.View);
                }

                // Subscribe to FocusChanged event
                panel.FocusChanged += OnPanelFocusChanged;
            }
        }

        private void OnPanelFocusChanged(string panelName, string mode, string tool)
        {
            // Update title indicators
            foreach (var p in Panels)
            {
                p.SetFocusIndicator(p.Name == panelName);
            }

            _currentPanelName = panelName;
            _currentMode = mode;
            _currentTool = tool;
            _currentKeyInfo = ""; // Reset key info on focus change
            UpdateStatusBar();

            // Switch KeyTable based on focus
            // Construct tag: PanelName + Mode + Tool (simplified) or just PanelName + ToolName if available
            // Reference logic: $kt_tag = $this.FocusedTool.TTPanel.Name + $this.FocusedTool.Name
            // Here we have panelName, mode, tool.
            // Example: Library + Table + Main -> LibraryTableMain
            // Example: Library + Table + Keyword -> LibraryTableKeyword
            string keyTableTag = panelName + mode + tool;
            SwitchKeyTable(keyTableTag);
        }

        public void Show()
        {
            MainWindow.ShowDialog();
        }

        public void Close()
        {
            MainWindow.Close();
        }

        // --- Logic ported from TTApplication.ps1 ---

        public int ChangeScreen(string param)
        {
            var screens = Screen.AllScreens;
            var curScreen = screens.FirstOrDefault(s => s.WorkingArea.Contains((int)MainWindow.Left, (int)MainWindow.Top)) ?? screens[0];
            int curNo = Array.IndexOf(screens, curScreen);

            int targetNo = 0;
            int p;
            if (param == "next")
            {
                targetNo = (curNo + 1) % screens.Length;
            }
            else if (param == "prev")
            {
                targetNo = (curNo + screens.Length - 1) % screens.Length;
            }
            else if (int.TryParse(param, out p))
            {
                targetNo = p % screens.Length;
            }

            var targetScreen = screens[targetNo];
            MainWindow.Left = targetScreen.WorkingArea.X + MainWindow.Left - curScreen.WorkingArea.X;
            MainWindow.Top = targetScreen.WorkingArea.Y + MainWindow.Top - curScreen.WorkingArea.Y;

            return targetNo;
        }

        public string GetBorderPosition(string name)
        {
            double a = -1;
            double b = 100;
            var grid = (Grid)MainWindow.FindName(name + "Grid");

            if (grid == null) return "-1";

            if (new[] { "UserSystem", "LibraryIndex", "ShelfDesk" }.Contains(name))
            {
                a = grid.RowDefinitions[0].ActualHeight;
                b = grid.RowDefinitions[1].ActualHeight;
            }
            else if (name == "User")
            {
                a = grid.ColumnDefinitions[0].ActualWidth;
                b = grid.ColumnDefinitions[1].ActualWidth;
            }
            else
            {
                return "-1";
            }

            int val = (int)(a / (a + b) * 100);
            if (val <= 1) val = 0;
            if (val >= 99) val = 100;
            return val.ToString();
        }

        public void SetBorderPosition(string name, string value)
        {
            int percent;
            int current;
            int v;
            if (int.TryParse(value, out v))
            {
                percent = v;
            }
            else if (value.StartsWith("+") || value.StartsWith("-"))
            {
                if (int.TryParse(GetBorderPosition(name), out current))
                {
                    percent = current + int.Parse(value);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;

            var grid = (Grid)MainWindow.FindName(name + "Grid");
            if (grid == null) return;

            if (new[] { "UserSystem", "LibraryIndex", "ShelfDesk" }.Contains(name))
            {
                grid.RowDefinitions[0].Height = new GridLength(percent, GridUnitType.Star);
                grid.RowDefinitions[1].Height = new GridLength(100 - percent, GridUnitType.Star);
            }
            else if (name == "User")
            {
                grid.ColumnDefinitions[0].Width = new GridLength(percent, GridUnitType.Star);
                grid.ColumnDefinitions[1].Width = new GridLength(100 - percent, GridUnitType.Star);
            }
        }

        public void SetTitle(string title)
        {
            MainWindow.Title = title;
        }

        // --- Focus Management ---

        public void Focus(TTPanel panel)
        {
            Display(panel);
            panel.Focus("", "");
        }

        public void Focus(string panelName, string mode, string tool)
        {
            if (PanelMap.ContainsKey(panelName))
            {
                var panel = PanelMap[panelName];
                // Display(panelName, mode); // Redundant now, handled by panel.Focus
                panel.Focus(mode, tool);
            }
        }

        public void Display(TTPanel panel)
        {
            // Placeholder for future panel visibility logic
        }

    
        public void Display(string panelName, string mode)
        {
             // Placeholder for future panel visibility logic
        }

        // KeyTable Definitions
        private Dictionary<string, string[]> KeyTableTags_EventTags = new Dictionary<string, string[]>
        {
            { "Application",            new[] { "App", "Panel" } },
            { "ExApp",                  new[] { "ExApp" } },
            { "ExDate",                 new[] { "ExDate" } },
            { "ExMenu",                 new[] { "ExMenu" } },
            { "ExLibrary",              new[] { "ExPanel", "ExLibrary" } },
            { "ExIndex",                new[] { "ExPanel", "ExIndex" } },
            { "ExShelf",                new[] { "ExPanel", "ExShelf" } },
            { "ExDesk",                 new[] { "ExPanel", "ExDesk" } },
            { "ExSystem",               new[] { "ExPanel", "ExSystem" } },
            { "LibraryEditorMain",      new[] { "App", "Panel", "Library", "Editor", "EditorMode" } },
            { "LibraryEditorKeyword",   new[] { "App", "Panel", "Library", "Keyword", "EditorMode", "EditorKeyword" } },
            { "LibraryTableMain",       new[] { "App", "Panel", "Library", "TableMode", "Table" } },
            { "LibraryTableKeyword",    new[] { "App", "Panel", "Library", "Keyword", "TableMode", "TableKeyword" } },
            { "LibraryWebViewMain",     new[] { "App", "Panel", "Library", "WebViewMode", "WebView" } },
            { "LibraryWebViewKeyword",  new[] { "App", "Panel", "Library", "Keyword", "WebViewMode", "WebViewKeyword" } },
            { "IndexEditorMain",        new[] { "App", "Panel", "Index", "Editor", "EditorMode" } },
            { "IndexEditorKeyword",     new[] { "App", "Panel", "Index", "Keyword", "EditorMode", "EditorKeyword" } },
            { "IndexTableMain",         new[] { "App", "Panel", "Index", "TableMode", "Table" } },
            { "IndexTableKeyword",      new[] { "App", "Panel", "Index", "Keyword", "TableMode", "TableKeyword" } },
            { "IndexWebViewMain",       new[] { "App", "Panel", "Index", "WebViewMode", "WebView" } },
            { "IndexWebViewKeyword",    new[] { "App", "Panel", "Index", "Keyword", "WebViewMode", "WebViewKeyword" } },
            { "ShelfEditorMain",        new[] { "App", "Panel", "Shelf", "Editor", "EditorMode" } },
            { "ShelfEditorKeyword",     new[] { "App", "Panel", "Shelf", "Keyword", "EditorMode", "EditorKeyword" } },
            { "ShelfTableMain",         new[] { "App", "Panel", "Shelf", "TableMode", "Table" } },
            { "ShelfTableKeyword",      new[] { "App", "Panel", "Shelf", "Keyword", "TableMode", "TableKeyword" } },
            { "ShelfWebViewMain",       new[] { "App", "Panel", "Shelf", "WebViewMode", "WebView" } },
            { "ShelfWebViewKeyword",    new[] { "App", "Panel", "Shelf", "Keyword", "WebViewMode", "WebViewKeyword" } },
            { "DeskEditorMain",         new[] { "App", "Panel", "Desk", "Editor", "EditorMode" } },
            { "DeskEditorKeyword",      new[] { "App", "Panel", "Desk", "Keyword", "EditorMode", "EditorKeyword" } },
            { "DeskTableMain",          new[] { "App", "Panel", "Desk", "TableMode", "Table" } },
            { "DeskTableKeyword",       new[] { "App", "Panel", "Desk", "Keyword", "TableMode", "TableKeyword" } },
            { "DeskWebViewMain",        new[] { "App", "Panel", "Desk", "WebViewMode", "WebView" } },
            { "DeskWebViewKeyword",     new[] { "App", "Panel", "Desk", "Keyword", "WebViewMode", "WebViewKeyword" } },
            { "SystemEditorMain",       new[] { "App", "Panel", "System", "Editor", "EditorMode" } },
            { "SystemEditorKeyword",    new[] { "App", "Panel", "System", "Keyword", "EditorMode", "EditorKeyword" } },
            { "SystemTableMain",        new[] { "App", "Panel", "System", "TableMode", "Table" } },
            { "SystemTableKeyword",     new[] { "App", "Panel", "System", "Keyword", "TableMode", "TableKeyword" } },
            { "SystemWebViewMain",      new[] { "App", "Panel", "System", "WebViewMode", "WebView" } },
            { "SystemWebViewKeyword",   new[] { "App", "Panel", "System", "Keyword", "WebViewMode", "WebViewKeyword" } }
        };

        // KeyTableTags -> Modifier(int) -> Key(int) -> TTAction
        private Dictionary<string, Dictionary<int, Dictionary<int, TTAction>>> KeyTableTags_Actions = new Dictionary<string, Dictionary<int, Dictionary<int, TTAction>>>();
        
        // Current KeyTable: Modifier(int) -> Key(int) -> TTAction
        private Dictionary<int, Dictionary<int, TTAction>> CurrentKeyTable = null;
        private string _currentKeyTableTag = "";

        public void SetupKeyTables()
        {
            KeyTableTags_Actions.Clear();

            foreach (var item in Models.Events.Items)
            {
                TTEvent evnt = item as TTEvent;
                if (evnt != null)
                {
                    string tag = evnt.Tag;
                    string modsStr = evnt.Mods;
                    string keyStr = evnt.Key;
                    string actionID = evnt.Name;

                    TTAction action = (TTAction)Actions.GetItem(actionID);
                    if (action == null) continue;

                    // Parse Modifier
                    int intm = 0;
                    if (!string.IsNullOrEmpty(modsStr) && modsStr != "None")
                    {
                        System.Windows.Input.ModifierKeys modKey;
                        if (Enum.TryParse(modsStr.Replace("None", "").Trim(), out modKey))
                        {
                            intm = (int)modKey;
                        }
                    }

                    // Parse Key
                    int intk = 0;
                    // Handle special keys if needed (Left1, WheelPlus etc as per reference)
                    // For now, standard keys
                    System.Windows.Input.Key key;
                    if (Enum.TryParse(keyStr, out key))
                    {
                        intk = (int)key;
                    }
                    else
                    {
                        // Fallback for special keys logic from reference if needed
                        continue; 
                    }

                    foreach (var kvp in KeyTableTags_EventTags)
                    {
                        string keyTableTag = kvp.Key;
                        string[] eventTags = kvp.Value;

                        if (Array.IndexOf(eventTags, tag) >= 0)
                        {
                            if (!KeyTableTags_Actions.ContainsKey(keyTableTag))
                            {
                                KeyTableTags_Actions[keyTableTag] = new Dictionary<int, Dictionary<int, TTAction>>();
                            }
                            if (!KeyTableTags_Actions[keyTableTag].ContainsKey(intm))
                            {
                                KeyTableTags_Actions[keyTableTag][intm] = new Dictionary<int, TTAction>();
                            }
                            
                            var modDict = KeyTableTags_Actions[keyTableTag][intm];
                            modDict[intk] = action;
                        }
                    }
                }
            }

            // Set initial key table
            SwitchKeyTable("Application");
        }

        public void SwitchKeyTable(string kt_tag)
        {
            if (kt_tag == "focusedtool" && _currentPanelName != "")
            {
                // Construct tag from focused panel/tool
                // Example: Library + EditorMain -> LibraryEditorMain
                // We assume _currentTool holds something like "EditorMain" or "TableKeyword"
                // But currently OnPanelFocusChanged passes "Editor" or "Table" as mode, and "Main" or "Keyword" as tool?
                // Let's check OnPanelFocusChanged logic.
                // It receives: panelName, mode, tool.
                // mode is "Table", "Editor", "WebView".
                // tool is "Main", "Keyword".
                // So we can construct: panelName + mode + tool
                // e.g. LibraryTableMain
                kt_tag = _currentPanelName + _currentMode + _currentTool;
            }

            if (KeyTableTags_Actions.ContainsKey(kt_tag))
            {
                CurrentKeyTable = KeyTableTags_Actions[kt_tag];
                _currentKeyTableTag = kt_tag;
                UpdateStatusBar();
            }
            else
            {
                // Fallback
                CurrentKeyTable = null;
                _currentKeyTableTag = "";
            }
        }

        public bool InvokeActionOnKey(System.Windows.Input.KeyEventArgs e)
        {
            if (CurrentKeyTable == null) return false;

            int intm = (int)System.Windows.Input.Keyboard.Modifiers;
            int intk = (int)(e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key);
            
            TTAction action = null;

            if (CurrentKeyTable.ContainsKey(intm))
            {
                if (CurrentKeyTable[intm].ContainsKey(intk))
                {
                    action = CurrentKeyTable[intm][intk];
                }
            }

            // Update status bar with key info
            string keyStr = e.Key.ToString();
            string modStr = "";
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) != 0) modStr += "Ctrl + ";
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Shift) != 0) modStr += "Shift + ";
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) != 0) modStr += "Alt + ";
            if ((System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Windows) != 0) modStr += "Win + ";
            _currentKeyInfo = modStr + keyStr;
            UpdateStatusBar();

            if (action != null)
            {
                // Invoke Action
                var args = new System.Collections.Hashtable();
                args.Add("UIMods", System.Windows.Input.Keyboard.Modifiers);
                args.Add("UIKey", e.Key.ToString());
                args.Add("UIIntKey", intk);

                return action.Invoke(args, _runspace);
            }

            return false;
        }
    }
}
