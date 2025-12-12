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

        protected override void OnKeywordEnter(string mode, string text)
        {
            SetKeyword(mode, text);
        }

        public void Focus()
        {
            SetTool("Main");
        }

        public override void Focus(string mode, string tool)
        {
            string targetMode = string.IsNullOrEmpty(mode) ? _currentPanelMode : mode;
            string targetTool = string.IsNullOrEmpty(tool) ? _currentPanelTool : tool;

            SetMode(targetMode);
            SetTool(targetTool);
        }

        public override void SetMode(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return;
            _currentPanelMode = mode;

            if (EditorPanel != null) EditorPanel.Visibility = Visibility.Collapsed;
            if (TablePanel != null) TablePanel.Visibility = Visibility.Collapsed;
            if (WebViewPanel != null) WebViewPanel.Visibility = Visibility.Collapsed;

            switch (mode.ToLower())
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

        public override void SetTool(string tool)
        {
            if (string.IsNullOrEmpty(tool)) return;
            _currentPanelTool = tool;

            if (EditorPanel != null && EditorPanel.Visibility == Visibility.Visible)
            {
                if (tool.ToLower() == "keyword" && EditorKeyword != null) EditorKeyword.Focus();
                else if (tool.ToLower() == "main" && EditorMain != null) EditorMain.Focus();
            }
            else if (TablePanel != null && TablePanel.Visibility == Visibility.Visible)
            {
                if (tool.ToLower() == "keyword" && TableKeyword != null) TableKeyword.Focus();
                else if (tool.ToLower() == "main" && TableMain != null) TableMain.Focus();
            }
            else if (WebViewPanel != null && WebViewPanel.Visibility == Visibility.Visible)
            {
                if (tool.ToLower() == "keyword" && WebViewKeyword != null) WebViewKeyword.Focus();
                else if (tool.ToLower() == "main" && WebViewMain != null) WebViewMain.Focus();
            }
        }

        public void SetKeyword(string mode, string keyword)
        {
            if (View != null && !View.Dispatcher.CheckAccess())
            {
                View.Dispatcher.BeginInvoke(new Action(() => SetKeyword(mode, keyword)));
                return;
            }

            switch (mode.ToLower())
            {
                case "editor":
                    if (EditorKeyword != null) EditorKeyword.Text = keyword;
                    break;
                case "table":
                    if (TableKeyword != null) TableKeyword.Text = keyword;
                    break;
                case "webview":
                    try 
                    {
                        if (WebViewKeyword != null && WebViewKeyword.Text != keyword) 
                        {
                            WebViewKeyword.Text = keyword;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error setting WebViewKeyword.Text: " + ex.Message);
                    }
                    NavigateWebView(keyword);
                    break;
            }
        }

        public string GetKeyword(string mode)
        {
            switch (mode.ToLower())
            {
                case "editor":
                    return EditorKeyword != null ? EditorKeyword.Text : "";
                case "table":
                    return TableKeyword != null ? TableKeyword.Text : "";
                case "webview":
                    return WebViewKeyword != null ? WebViewKeyword.Text : "";
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
