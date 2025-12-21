using System;

namespace ThinktankApp
{
    public class TTEvents : TTCollection
    {
        public TTEvents() : base()
        {
            Description = "Events";
            _itemDisplayColumns = "Context:コンテキスト,Mods:修飾キー,Key:キー,Name:コマンド,ID:イベントID";
            _itemNarrowProperties = "Name,ID";
            _itemWideProperties = "Context,Mods,Key,Name";
            _itemMinimalProperties = "Name,ID";
            _itemSaveProperties = "Context,Mods,Key,Name";
        }
    }
}
