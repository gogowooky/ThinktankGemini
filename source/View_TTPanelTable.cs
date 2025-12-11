using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.ComponentModel;
using ICSharpCode.AvalonEdit;

namespace ThinktankApp
{
    public class TTPanelTable : TTPanelEditor
    {
        public DockPanel TablePanel { get; private set; }
        public TextEditor TableKeyword { get; private set; }
        public DataGrid TableMain { get; private set; }
        public string TableResource { get; private set; }
        
        protected TTCollection _currentCollection;
        private DispatcherTimer _resizeTimer;

        public TTPanelTable(string name, string xamlPath, string stylePath, TTModels models) 
            : base(name, xamlPath, stylePath, models)
        {
            _resizeTimer = new DispatcherTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(100);
            _resizeTimer.Tick += (s, e) =>
            {
                _resizeTimer.Stop();
                UpdateTableColumns();
            };
        }

        protected override void LoadView(string xamlPath, string stylePath)
        {
            base.LoadView(xamlPath, stylePath);
            if (View != null)
            {
                TablePanel = (DockPanel)View.FindName("TablePanel");
                TableKeyword = (TextEditor)View.FindName("TableKeyword");
                TableMain = (DataGrid)View.FindName("TableMain");
            }
        }

        public override void Setup()
        {
            base.Setup();

            if (TableMain != null) 
            {
                TableMain.GotFocus += (s, e) => OnFocusChanged("Table", "Main");
                TableMain.SizeChanged += (s, e) => 
                {
                    _resizeTimer.Stop();
                    _resizeTimer.Start();
                };
            }
            if (TableKeyword != null) 
            {
                TableKeyword.GotFocus += (s, e) => OnFocusChanged("Table", "Keyword");
                TableKeyword.TextChanged += (s, e) => UpdateTableFilter();
            }
        }

        public void UpdateTableFilter()
        {
            if (TableMain == null || TableMain.ItemsSource == null) return;

            string filterText = TableKeyword != null ? TableKeyword.Text : "";
            System.Windows.Data.CollectionView view = (System.Windows.Data.CollectionView)System.Windows.Data.CollectionViewSource.GetDefaultView(TableMain.ItemsSource);
            
            if (string.IsNullOrWhiteSpace(filterText))
            {
                view.Filter = (obj) =>
                {
                    var item = obj as TTObject;
                    if (item == null) return false;
                    if (item is TTAction && ((TTAction)item).IsHidden) return false;
                    return true;
                };
            }
            else
            {
                var orGroups = filterText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                
                view.Filter = (obj) =>
                {
                    var item = obj as TTObject;
                    if (item == null) return false;
                    
                    // Check IsHidden
                    if (item is TTAction && ((TTAction)item).IsHidden) return false;

                    // OR logic: if any group matches, return true
                    foreach (var group in orGroups)
                    {
                        var andKeywords = group.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        bool groupMatch = true;

                        // AND logic: all keywords in group must match
                        foreach (var keyword in andKeywords)
                        {
                            if (!item.Matches(keyword.Trim()))
                            {
                                groupMatch = false;
                                break;
                            }
                        }

                        if (groupMatch) return true;
                    }

                    return false;
                };
            }
        }

        public override void SetFontSize(string size)
        {
            base.SetFontSize(size);
            double fontSize;
            if (double.TryParse(size, out fontSize))
            {
                if (TableMain != null) TableMain.FontSize = fontSize;
            }
        }

        public void SetTableResource(string resourceId)
        {
            TableResource = resourceId;
            if (Models != null)
            {
                if (_currentCollection != null)
                {
                    _currentCollection.PropertyChanged -= OnCollectionPropertyChanged;
                }

                var collection = Models.GetCollection(resourceId);
                _currentCollection = collection;

                if (_currentCollection != null)
                {
                    _currentCollection.PropertyChanged += OnCollectionPropertyChanged;
                }
                
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
                                TableMain.ItemsSource = new System.Windows.Data.ListCollectionView(collection.Items);
                                UpdateTableFilter(); // Ensure filter is applied
                                // UpdateTitle() is in TTPanel, but we can't call it easily from here if it's not virtual/abstract.
                                // However, TTPanel controls the title. 
                                // We might need an event or virtual method for TitleUpdate?
                                // For now, let's assume TTPanel handles Title updates via other means or we make UpdateTitle virtual.
                                OnTableResourceSet();
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

        public override string GetMode()
        {
            if (TablePanel != null && TablePanel.Visibility == Visibility.Visible) return "Table";
            return base.GetMode();
        }

        protected override string GetTitleSuffix()
        {
            string s = base.GetTitleSuffix();
            if (GetMode() == "Table" && !string.IsNullOrEmpty(TableResource))
            {
                s += " [" + TableResource + "]";
                if (_currentCollection != null)
                {
                    s += " (" + _currentCollection.Count + ")";
                }
            }
            return s;
        }

        protected virtual void OnTableResourceSet() 
        {
            UpdateTitle();
        }

        protected void UpdateTableColumns()
        {
            if (TableMain == null || _currentCollection == null) return;

            string propertiesCsv = _currentCollection.GetDisplayProperties(TableMain.ActualWidth);
            if (string.IsNullOrEmpty(propertiesCsv)) return;

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

        protected virtual void OnCollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
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
            if (string.IsNullOrEmpty(sortInfo) || TableMain == null) return;

            var parts = sortInfo.Split('|');
            if (parts.Length != 2) return;

            string propertyName = parts[0];
            string directionStr = parts[1];
            ListSortDirection direction = directionStr.Equals("Ascending", StringComparison.OrdinalIgnoreCase) ? ListSortDirection.Ascending : ListSortDirection.Descending;

            TableMain.Items.SortDescriptions.Clear();
            TableMain.Items.SortDescriptions.Add(new SortDescription(propertyName, direction));
        }

        public void SetColumnHeaderVisible(string visible)
        {
            if (TableMain == null) return;
            bool isVisible = visible.ToLower() == "true";
            TableMain.HeadersVisibility = isVisible ? DataGridHeadersVisibility.Column : DataGridHeadersVisibility.None;
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
    }
}
