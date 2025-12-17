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
    public class TTApplication : TTApplicationResource
    {
        public TTActions Actions { get; private set; }
        public TTStatus Status { get; private set; }
        public TTModels Models { get; private set; }
        public TTWindow Window { get; private set; }

        private Runspace _runspace;

        public TTApplication(string xamlPath, string stylePath, string panelXamlPath, string scriptDir)
            : base(xamlPath, stylePath, scriptDir)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            Models = new TTModels();
            Actions = Models.Actions;
            Status = Models.Status;
            Window = new TTWindow(MainWindow);

            InitializePanels(panelXamlPath, stylePath);

            InitializePowerShell(scriptDir);
        }

        protected override void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            base.OnWindowLoaded(sender, e);
            if (_runspace != null)
            {
                try
                {
                    using (PowerShell ps = PowerShell.Create())
                    {
                        ps.Runspace = _runspace;
                        ps.AddScript("Initialize-TTStatus");
                        ps.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(MainWindow, "Error initializing TTStatus: " + ex.Message);
                }
            }
        }

        private void InitializePowerShell(string scriptDir)
        {
            // Ensure PowerShell Runspace is created on the UI Thread to prevent deadlocks
            if (MainWindow != null && MainWindow.Dispatcher != null)
            {
                MainWindow.Dispatcher.Invoke(new Action(() => 
                {
                    InitializePowerShellInternal(scriptDir);
                }));
            }
            else
            {
                // Fallback (likely running on UI thread anyway if MainWindow is not set yet or something weird)
                InitializePowerShellInternal(scriptDir);
            }
        }

        private void InitializePowerShellInternal(string scriptDir)
        {
            try
            {
                _runspace = RunspaceFactory.CreateRunspace();
                _runspace.ApartmentState = System.Threading.ApartmentState.STA;
                _runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;
                _runspace.Open();
                
                // Expose TTApplication instance as $global:Application
                _runspace.SessionStateProxy.SetVariable("global:Application", this);
                
                // Expose TTModels instance as $global:Models
                _runspace.SessionStateProxy.SetVariable("global:Models", Models);

                // Set RootPath and ScriptPath
                _runspace.SessionStateProxy.SetVariable("global:RootPath", BaseDir);
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
                            System.Windows.MessageBox.Show(MainWindow, errorMsg);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(MainWindow, "CoreFunctions.ps1 not found at: " + coreScriptPath);
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
                            System.Windows.MessageBox.Show(MainWindow, errorMsg);
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
                            System.Windows.MessageBox.Show(MainWindow, errorMsg);
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
                            System.Windows.MessageBox.Show(MainWindow, errorMsg);
                        }
                    }
                    else
                    {
                         System.Windows.MessageBox.Show(MainWindow, "DefaultActions.ps1 not found at: " + scriptPath);
                    }

                    // Apply all default statuses
                    ps.AddScript(@"
                        $global:Application.Status.Items | ForEach-Object {
                            Apply-TTState $_.ID 'Default' $Env:Computername
                        }
                    ");
                    ps.Invoke();
                }

                IsInitializing = false;

                // Apply initial focus if CurrentPanel was set during initialization
                if (!string.IsNullOrEmpty(CurrentPanel))
                {
                    Focus(CurrentPanel, "", "");
                }

                // Setup KeyTables after loading all scripts and states
                SetupKeyTables();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(MainWindow, "Error initializing PowerShell: " + ex.Message);
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
                        panel.Mode = "Table";
                        break;
                    case "Index": 
                        Index = panel; 
                        panel.Mode = "Table";
                        break;
                    case "Shelf": 
                        Shelf = panel; 
                        panel.Mode = "Table";
                        break;
                    case "Desk": 
                        Desk = panel; 
                        panel.Mode = "Table";
                        break;
                    case "System": 
                        SystemPanel = panel; 
                        panel.Mode = "WebView";
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

        protected override void RebuildCurrentKeyTable()
        {
            CurrentKeyTable = new Dictionary<int, Dictionary<int, TTAction>>();
            _currentKeyTableTag = string.Format("{0}-{1}-{2}-{3}", _currentPanelName, _currentMode, _currentTool, _currentExMode);

            // Temporary storage for best matches: Modifier -> Key -> ActionMatch
            var bestMatches = new Dictionary<int, Dictionary<int, ActionMatch>>();

            foreach (var item in Models.Events.Items)
            {
                TTEvent evnt = item as TTEvent;
                if (evnt == null) continue;

                // Parse Context: Panel-Mode-Tool-ExMode
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
                
                // Strict ExMode matching:
                if (!string.IsNullOrEmpty(_currentExMode))
                {
                     if (!string.Equals(pExMode, _currentExMode, StringComparison.OrdinalIgnoreCase))
                     {
                         continue;
                     }
                }
                else
                {
                    if (!MatchContext(pExMode, _currentExMode)) continue;
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

                // Apply ExMode triggering modifiers if applicable
                if (!string.IsNullOrEmpty(_currentExMode) && 
                    !string.IsNullOrEmpty(pExMode) && 
                    pExMode != "*" &&
                    pExMode.Equals(_currentExMode, StringComparison.OrdinalIgnoreCase))
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
            if (string.IsNullOrEmpty(value)) return false; 
            return string.Equals(pattern, value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool InvokeActionOnKey(System.Windows.Input.KeyEventArgs e)
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
            
            if (action != null)
            {
                _lastActionId = action.ID;
            }
            else
            {
                _lastActionId = "";
            }

            UpdateStatusBar();

            if (action != null)
            {
                // Invoke Action
                var args = new System.Collections.Hashtable();
                args.Add("UIMods", System.Windows.Input.Keyboard.Modifiers);
                args.Add("UIKey", e.Key.ToString());
                args.Add("UIIntKey", intk);

                bool result = action.Invoke(args, _runspace);

                // If we are in ExMode, check return value to decide persistence
                if (!string.IsNullOrEmpty(_currentExMode))
                {
                    if (!result)
                    {
                        ExMode = "";
                    }
                    return true; // Always handle the key in ExMode if action matched
                }

                return result;
            }

            return false;
        }

        public TTPanel ExFdPanel()
        {
            foreach (var key in PanelMap.Keys)
            {
                if (ExMode.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return PanelMap[key];
                }
            }
            return GetFdPanel();
        }
    }
}
