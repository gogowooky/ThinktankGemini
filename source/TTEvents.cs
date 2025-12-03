using System;

namespace ThinktankApp
{
    public class TTEvents : TTCollection
    {
        public TTEvents() : base()
        {
            Description = "Events";
            _itemDisplayColumns = "Tag:タグ,_mods:修飾キー,_key:キー,Name:コマンド,ID:イベントID";
            _itemNarrowProperties = "Name,ID";
            _itemWideProperties = "Tag,_mods,_key,Name,ID";
            _itemMinimalProperties = "Name,ID";
            _itemSaveProperties = "Tag,_mods,_key,Name,ID";
        }
    }
}
