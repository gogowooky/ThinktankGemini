using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using ICSharpCode.AvalonEdit;
using Microsoft.Web.WebView2.Wpf;
using System.Windows.Threading;

using System.ComponentModel;

namespace ThinktankApp
{
    public class TTPanel
    {
        public string Name { get; private set; }
        public UserControl View { get; private set; }
        
        public Label Title { get; private set; }
        public ContextMenu Menu { get; private set; }

        public DockPanel EditorPanel { get; private set; }
        public TextEditor EditorKeyword { get; private set; }
        public TextEditor EditorMain { get; private set; }

        public DockPanel TablePanel { get; private set; }
        public TextEditor TableKeyword { get; private set; }
        public DataGrid TableMain { get; private set; }

        public DockPanel WebViewPanel { get; private set; }
        public TextEditor WebViewKeyword { get; private set; }
        public WebView2 WebViewMain { get; private set; }

        public TTModels Models { get; private set; }
        public string TableResource { get; private set; }

        private DispatcherTimer _resizeTimer;

        public TTPanel(string name, string xamlPath, string stylePath, TTModels models)
        {
            Name = name;
            Models = models;
            LoadView(xamlPath, stylePath);

            _resizeTimer = new DispatcherTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(100);
            _resizeTimer.Tick += (s, e) =>
            {
                _resizeTimer.Stop();
                UpdateTableColumns();
            };
        }

        private void LoadView(string xamlPath, string stylePath)
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
                    View = (UserControl)XamlReader.Load(xmlReader);
                }

                // Find controls
                Title = (Label)View.FindName("Title");
                Menu = View.ContextMenu;

                EditorPanel = (DockPanel)View.FindName("EditorPanel");
                EditorKeyword = (TextEditor)View.FindName("EditorKeyword");
                EditorMain = (TextEditor)View.FindName("EditorMain");

                TablePanel = (DockPanel)View.FindName("TablePanel");
                TableKeyword = (TextEditor)View.FindName("TableKeyword");
                TableMain = (DataGrid)View.FindName("TableMain");

                WebViewPanel = (DockPanel)View.FindName("WebViewPanel");
                WebViewKeyword = (TextEditor)View.FindName("WebViewKeyword");
                WebViewMain = (WebView2)View.FindName("WebViewMain");

