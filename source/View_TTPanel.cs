using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using ICSharpCode.AvalonEdit;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Windows.Threading;
using System.Windows.Input;
using System.ComponentModel;

namespace ThinktankApp
{

    public class TTPanel : TTPanelWebView
    {


        public TTPanel(string name, string xamlPath, string stylePath, TTModels models) 
            : base(name, xamlPath, stylePath, models)
        {
        }

        public static readonly DependencyProperty TTPanelProperty =
            DependencyProperty.RegisterAttached("TTPanel", typeof(TTPanel), typeof(TTPanel), new PropertyMetadata(null));

        public static void SetTTPanel(UIElement element, TTPanel value)
        {
            element.SetValue(TTPanelProperty, value);
        }

        public static TTPanel GetTTPanel(UIElement element)
        {
            return (TTPanel)element.GetValue(TTPanelProperty);
        }

        protected override void OnKeywordEnter(string mode, string text)
        {
            SetKeyword(mode, text);
        }

        public void Focus()
        {
            Tool = "Main";
        }

        public override void Focus(string mode, string tool)
        {
            string targetMode = string.IsNullOrEmpty(mode) ? _currentPanelMode : mode;
            string targetTool = string.IsNullOrEmpty(tool) ? _currentPanelTool : tool;

            Mode = targetMode;
            Tool = targetTool;
        }

        public override string Mode
        {
            get { return base.Mode; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                base.Mode = value;

                if (EditorPanel != null) EditorPanel.Visibility = Visibility.Collapsed;
                if (TablePanel != null) TablePanel.Visibility = Visibility.Collapsed;
                if (WebViewPanel != null) WebViewPanel.Visibility = Visibility.Collapsed;

                switch (value.ToLower())
                {
                    case "editor":
                        if (EditorPanel != null) EditorPanel.Visibility = Visibility.Visible;
                        break;
                    case "table":
                        if (TablePanel != null) TablePanel.Visibility = Visibility.Visible;
                        break;
                    case "webview":
                        if (WebViewPanel != null) WebViewPanel.Visibility = Visibility.Visible;
                        break;
                }
                UpdateTitle();
            }
        }

        public override string Tool
        {
            get { return base.Tool; }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                base.Tool = value;
                
                if (TTApplicationBase.Current != null && TTApplicationBase.Current.IsInitializing) return;

                if (View != null)
                {
                    View.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                    {
                        string tool = value; // capture for closure
                        if (EditorPanel != null && EditorPanel.Visibility == Visibility.Visible)
                        {
                            if (tool.ToLower().EndsWith("keyword") && EditorKeyword != null) 
                            {
                                EditorKeyword.Focus();
                            }
                            else if (tool.ToLower().EndsWith("main") && EditorMain != null) 
                            {
                                EditorMain.Focus();
                            }
                        }
                        else if (TablePanel != null && TablePanel.Visibility == Visibility.Visible)
                        {
                            if (tool.ToLower().EndsWith("keyword") && TableKeyword != null) 
                            {
                                TableKeyword.Focus();
                            }
                            else if (tool.ToLower().EndsWith("main") && TableMain != null) 
                            {
                                TableMain.Focus();
                            }
                        }
                        else if (WebViewPanel != null && WebViewPanel.Visibility == Visibility.Visible)
                        {
                            if (tool.ToLower().EndsWith("keyword") && WebViewKeyword != null) 
                            {
                                WebViewKeyword.Focus();
                            }
                            else if (tool.ToLower().EndsWith("main") && WebViewMain != null) 
                            {
                                WebViewMain.Focus();
                            }
                        }
                    }));
                }
            }
        }

        public void SetKeyword(string mode, string keyword)
        {
            if (View != null && !View.Dispatcher.CheckAccess())
            {
                View.Dispatcher.BeginInvoke(new Action(() => SetKeyword(mode, keyword)));
                return;
            }

            TextEditor editor = null;
            switch (mode.ToLower())
            {
                case "editor": editor = EditorKeyword; break;
                case "table": editor = TableKeyword; break;
                case "webview": editor = WebViewKeyword; break;
            }

            if (editor != null)
            {
                var document = editor.Document;
                bool found = false;
                foreach (var line in document.Lines)
                {
                    if (document.GetText(line) == keyword)
                    {
                        editor.CaretOffset = line.Offset;
                        editor.ScrollToLine(line.LineNumber);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var currentLine = document.GetLineByOffset(editor.CaretOffset);
                    int insertOffset = currentLine.EndOffset;
                    // If not empty document and we are appending, ensure newline
                    string textToInsert = (document.TextLength > 0 ? "\n" : "") + keyword;
                    document.Insert(insertOffset, textToInsert);
                    editor.CaretOffset = insertOffset + textToInsert.Length; 
                    editor.ScrollToLine(document.GetLineByOffset(editor.CaretOffset).LineNumber);
                }
            }

            if (mode.ToLower() == "webview")
            {
                NavigateWebView(keyword);
            }
        }

        public string GetKeyword(string mode)
        {
            TextEditor editor = null;
            switch (mode.ToLower())
            {
                case "editor": editor = EditorKeyword; break;
                case "table": editor = TableKeyword; break;
                case "webview": editor = WebViewKeyword; break;
            }

            if (editor != null)
            {
                var line = editor.Document.GetLineByOffset(editor.CaretOffset);
                return editor.Document.GetText(line);
            }
            return "";
        }

        public void SetKeywordVisible(string visible)
        {
            bool isVisible = visible.ToLower() == "true";
            Visibility v = isVisible ? Visibility.Visible : Visibility.Collapsed;

            if (EditorKeyword != null) EditorKeyword.Visibility = v;
            if (TableKeyword != null) TableKeyword.Visibility = v;
            if (WebViewKeyword != null) WebViewKeyword.Visibility = v;
        }


    }
}
