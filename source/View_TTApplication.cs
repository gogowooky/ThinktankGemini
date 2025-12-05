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

        public string BaseDirectory { get; private set; }

        // Dictionary to access panels by name
        public Dictionary<string, TTPanel> PanelMap { get; private set; }

        public string CurrentPanelName { get { return _currentPanelName; } }
        private string _currentPanelName = "";
        private string _currentMode = "";
        private string _currentTool = "";
        private string _currentExModMode = ""; // Added ExModMode
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
                _runspace.ApartmentState = System.Threading.ApartmentState.STA;
                _runspace.ThreadOptions = PSThreadOptions.ReuseThread;
                _runspace.Open();
                
                // Expose TTApplication instance as $global:Application
                _runspace.SessionStateProxy.SetVariable("global:Application", this);
                
                // Expose TTModels instance as $global:Models
                _runspace.SessionStateProxy.SetVariable("global:Models", Models);

                // Set RootPath and ScriptPath
                string rootDir = Path.GetDirectoryName(scriptDir); // Assuming scriptDir is .../ThinktankApp/script
                BaseDirectory = rootDir;
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
                MainWindow.PreviewKeyUp += OnPreviewKeyUp;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(string.Format("Error loading TTApplication: {0}\n{1}", ex.Message, ex.StackTrace));
                throw;
            }
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var key = (e.Key == System.Windows.Input.Key.System) ? e.SystemKey : e.Key;

            // Ignore modifier keys themselves
            if (key == System.Windows.Input.Key.LeftCtrl || key == System.Windows.Input.Key.RightCtrl ||
                key == System.Windows.Input.Key.LeftAlt || key == System.Windows.Input.Key.RightAlt ||
                key == System.Windows.Input.Key.LeftShift || key == System.Windows.Input.Key.RightShift ||
                key == System.Windows.Input.Key.LWin || key == System.Windows.Input.Key.RWin)
            {
                return;
            }

            // Console.WriteLine("KeyDown: " + key);
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
                StatusBar.Items.Add(string.Format("{0} : {1} : {2} : {3}   {4}", _currentPanelName, _currentMode, _currentTool, _currentExModMode, _currentKeyInfo));
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

            RebuildCurrentKeyTable();
        }

        public TTPanel GetFdPanel()
        {
            if (string.IsNullOrEmpty(_currentPanelName)) return null;
            if (PanelMap.ContainsKey(_currentPanelName))
            {
                return PanelMap[_currentPanelName];
            }
            return null;
        }

        private System.Windows.Input.ModifierKeys _triggeringModifiers = System.Windows.Input.ModifierKeys.None;

        public void SetExModMode(string mode)
        {
            MainWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                _currentExModMode = mode;
                
                // Capture current modifiers when setting a non-empty mode
                if (!string.IsNullOrEmpty(mode))
                {
                    _triggeringModifiers = System.Windows.Input.Keyboard.Modifiers;
                }
                else
                {
                    _triggeringModifiers = System.Windows.Input.ModifierKeys.None;
                    _currentKeyInfo = "";
                }

                RebuildCurrentKeyTable();
                UpdateStatusBar();
            }));
        }

        private void OnPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Console.WriteLine("KeyUp: " + e.Key + " SystemKey: " + e.SystemKey + " Modifiers: " + System.Windows.Input.Keyboard.Modifiers);

            if (_triggeringModifiers == System.Windows.Input.ModifierKeys.None) return;

            // Resolve the key (handle System keys like Alt+A where Key is System but SystemKey is A)
            var key = (e.Key == System.Windows.Input.Key.System) ? e.SystemKey : e.Key;

            // Check if the released key corresponds to one of the triggering modifiers
            bool modifierReleased = false;

            if ((_triggeringModifiers & System.Windows.Input.ModifierKeys.Alt) != 0 && 
                (key == System.Windows.Input.Key.LeftAlt || key == System.Windows.Input.Key.RightAlt))
            {
                modifierReleased = true;
            }
            else if ((_triggeringModifiers & System.Windows.Input.ModifierKeys.Control) != 0 && 
                     (key == System.Windows.Input.Key.LeftCtrl || key == System.Windows.Input.Key.RightCtrl))
            {
                modifierReleased = true;
            }
            else if ((_triggeringModifiers & System.Windows.Input.ModifierKeys.Shift) != 0 && 
                     (key == System.Windows.Input.Key.LeftShift || key == System.Windows.Input.Key.RightShift))
            {
                modifierReleased = true;
            }
            else if ((_triggeringModifiers & System.Windows.Input.ModifierKeys.Windows) != 0 && 
                     (key == System.Windows.Input.Key.LWin || key == System.Windows.Input.Key.RWin))
            {
                modifierReleased = true;
            }

            if (modifierReleased)
            {
                // Console.WriteLine("Modifier released. Clearing ExModMode.");
                SetExModMode("");
            }
        }

        public void Show()
        {
            MainWindow.ShowDialog();
        }

        public void Close()
        {
            MainWindow.Dispatcher.BeginInvoke(new Action(() => MainWindow.Close()));
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

        // Current KeyTable: Modifier(int) -> Key(int) -> TTAction
        private Dictionary<int, Dictionary<int, TTAction>> CurrentKeyTable = null;
        private string _currentKeyTableTag = "";

        // Helper class to store action with its specificity score
        private class ActionMatch
        {
            public TTAction Action;
            public int Score;
        }

        public void SetupKeyTables()
        {
            RebuildCurrentKeyTable();
        }

        private void RebuildCurrentKeyTable()
        {
            CurrentKeyTable = new Dictionary<int, Dictionary<int, TTAction>>();
            _currentKeyTableTag = string.Format("{0}-{1}-{2}-{3}", _currentPanelName, _currentMode, _currentTool, _currentExModMode);

            // Temporary storage for best matches: Modifier -> Key -> ActionMatch
            var bestMatches = new Dictionary<int, Dictionary<int, ActionMatch>>();

            foreach (var item in Models.Events.Items)
            {
                TTEvent evnt = item as TTEvent;
                if (evnt == null) continue;

                // Parse Context: Panel-Mode-Tool-ExModMode
                string[] parts = evnt.Context.Split('-');
                if (parts.Length != 4) continue; 

                string pPanel = parts[0];
                string pMode = parts[1];
                string pTool = parts[2];
                string pExMode = parts[3];

                // Check Match
                if (!MatchContext(pPanel, _currentPanelName)) continue;
                if (!MatchContext(pMode, _currentMode)) continue;
                if (!MatchContext(pTool, _currentTool)) continue;
                
                // Strict ExModMode matching:
                // If we are in an ExModMode, the event MUST explicitly specify that mode.
                // Wildcards (*) are not allowed for ExModMode when it is active.
                if (!string.IsNullOrEmpty(_currentExModMode))
                {
                     if (!string.Equals(pExMode, _currentExModMode, StringComparison.OrdinalIgnoreCase))
                     {
                         continue;
                     }
                }
                else
                {
                    if (!MatchContext(pExMode, _currentExModMode)) continue;
                }

                // Calculate Score (higher is better)
                int score = 0;
                if (pPanel != "*") score += 1;
                if (pMode != "*") score += 1;
                if (pTool != "*") score += 1;
                if (pExMode != "*") score += 1;

                string actionID = evnt.Name;
                TTAction action = (TTAction)Actions.GetItem(actionID);
                if (action == null) continue;

                // Parse Modifier
                int intm = 0;
                if (!string.IsNullOrEmpty(evnt.Mods) && evnt.Mods != "None")
                {
                    System.Windows.Input.ModifierKeys modKey;
                    string modStr = evnt.Mods.Replace("None", "").Replace("+", ",").Trim();
                    if (Enum.TryParse(modStr, out modKey))
                    {
                        intm = (int)modKey;
                    }
                }

                // Apply ExModMode triggering modifiers if applicable
                // If the event targets the current ExModMode specifically (not wildcard),
                // add the triggering modifiers of the current ExModMode to the expected modifiers.
                if (!string.IsNullOrEmpty(_currentExModMode) && 
                    !string.IsNullOrEmpty(pExMode) && 
                    pExMode != "*" &&
                    pExMode.Equals(_currentExModMode, StringComparison.OrdinalIgnoreCase))
                {
                    intm |= (int)_triggeringModifiers;
                }

                // Parse Key
                int intk = 0;
                System.Windows.Input.Key key;
                if (Enum.TryParse(evnt.Key, out key))
                {
                    intk = (int)key;
                }
                else
                {
                    continue; 
                }

                // Register if better score
                if (!bestMatches.ContainsKey(intm))
                {
                    bestMatches[intm] = new Dictionary<int, ActionMatch>();
                }

                if (!bestMatches[intm].ContainsKey(intk) || bestMatches[intm][intk].Score < score)
                {
                    bestMatches[intm][intk] = new ActionMatch { Action = action, Score = score };
                }
            }

            // Convert bestMatches to CurrentKeyTable
            foreach (var modKvp in bestMatches)
            {
                CurrentKeyTable[modKvp.Key] = new Dictionary<int, TTAction>();
                foreach (var keyKvp in modKvp.Value)
                {
                    CurrentKeyTable[modKvp.Key][keyKvp.Key] = keyKvp.Value.Action;
                }
            }
            
            UpdateStatusBar();
        }

        private bool MatchContext(string pattern, string value)
        {
            if (pattern == "*") return true;
            // Handle empty value matching only if pattern is empty (unlikely with * usage) or *
            if (string.IsNullOrEmpty(value)) return false; 
            return string.Equals(pattern, value, StringComparison.OrdinalIgnoreCase);
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
            var key = (e.Key == System.Windows.Input.Key.System) ? e.SystemKey : e.Key;
            string keyStr = key.ToString();
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

                bool result = action.Invoke(args, _runspace);

                // If we are in ExModMode, check return value to decide persistence
                if (!string.IsNullOrEmpty(_currentExModMode))
                {
                    if (!result)
                    {
                        SetExModMode("");
                    }
                    return true; // Always handle the key in ExModMode if action matched
                }

                return result;
            }

            return false;
        }
    }
}
