using System;
using System.Linq;

namespace ThinktankApp
{
    public class TTStatus : TTCollection
    {
        public TTStatus() : base()
        {
            Description = "Status";
            _itemDisplayColumns = "ID:ステータスID,Name:名前,Value:設定,From:由来";
            _itemNarrowProperties = "ID,Value";
            _itemWideProperties = "Name,ID,Value,From";
            _itemMinimalProperties = "ID,Value";
            _itemSaveProperties = "ID,Name,Value,From";
        }

        public void SetValue(string id, string value, string from = "")
        {
            var item = GetItem(id) as TTState;
            if (item != null)
            {
                if (item.Value != value || item.From != from)
                {
                    item.Value = value;
                    if (!string.IsNullOrEmpty(from))
                    {
                        item.From = from;
                    }
                }
            }
        }
    }
}
