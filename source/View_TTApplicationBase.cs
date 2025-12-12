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
    public abstract class TTApplicationBase
    {
        public Window MainWindow { get; private set; }
        public List<TTPanel> Panels { get; protected set; }
        public System.Windows.Controls.Primitives.StatusBar StatusBar { get; private set; }
        public System.Windows.Controls.Menu Menu { get; private set; }

        // Dictionary to access panels by name
        public Dictionary<string, TTPanel> PanelMap { get; protected set; }

        public string CurrentPanel
        {
            get { return _currentPanelName; }
            set { Focus(value, "", ""); }
        }
        protected string _currentPanelName = "";

        public string CurrentMode { get { return _currentMode; } }
        protected string _currentMode = "";

        public string CurrentTool { get { return _currentTool; } }
        protected string _currentTool = "";
        public static TTApplicationBase Current { get; private set; }
        public bool IsInitializing { get; set; }

        public TTApplicationBase(string xamlPath, string stylePath)
        {
            IsInitializing = true;
            Current = this;
            Panels = new List<TTPanel>();
            PanelMap = new Dictionary<string, TTPanel>();
            LoadWindow(xamlPath, stylePath);
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

                MainWindow.Loaded += OnWindowLoaded;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(MainWindow, string.Format("Error loading TTApplication: {0}\n{1}", ex.Message, ex.StackTrace));
                throw;
            }
        }

        protected virtual void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption = "Message", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            if (MainWindow != null)
            {
                if (MainWindow != null && MainWindow.CheckAccess())
                {
                    return System.Windows.MessageBox.Show(MainWindow, messageBoxText, caption, button, icon);
                }
                else
                {
                    // Call from background thread: Do NOT use Dispatcher.Invoke to avoid deadlock if UI thread is blocked.
                    // Also do not use MainWindow as owner to avoid InvalidOperationException.
                    return System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
                }
            }
            else
            {
                // Fallback for when MainWindow is null (e.g. startup error) - might still need Dispatcher if application exists?
                // But usually if MainWindow is null, we might be early or app is closing.
                // Safest to just show it, or check Application.Current.Dispatcher if possible.
                if (System.Windows.Application.Current != null && System.Windows.Application.Current.Dispatcher != null && !System.Windows.Application.Current.Dispatcher.CheckAccess())
                {
                     return (MessageBoxResult)System.Windows.Application.Current.Dispatcher.Invoke(new Func<MessageBoxResult>(() =>
                    {
                        return System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
                    }));
                }
                return System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);
            }
        }

        protected virtual void UpdateStatusBar()
        {
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

        public void Show()
        {
            MainWindow.ShowDialog();
        }

        public void Close()
        {
            MainWindow.Dispatcher.BeginInvoke(new Action(() => MainWindow.Close()));
        }

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

        public string Title
        {
            get 
            {
                if (MainWindow.Dispatcher.CheckAccess())
                    return MainWindow.Title;
                else
                    return (string)MainWindow.Dispatcher.Invoke(new Func<string>(() => MainWindow.Title));
            }
            set
            {
                if (MainWindow.Dispatcher.CheckAccess())
                    MainWindow.Title = value;
                else
                    MainWindow.Dispatcher.Invoke(new Action(() => MainWindow.Title = value));
            }
        }

        public void Focus(TTPanel panel)
        {
            panel.Focus("", "");
        }

        public void Focus(string panelName, string mode, string tool)
        {
            if (PanelMap.ContainsKey(panelName))
            {
                var panel = PanelMap[panelName];
                panel.Focus(mode, tool);
            }
        }
    }
}
