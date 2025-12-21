using System;

namespace ThinktankApp
{
    public class TTWebLinks : TTCollection
    {
        public TTWebLinks() : base()
        {
            Description = "Webリンク";
            _itemDisplayColumns = "ID:WebリンクID,Name:Webリンク名,Url:URL";
            _itemNarrowProperties = "ID,Url";
            _itemWideProperties = "Name,ID,Url";
            _itemMinimalProperties = "ID,Url";
            _itemSaveProperties = "ID,Name,Url";
        }
    }
}
