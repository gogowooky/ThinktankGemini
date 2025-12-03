using System;
using System.Linq;

namespace ThinktankApp
{
    public class TTStatus : TTCollection
    {
        public TTStatus() : base()
        {
            Description = "Status";
            _itemDisplayColumns = "ID:ステータスID,Name:名前,Value:設定";
            _itemNarrowProperties = "ID,Value";
            _itemWideProperties = "Name,ID,Value";
            _itemMinimalProperties = "ID,Value";
            _itemSaveProperties = "ID,Name,Value";
        }

        public void SetValue(string id, string value)
        {
            var item = GetItem(id) as TTState;
            if (item != null)
            {
                if (item.Value != value)
                {
                    item.Value = value;
                    // NotifyUpdated logic could be added here if needed
                }
            }
        }
    }
}
