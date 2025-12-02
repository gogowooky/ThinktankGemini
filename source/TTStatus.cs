using System;
using System.Linq;

namespace ThinktankApp
{
    public class TTStatus : TTCollection
    {
        public TTStatus() : base()
        {
            Description = "Status";
            _itemDisplayColumns = "Name:名前,Value:設定";
            _itemNarrowProperties = "Name,Value";
            _itemWideProperties = "Name,Value";
            _itemMinimalProperties = "Name,Value";
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