                // Set Title
                if (Title != null)
                {
                    Title.Content = Name;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error loading TTPanel '{0}': {1}\n{2}", Name, ex.Message, ex.StackTrace));
                throw;
            }
        }

        public event Action<string, string, string> FocusChanged;

        public void Setup()
        {
            // Configure EditorMain
            if (EditorMain != null)
            {
                EditorMain.ShowLineNumbers = true;
                EditorMain.WordWrap = true;
                EditorMain.FontFamily = new System.Windows.Media.FontFamily("Meiryo");
                EditorMain.FontSize = 12;
                EditorMain.GotFocus += (s, e) => OnFocusChanged("Editor", "Main");
            }

            // Configure EditorKeyword
            if (EditorKeyword != null)
            {
                EditorKeyword.ShowLineNumbers = false;
                EditorKeyword.WordWrap = false;
                EditorKeyword.FontFamily = new System.Windows.Media.FontFamily("Meiryo");
                EditorKeyword.FontSize = 12;
                EditorKeyword.GotFocus += (s, e) => OnFocusChanged("Editor", "Keyword");
            }

            if (TableMain != null) 
            {
                TableMain.GotFocus += (s, e) => OnFocusChanged("Table", "Main");
                TableMain.SizeChanged += (s, e) => 
                {
                    _resizeTimer.Stop();
                    _resizeTimer.Start();
                };
            }
            if (TableKeyword != null) TableKeyword.GotFocus += (s, e) => OnFocusChanged("Table", "Keyword");

            if (WebViewMain != null) WebViewMain.GotFocus += (s, e) => OnFocusChanged("WebView", "Main");
            if (WebViewKeyword != null) WebViewKeyword.GotFocus += (s, e) => OnFocusChanged("WebView", "Keyword");
        }

        private void OnFocusChanged(string mode, string tool)
        {
            if (FocusChanged != null)
            {
                FocusChanged(Name, mode, tool);
            }
        }

        private bool _isFocused = false;
        private TTCollection _currentCollection;

        private void UpdateTitle()
        {
            if (Title == null) return;

            string titleText = Name;
            string mode = GetMode();

            if (mode == "Table" && !string.IsNullOrEmpty(TableResource))
            {
                titleText += " [" + TableResource + "]";
                if (_currentCollection != null)
                {
                    titleText += " (" + _currentCollection.Count + ")";
                }
            }

            if (_isFocused)
            {
                titleText = "●" + titleText;
            }

            Title.Content = titleText;
        }

        public void SetFocusIndicator(bool isFocused)
        {
            _isFocused = isFocused;
            UpdateTitle();
        }

        public void Focus(string mode, string tool)
        {
            SetMode(mode);
            SetTool(tool);
        }

        public void SetMode(string mode)
        {
            if (string.IsNullOrEmpty(mode)) return;

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

        public string GetMode()
        {
            if (EditorPanel != null && EditorPanel.Visibility == Visibility.Visible) return "Editor";
            if (TablePanel != null && TablePanel.Visibility == Visibility.Visible) return "Table";
            if (WebViewPanel != null && WebViewPanel.Visibility == Visibility.Visible) return "WebView";
            return "";
        }

        public void SetTool(string tool)
        {
            if (string.IsNullOrEmpty(tool)) return;

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
            switch (mode.ToLower())
            {
                case "editor":
                    if (EditorKeyword != null) EditorKeyword.Text = keyword;
                    break;
                case "table":
                    if (TableKeyword != null) TableKeyword.Text = keyword;
                    break;
                case "webview":
                    if (WebViewKeyword != null) WebViewKeyword.Text = keyword;
                    if (WebViewMain != null && !string.IsNullOrEmpty(keyword) && (keyword.StartsWith("http://") || keyword.StartsWith("https://")))
                    {
                        try { WebViewMain.Source = new Uri(keyword); } catch { }
                    }
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


        public void SetTableResource(string resourceId)
        {
            TableResource = resourceId;
            if (Models != null)
            {
                // Unsubscribe from previous collection
                if (_currentCollection != null)
                {
                    _currentCollection.PropertyChanged -= OnCollectionPropertyChanged;
                }

                var collection = Models.GetCollection(resourceId);
                _currentCollection = collection;

                // Subscribe to new collection
                if (_currentCollection != null)
                {
                    _currentCollection.PropertyChanged += OnCollectionPropertyChanged;
                }
                
                // Ensure UI updates happen on the UI thread using View's Dispatcher asynchronously to avoid deadlock
                if (View != null)
                {
                    View.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (collection != null && TableMain != null)
                            {
                                TableMain.AutoGenerateColumns = false;
                                UpdateTableColumns();
                                TableMain.ItemsSource = collection.Items;
                                UpdateTitle();
                            }
                            else
                            {
                                string reason = "";
                                if (collection == null) 
                                {
                                    string available = "";
                                    foreach(var item in Models.Items) { available += item.ID + ", "; }
                                    reason += "Collection is null. Available: " + available + ". ";
                                }
                                if (TableMain == null) reason += "TableMain is null.";
                                System.Console.WriteLine("Collection NOT found or TableMain is null for " + resourceId + " in " + Name + ". Reason: " + reason);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine("Error in SetTableResource: " + ex.Message + "\n" + ex.StackTrace);
                        }
                    }));
                }
            }
        }

        private void UpdateTableColumns()
        {
            if (TableMain == null || _currentCollection == null) return;

            string propertiesCsv = _currentCollection.GetDisplayProperties(TableMain.ActualWidth);
            if (string.IsNullOrEmpty(propertiesCsv)) return;

            // Check if columns need update to avoid flickering
            // Simple check: compare property names
            bool needsUpdate = false;
            var props = propertiesCsv.Split(',');
            if (TableMain.Columns.Count != props.Length)
            {
                needsUpdate = true;
            }
            else
            {
                for (int i = 0; i < props.Length; i++)
                {
                    var col = TableMain.Columns[i] as System.Windows.Controls.DataGridTextColumn;
                    var binding = col != null ? col.Binding as System.Windows.Data.Binding : null;
                    if (binding == null || binding.Path.Path != props[i].Trim())
                    {
                        needsUpdate = true;
                        break;
                    }
                }
            }

            if (needsUpdate)
            {
                TableMain.Columns.Clear();
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    var pName = prop.Trim();
                    var header = _currentCollection.GetColumnHeader(pName);
                    
                    var col = new System.Windows.Controls.DataGridTextColumn();
                    col.Header = header;
                    col.Binding = new System.Windows.Data.Binding(pName);

                    if (i == props.Length - 1)
                    {
                        col.Width = new System.Windows.Controls.DataGridLength(1, System.Windows.Controls.DataGridLengthUnitType.Star);
                    }

                    TableMain.Columns.Add(col);
                }
            }
        }

        private void OnCollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Count")
            {
                if (View != null)
                {
                    View.Dispatcher.BeginInvoke(new Action(() => UpdateTitle()));
                }
            }
        }

        public void SetTableSort(string sortInfo)
        {
            // Format: "PropertyName|Direction" (e.g., "ID|Descending")
            if (string.IsNullOrEmpty(sortInfo) || TableMain == null) return;

            var parts = sortInfo.Split('|');
            if (parts.Length != 2) return;

            string propertyName = parts[0];
            string directionStr = parts[1];
            ListSortDirection direction = directionStr.Equals("Ascending", StringComparison.OrdinalIgnoreCase) ? ListSortDirection.Ascending : ListSortDirection.Descending;

            TableMain.Items.SortDescriptions.Clear();
            TableMain.Items.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }

        public void SetTitleVisible(string visible)
        {
            if (Title == null) return;
            bool isVisible = visible.ToLower() == "true";
            Title.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetKeywordVisible(string visible)
        {
            bool isVisible = visible.ToLower() == "true";
            Visibility v = isVisible ? Visibility.Visible : Visibility.Collapsed;

            if (EditorKeyword != null) EditorKeyword.Visibility = v;
            if (TableKeyword != null) TableKeyword.Visibility = v;
            if (WebViewKeyword != null) WebViewKeyword.Visibility = v;
        }

        public void SetColumnHeaderVisible(string visible)
        {
            if (TableMain == null) return;
            bool isVisible = visible.ToLower() == "true";
            TableMain.HeadersVisibility = isVisible ? DataGridHeadersVisibility.Column : DataGridHeadersVisibility.None;
            // Note: If RowHeader is also visible, it should be All. Simplified for now.
            if (GetRowHeaderVisible() == "true" && isVisible) TableMain.HeadersVisibility = DataGridHeadersVisibility.All;
            else if (GetRowHeaderVisible() == "true") TableMain.HeadersVisibility = DataGridHeadersVisibility.Row;
        }

        public string GetColumnHeaderVisible()
        {
            if (TableMain == null) return "false";
            return (TableMain.HeadersVisibility == DataGridHeadersVisibility.Column || TableMain.HeadersVisibility == DataGridHeadersVisibility.All) ? "true" : "false";
        }

        public void SetRowHeaderVisible(string visible)
        {
            if (TableMain == null) return;
            bool isVisible = visible.ToLower() == "true";
            // Simplified logic
            if (GetColumnHeaderVisible() == "true" && isVisible) TableMain.HeadersVisibility = DataGridHeadersVisibility.All;
            else if (isVisible) TableMain.HeadersVisibility = DataGridHeadersVisibility.Row;
            else if (GetColumnHeaderVisible() == "true") TableMain.HeadersVisibility = DataGridHeadersVisibility.Column;
            else TableMain.HeadersVisibility = DataGridHeadersVisibility.None;
        }

        public string GetRowHeaderVisible()
        {
            if (TableMain == null) return "false";
            return (TableMain.HeadersVisibility == DataGridHeadersVisibility.Row || TableMain.HeadersVisibility == DataGridHeadersVisibility.All) ? "true" : "false";
        }

        public void SetFontSize(string size)
        {
            double fontSize;
            if (double.TryParse(size, out fontSize))
            {
                if (EditorMain != null) EditorMain.FontSize = fontSize;
                if (TableMain != null) TableMain.FontSize = fontSize;
                // WebView font size cannot be easily set from here without injecting JS
            }
        }
    }
}
