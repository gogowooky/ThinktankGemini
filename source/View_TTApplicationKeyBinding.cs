using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace ThinktankApp
{
    public abstract class TTApplicationKeyBinding : TTApplicationBase
    {
        protected string _currentExModMode = "";
        protected string _currentKeyInfo = "";
        protected System.Windows.Input.ModifierKeys _triggeringModifiers = System.Windows.Input.ModifierKeys.None;

        public TTApplicationKeyBinding(string xamlPath, string stylePath)
            : base(xamlPath, stylePath)
        {
        }

        public string ExModMode
        {
            get { return _currentExModMode; }
            set
            {
                MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _currentExModMode = value;
                    
                    // Capture current modifiers when setting a non-empty mode
                    if (!string.IsNullOrEmpty(value))
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
        }

        protected override void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            base.OnWindowLoaded(sender, e);
            MainWindow.PreviewKeyDown += OnPreviewKeyDown;
            MainWindow.PreviewKeyUp += OnPreviewKeyUp;
        }

        protected void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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

            if (InvokeActionOnKey(e))
            {
                e.Handled = true;
            }
        }

        protected void OnPreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
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
                ExModMode = "";
            }
        }

        public abstract bool InvokeActionOnKey(System.Windows.Input.KeyEventArgs e);
        protected abstract void RebuildCurrentKeyTable();
        protected override void UpdateStatusBar()
        {
            if (StatusBar != null)
            {
                StatusBar.Items.Clear();
                StatusBar.Items.Add(string.Format("{0} : {1} : {2} : {3}   {4}", _currentPanelName, _currentMode, _currentTool, _currentExModMode, _currentKeyInfo));
            }
        }
    }
}
