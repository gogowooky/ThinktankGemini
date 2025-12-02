using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ThinktankApp
{
    public class TTCollection : TTObject
    {
        private ObservableCollection<TTObject> _items;
        private int _count;
        private string _description;

        public ObservableCollection<TTObject> Items
        {
            get { return _items; }
            set 
            { 
                if (_items != value)
                {
                    if (_items != null)
                    {
                        _items.CollectionChanged -= Items_CollectionChanged;
                    }
                    _items = value;
                    if (_items != null)
                    {
                        _items.CollectionChanged += Items_CollectionChanged;
                    }
                    OnPropertyChanged();
                    Count = _items != null ? _items.Count : 0;
                }
            }
        }

        public int Count
        {
            get { return _count; }
            set { SetProperty(ref _count, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        protected string _itemSaveProperties = "";
        protected string _itemDisplayColumns = "";
        protected string _itemNarrowProperties = "";
        protected string _itemWideProperties = "";
        protected string _itemMinimalProperties = "";

        public TTCollection()
        {
            Items = new ObservableCollection<TTObject>();
            Description = "Collection";
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Count = Items.Count;
        }

        public TTObject GetItem(string id)
        {
            return Items.FirstOrDefault(i => i.ID == id);
        }

        public void AddItem(TTObject item)
        {
            if (item == null) return;
            item.Parent = this;
            Items.Add(item);
        }

        public void DeleteItem(string id)
        {
            var item = GetItem(id);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public void ClearItems()
        {
            Items.Clear();
        }
    }
}
