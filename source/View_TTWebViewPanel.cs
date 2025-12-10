using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;

namespace ThinktankApp
{
    public class TTWebViewPanel : TTTablePanel
    {
        public DockPanel WebViewPanel { get; private set; }
        public TextEditor WebViewKeyword { get; private set; }
        public WebView2 WebViewMain { get; private set; }
        
        private string _pendingUrl = "";

        public TTWebViewPanel(string name, string xamlPath, string stylePath, TTModels models) 
            : base(name, xamlPath, stylePath, models)
        {
        }

        protected override void LoadView(string xamlPath, string stylePath)
        {
            base.LoadView(xamlPath, stylePath);
            if (View != null)
            {
                WebViewPanel = (DockPanel)View.FindName("WebViewPanel");
                WebViewKeyword = (TextEditor)View.FindName("WebViewKeyword");
                WebViewMain = (WebView2)View.FindName("WebViewMain");
            }
        }

        public override void Setup()
        {
            base.Setup();

            if (WebViewMain != null) 
            {
                WebViewMain.GotFocus += (s, e) => OnFocusChanged("WebView", "Main");
                WebViewMain.CoreWebView2InitializationCompleted += (s, e) =>
                {
                    if (!e.IsSuccess)
                    {
                        Console.WriteLine("WebView2 Initialization Failed: " + e.InitializationException.Message);
                    }
                };
                WebViewMain.Loaded += async (s, e) => 
                {
                    try 
                    {
                        string userDataFolder = Path.Combine(Path.GetTempPath(), "ThinktankWebView2");
                        var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                        await WebViewMain.EnsureCoreWebView2Async(env);
                        
                        if (!string.IsNullOrEmpty(_pendingUrl))
                        {
                            WebViewMain.CoreWebView2.Navigate(_pendingUrl);
                            _pendingUrl = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error calling EnsureCoreWebView2Async: " + ex.Message);
                    }
                };
            }
            if (WebViewKeyword != null) 
            {
                WebViewKeyword.GotFocus += (s, e) => OnFocusChanged("WebView", "Keyword");
                WebViewKeyword.PreviewKeyDown += (s, e) => 
                {
                    if (e.Key == Key.Enter)
                    {
                        e.Handled = true;
                        OnKeywordEnter("WebView", WebViewKeyword.Text);
                    }
                };
            }
        }

        protected virtual void OnKeywordEnter(string mode, string text) { }

        public void NavigateWebView(string url)
        {
            string targetUrl = url;
            if (!string.IsNullOrEmpty(targetUrl))
            {
                using (StringReader reader = new StringReader(targetUrl))
                {
                    targetUrl = reader.ReadLine();
                }
            }
            if (targetUrl != null) targetUrl = targetUrl.Trim();

            if (WebViewMain != null && !string.IsNullOrEmpty(targetUrl) && (targetUrl.StartsWith("http://") || targetUrl.StartsWith("https://")))
            {
                try 
                { 
                    if (WebViewMain.CoreWebView2 != null)
                    {
                        WebViewMain.CoreWebView2.Navigate(targetUrl);
                    }
                    else
                    {
                        _pendingUrl = targetUrl;
                    }
                } 
                catch (Exception ex)
                {
                    Console.WriteLine("Error navigating WebView: " + ex.Message);
                }
            }
        }

        public override string GetMode()
        {
            if (WebViewPanel != null && WebViewPanel.Visibility == Visibility.Visible) return "WebView";
            return base.GetMode();
        }
    }
}
